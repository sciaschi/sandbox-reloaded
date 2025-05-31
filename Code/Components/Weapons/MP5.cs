﻿[Spawnable, Library( "weapon_mp5", Title = "MP5" )]
partial class MP5 : BaseWeapon
{

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		BroadcastAttackPrimary();

		ViewModel?.Renderer?.Set( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		//
		// Shoot the bullets
		//
		ShootBullet( 0.1f, 1.5f, 5.0f, 3.0f );
	}

	[Rpc.Broadcast]
	private void BroadcastAttackPrimary()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		Sound.Play( "rust_smg.shoot", WorldPosition );
	}

	public override void OnControl()
	{
		base.OnControl();

		var attackHold = !IsReloading && Input.Down( "attack1" ) ? 1.0f : 0.0f;

		BroadcastOnControl( attackHold );

		ViewModel?.Renderer?.Set( "attack_hold", attackHold );
	}

	[Rpc.Broadcast]
	private void BroadcastOnControl( float attackHold )
	{
		Owner?.Controller?.Renderer?.Set( "attack_hold", attackHold );
	}

	// TODO: Probably should unify these particle methods + make it work for world models

	protected override void ShootEffects()
	{
		base.ShootEffects();

		AttachParticleSystem( "particles/eject_9mm.prefab", "eject" );
	}
}
