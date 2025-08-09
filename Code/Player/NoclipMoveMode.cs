using Sandbox.Movement;

public sealed class NoclipMoveMode : MoveMode
{
	[Property]
	public float RunSpeed { get; set; } = 10000;

	[Property]
	public float WalkSpeed { get; set; } = 2000;

	protected override void OnUpdateAnimatorState( SkinnedModelRenderer renderer )
	{
		renderer.Set( "b_noclip", true );
	}

	public override int Score( PlayerController controller )
	{
		return 1000;
	}

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.LinearDamping = 5.0f;
		body.AngularDamping = 1f;

		body.Tags.Set( "noclip", true );
	}

	public override void OnModeBegin()
	{
		Controller.IsClimbing = true;
		Controller.Body.Gravity = false;
	}

	public override void OnModeEnd( MoveMode next )
	{
		Controller.IsClimbing = false;
		Controller.Body.Tags.Set( "noclip", false );
		Controller.Renderer.Set( "b_noclip", false );
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// don't normalize, because analog input might want to go slow
		input = input.ClampLength( 1 );

		var direction = eyes * input;

		// Run if we're holding down alt move button
		bool run = Input.Down( "run" );

		// if we're running, use run speed, if not use walk speed
		var velocity = run ? RunSpeed : WalkSpeed;

		if ( Input.Down( "duck" ) )
			direction *= 0.2f;

		if ( Input.Down( "jump" ) )
			direction += Vector3.Up;

		return direction * velocity;
	}
}
