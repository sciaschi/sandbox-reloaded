/// <summary>
/// The local user's preferences in Deathmatch
/// </summary>
public static class GamePreferences
{
	/// <summary>
	/// Enables automatic switching to better weapons on item pickup
	/// </summary>
	[ConVar( "autoswitch", ConVarFlags.UserInfo | ConVarFlags.Saved )]
	public static bool AutoSwitch { get; set; } = true;

	/// <summary>
	/// Enables fast switching between inventory weapons
	/// </summary>
	[ConVar( "fastswitch", ConVarFlags.Saved )]
	public static bool FastSwitch { get; set; } = false;

	/// <summary>
	/// Intensity of your camera's screenshake
	/// </summary>
	[ConVar( "viewbob", ConVarFlags.Saved )]
	[Group( "Camera" )]
	public static bool ViewBobbing { get; set; } = true;

	/// <summary>
	/// Intensity of your camera's screenshake
	/// </summary>
	[ConVar( "screenshake", ConVarFlags.Saved )]
	[Range( 0.1f, 2f ), Step( 0.1f ), Group( "Camera" )]
	public static float Screenshake { get; set; } = 0.3f;
}
