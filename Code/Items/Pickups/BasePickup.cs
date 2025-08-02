/// <summary>
/// A weapon, or weapons, or ammo, that can be picked up
/// </summary>
public abstract class BasePickup : Component, Component.ITriggerListener
{
	/// <summary>
	/// Check if the player can pick up this object
	/// </summary>
	public virtual bool CanPickup( Player player, PlayerInventory inventory )
	{
		return true;
	}

	/// <summary>
	/// Give the player the effect of this pickup
	/// </summary>
	/// <returns>Should this object be consumed, eg on successful pickup</returns>
	protected virtual bool OnPickup( Player player, PlayerInventory inventory )
	{
		return true;
	}

	/// <summary>
	/// Called when a gameobject enters the trigger.
	/// </summary>
	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		if ( !Networking.IsHost ) return;
		if ( GameObject.IsDestroyed ) return;

		if ( !other.Components.TryGet( out Player player ) )
			return;

		if ( !player.Components.TryGet( out PlayerInventory inventory ) )
			return;

		if ( !CanPickup( player, inventory ) )
			return;

		if ( !OnPickup( player, inventory ) )
			return;

		DestroyGameObject();
	}
}
