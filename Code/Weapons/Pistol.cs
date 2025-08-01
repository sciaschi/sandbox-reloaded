public class Pistol : BaseBulletWeapon
{
	[Property] public float Damage { get; set; } = 12.0f;
	[Property] public float PrimaryFireRate { get; set; } = 0.15f;

	public override void OnControl( Player player )
	{
		base.OnControl( player );

		float fireRate = PrimaryFireRate;

		if ( IsInputQueued( () => Input.Pressed( "Attack1" ), fireRate ) )
		{
			ShootBullet( player, fireRate );
		}
	}

	public void ShootBullet( Player player, float fireRate )
	{
		if ( !CanShoot() )
		{
			TryAutoReload();
			return;
		}

		AddShootDelay( fireRate );

		var aimConeAmount = GetAimConeAmount();
		var forward = player.EyeTransform.Rotation.Forward.WithAimCone( 0.1f + aimConeAmount * 3f, 0.1f + aimConeAmount * 3f );
		var bulletRadius = 1;

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.WithoutTags( "playercontroller" ) // don't hit playercontroller colliders
							.Radius( bulletRadius )
							.UseHitboxes()
							.Run();

		ShootEffects( tr.EndPosition, tr.Hit, tr.Normal, tr.GameObject, tr.Surface );
		TraceAttack( TraceAttackInfo.From( tr, Damage ) );
		TimeSinceShoot = 0;

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.2f, -0.5f ), Random.Shared.Float( -1, 1 ) * 0.4f, 0 );

		if ( !player.Controller.ThirdPerson && player.IsLocalPlayer )
		{
			_ = new Sandbox.CameraNoise.Recoil( 1f, 0.3f );
		}
	}

	// returns 0 for no aim spread, 1 for full aim cone
	float GetAimConeAmount()
	{
		return TimeSinceShoot.Relative.Remap( 0, 0.5f, 1, 0 );
	}
}
