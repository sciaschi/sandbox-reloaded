using System.Threading;

public partial class BaseWeapon
{
	/// <summary>
	/// Should we consume 1 bullet per reload instead of filling the clip?
	/// </summary>
	[Property, Feature( "Ammo" )]
	public bool IncrementalReloading { get; set; } = false;

	/// <summary>
	/// Can we cancel reloads?
	/// </summary>
	[Property, Feature( "Ammo" )]
	public bool CanCancelReload { get; set; } = true;

	private CancellationTokenSource reloadToken;
	private bool isReloading;

	public bool CanReload()
	{
		if ( isReloading ) return false;

		var owner = Owner;
		if ( !owner.IsValid() )
			return false;

		return true;
	}

	public bool IsReloading() => isReloading;

	public virtual void CancelReload()
	{
		if ( reloadToken?.IsCancellationRequested == false )
		{
			reloadToken?.Cancel();
			isReloading = false;
		}
	}

	public virtual async void OnReloadStart()
	{
		if ( !CanReload() )
			return;

		CancelReload();

		try
		{
			reloadToken = new CancellationTokenSource();
			isReloading = true;

			await ReloadAsync( reloadToken.Token );
		}
		finally
		{
			reloadToken?.Dispose();
			reloadToken = null;
		}
	}

	[Rpc.Broadcast]
	private void BroadcastReload()
	{
		if ( !Owner.IsValid() ) return;

		Assert.True( Owner.Controller.IsValid(), "BaseWeapon::BroadcastReload - Player Controller is invalid!" );
		Assert.True( Owner.Controller.Renderer.IsValid(), "BaseWeapon::BroadcastReload - Renderer is invalid!" );

		Owner.Controller.Renderer.Set( "b_reload", true );
	}

	protected virtual async Task ReloadAsync( CancellationToken ct )
	{
		try
		{
			IWeaponEvent.PostToGameObject( ViewModel, x => x.OnReloadStart() );

			BroadcastReload();
		}
		finally
		{
			reloadToken?.Cancel();
			isReloading = false;
		}
	}
}
