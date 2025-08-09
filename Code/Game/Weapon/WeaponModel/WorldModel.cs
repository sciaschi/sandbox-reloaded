using static BaseWeapon;

public sealed class WorldModel : WeaponModel, IWeaponEvent
{
	void IWeaponEvent.OnAttack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		if ( e.IsFirstPerson )
			return;

		DoMuzzleEffect();
		DoEjectBrass();
	}
}
