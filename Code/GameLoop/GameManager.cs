public sealed partial class GameManager( Scene scene ) : GameObjectSystem<GameManager>( scene ), Component.INetworkListener, ISceneStartup
{
	void ISceneStartup.OnHostInitialize()
	{
		if ( !Networking.IsActive && InGame() )
		{
			Networking.CreateLobby( new Sandbox.Network.LobbyConfig() { Privacy = Sandbox.Network.LobbyPrivacy.Public, MaxPlayers = 32, Name = "Sandbox Classic Server", DestroyWhenHostLeaves = true } );
		}
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		channel.CanSpawnObjects = false;

		var playerData = CreatePlayerInfo( channel );
		SpawnPlayer( playerData );
	}

	/// <summary>
	/// Called when someone leaves the server. This will only be called for the host.
	/// </summary>
	void Component.INetworkListener.OnDisconnected( Connection channel )
	{
		var pd = PlayerData.For( channel );
		pd?.GameObject.Destroy();
	}

	private PlayerData CreatePlayerInfo( Connection channel )
	{
		var go = new GameObject( true, $"PlayerInfo - {channel.DisplayName}" );
		var data = go.AddComponent<PlayerData>();
		data.SteamId = (long)channel.SteamId;
		data.PlayerId = channel.Id;
		data.DisplayName = channel.DisplayName;

		go.NetworkSpawn( null );
		go.Network.SetOwnerTransfer( OwnerTransfer.Fixed );

		return data;
	}

	public void SpawnPlayer( Connection connection ) => SpawnPlayer( PlayerData.For( connection ) );

	public void SpawnPlayer( PlayerData playerData )
	{
		Assert.NotNull( playerData, "PlayerData is null" );
		Assert.True( Networking.IsHost, $"Client tried to SpawnPlayer: {playerData.DisplayName}" );

		// does this connection already have a player?
		if ( Scene.GetAll<Player>().Where( x => x.Network.Owner?.Id == playerData.PlayerId ).Any() )
			return;

		// Find a spawn location for this player
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var playerGo = GameObject.Clone( "/prefabs/engine/player.prefab", new CloneConfig { Name = playerData.DisplayName, StartEnabled = false, Transform = startLocation } );

		Log.Info( playerGo );

		var player = playerGo.Components.Get<Player>( true );
		player.PlayerData = playerData;

		var owner = Connection.Find( playerData.PlayerId );
		playerGo.NetworkSpawn( owner );

		IPlayerEvent.PostToGameObject( player.GameObject, x => x.OnSpawned() );
		player.EquipBestWeapon();
	}

	public void SpawnPlayerDelayed( PlayerData playerData )
	{
		GameTask.RunInThreadAsync( async () =>
		{
			await Task.Delay( 4000 );
			await GameTask.MainThread();
			Current?.SpawnPlayer( playerData );
		} );
	}

	/// <summary>
	/// In the editor, spawn the player where they're looking
	/// </summary>
	public static Transform EditorSpawnLocation { get; set; }

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();

		if ( spawnPoints.Length == 0 )
		{
			if ( Application.IsEditor && !EditorSpawnLocation.Position.IsNearlyZero() )
			{
				return EditorSpawnLocation;
			}

			return Transform.Zero;
		}

		var players = Scene.GetAll<Player>();

		if ( !players.Any() )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Find spawnpoint furthest away from any players
		// TODO: in the future we may want a different logic, as spawning far away is not necessarily good.
		// But good enough for now and also reduces chances of players from spawning on top of  or inside each other.
		//
		SpawnPoint spawnPointFurthestAway = null;
		float spawnPointFurthestAwayDistanceSqr = float.MinValue;

		foreach ( var spawnPoint in spawnPoints )
		{
			float closestPlayerDistanceToSpawnpointSqr = float.MaxValue;

			foreach ( var player in players )
			{
				float playerDistanceToSpawnPointSqr = (spawnPoint.Transform.World.Position - player.Transform.World.Position).LengthSquared;

				if ( playerDistanceToSpawnPointSqr < closestPlayerDistanceToSpawnpointSqr )
				{
					closestPlayerDistanceToSpawnpointSqr = playerDistanceToSpawnPointSqr;
				}
			}

			if ( closestPlayerDistanceToSpawnpointSqr > spawnPointFurthestAwayDistanceSqr )
			{
				spawnPointFurthestAwayDistanceSqr = closestPlayerDistanceToSpawnpointSqr;
				spawnPointFurthestAway = spawnPoint;
			}
		}

		return spawnPointFurthestAway.Transform.World;
	}

	/// <summary>
	/// Called on the host when a played is killed
	/// </summary>
	public void OnDeath( Player player, DeathmatchDamageInfo dmg )
	{
		Assert.True( Networking.IsHost );
		Assert.True( player.IsValid(), "Player invalid" );
		Assert.True( player.PlayerData.IsValid(), $"{player.GameObject.Name}'s PlayerData invalid" );

		var weapon = dmg.Weapon;
		var attackerData = PlayerData.For( dmg.InstigatorId );
		bool isSuicide = attackerData == player.PlayerData;

		if ( attackerData.IsValid() && !isSuicide )
		{
			Assert.True( weapon.IsValid(), $"Weapon invalid. (Attacker: {attackerData.DisplayName}, Victim: {player.DisplayName})" );

			attackerData.Kills++;
			attackerData.AddStat( $"kills" );

			if ( weapon.IsValid() )
			{
				attackerData.AddStat( $"kills.{weapon.Name}" );
			}
		}

		player.PlayerData.Deaths++;

		string attackerName = attackerData.IsValid() ? attackerData.DisplayName : dmg.Attacker?.Name ?? "";
		string weaponName = weapon.IsValid() ? weapon.Name : "";
		long attackerSteamId = attackerData.IsValid() ? attackerData.SteamId : 0;

		if ( string.IsNullOrEmpty( attackerName ) )
		{
			// Player died without a clear attacker (environmental, etc.)
			OnKilledMessage( 0, "", player.PlayerData.SteamId, player.DisplayName, "died" );
		}
		else
		{
			// Normal kill
			OnKilledMessage( attackerSteamId, attackerName, player.PlayerData.SteamId, player.DisplayName, weaponName );
		}

		// Log to console (only on host to avoid duplicates)
		if ( string.IsNullOrEmpty( attackerName ) )
			Log.Info( $"{player.DisplayName} died (tags: {dmg.Tags})" );
		else if ( weapon.IsValid() )
			Log.Info( $"{attackerName} killed {(isSuicide ? "self" : player.DisplayName)} with {weapon.Name} (tags: {dmg.Tags})" );
		else
			Log.Info( $"{attackerName} killed {(isSuicide ? "self" : player.DisplayName)} (tags: {dmg.Tags})" );
	}

	/// <summary>
	/// Called clientside from OnDeath on the server to add kill messages to the killfeed.
	/// </summary>
	[Rpc.Broadcast]
	public void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	[ConCmd( "spawn", ConVarFlags.Server )]
	public static async void Spawn( Connection caller, string path_or_ident )
	{
		var player = Player.FindForConnection( caller );
		if ( player is null ) return;

		// store off their eye transform
		var eyes = player.EyeTransform;

		var trace = Game.SceneTrace.Ray( eyes.Position, eyes.Position + eyes.Forward * 200 )
			.IgnoreGameObject( player.GameObject )
			.WithoutTags( "player" )
			.Run();


		var up = trace.Normal;
		var backward = -eyes.Forward;

		var right = Vector3.Cross( up, backward ).Normal;
		var forward = Vector3.Cross( right, up ).Normal;

		var facingAngle = Rotation.LookAt( forward, up );

		var spawnTransform = new Transform( trace.EndPosition, facingAngle );

		// get their player


		// TODO - can this user spawn this package?

		var modelPath = await FindModelPath( path_or_ident );
		if ( string.IsNullOrWhiteSpace( modelPath ) )
		{
			Log.Warning( $"Couldn't find {path_or_ident}" );
			return;
		}

		var model = await Model.LoadAsync( modelPath );
		SpawnModel( model, spawnTransform, player );
	}

	static async Task<string> FindModelPath( string ident_or_path )
	{
		if ( ident_or_path.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) )
			return ident_or_path;

		var package = await Package.FetchAsync( ident_or_path, false );
		if ( package is null ) return null;
		if ( package.TypeName != "model" ) return null;

		await package.MountAsync();

		var modelName = package.GetMeta<string>( "PrimaryAsset" );
		return modelName;
	}

	private static void SpawnModel( Model model, Transform spawnTransform, Player player )
	{
		Log.Info( $"[{player}] Spawning Model {model.Name}" );

		var depth = -model.Bounds.Mins.z;

		spawnTransform.Position += spawnTransform.Up * depth;

		var go = new GameObject( false, "prop" )
		{
			WorldTransform = spawnTransform
		};

		var prop = go.AddComponent<Prop>();
		prop.Model = model;

		if ( (model.Physics?.Parts?.Count ?? 0) == 0 )
		{
			Log.Info( "No physics - adding a cube" );

			var collider = go.AddComponent<BoxCollider>();
			collider.Scale = model.Bounds.Size;
			collider.Center = model.Bounds.Center;


			go.AddComponent<Rigidbody>();
		}

		go.NetworkSpawn( true, null );
	}

	/// <summary>
	/// Checks if the user is currently in the game scene.
	/// </summary>
	/// <returns></returns>
	public static bool InGame()
	{
		var sceneInfo = Game.ActiveScene.GetComponentInChildren<SceneInformation>();
		var title = sceneInfo.Title;

		if ( title != "game" )
			return false;

		return true;
	}
}
