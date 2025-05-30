public static class Particles
{
	public static GameObject CreateParticleSystem( string path, Transform transform, float time = 1, GameObject parent = null )
	{
		SpawnParticleSystem( Connection.Local.Id, path, transform.Position, transform.Rotation, time, parent );

		return MakeParticleSystem( path, transform, time, parent );
	}

	[Rpc.Broadcast(NetFlags.Unreliable)]
	public static void SpawnParticleSystem( Guid connection, string path, Vector3 position, Rotation rotation, float time = 1, GameObject parent = null )
	{
		if ( Connection.Local.Id == connection )
			return;

		MakeParticleSystem( path, new Transform( position, rotation ), time, parent );
	}

	
	public static GameObject MakeParticleSystem( string path, Transform transform, float time = 1, GameObject parent = null )
	{
		var particleSystem = GameObject.GetPrefab( path );

		if ( !particleSystem.IsValid() )
			return null;

		var go = particleSystem.Clone( new CloneConfig
		{
			Name = particleSystem.Name, 
			Parent = parent, 
			Transform = transform,
			StartEnabled = true
		} );

		go.NetworkMode = NetworkMode.Never;

		if ( time > 0 )
			go.DestroyAsync( time );

		return go;
	}
}
