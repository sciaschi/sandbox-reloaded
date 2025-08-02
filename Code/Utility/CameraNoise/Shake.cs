using Sandbox.Utility;

namespace Sandbox.CameraNoise;

class Shake( float amount, float time ) : BaseCameraNoise
{
	readonly float lifeTime = time;
	float deathTime = time;
	readonly float amount = amount * GamePreferences.Screenshake;

	public override bool IsDone => deathTime <= 0;

	Vector3.SpringDamped damping;

	public override void Update()
	{
		deathTime -= Time.Delta;
		damping.Update( Time.Delta );
	}

	public override void ModifyCamera( CameraComponent cc )
	{
		var x = Noise.Perlin( Time.Now * 1000.0f, 2345 );
		var y = Noise.Perlin( Time.Now * 1000.0f, 21 );
		var z = Noise.Perlin( Time.Now * 1000.0f, 865 );

		var delta = MathX.Remap( deathTime, 0, lifeTime, 0, 1 );

		cc.WorldRotation *= new Angles( x, y, z ) * delta * amount;
	}
}
