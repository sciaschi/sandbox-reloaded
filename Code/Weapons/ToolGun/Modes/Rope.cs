
[Icon( "🐍" )]
[ClassName( "rope" )]
[Group( "Constraints" )]
public class Rope : BaseConstraintToolMode
{
	[Range( -500, 500 )]
	[Property, Sync]
	public float Slack { get; set; } = 0.0f;

	[Property, Sync]
	public bool Rigid { get; set; } = false;

	public override bool CanConstraintToSelf => true;

	protected override void CreateConstraint( SelectionPoint point1, SelectionPoint point2 )
	{
		var go1 = new GameObject( false, "rope" );
		go1.Parent = point1.GameObject;
		go1.LocalTransform = point1.LocalTransform;
		go1.LocalRotation = Rotation.Identity;

		var go2 = new GameObject( false, "rope" );
		go2.Parent = point2.GameObject;
		go2.LocalTransform = point2.LocalTransform;
		go2.LocalRotation = Rotation.Identity;

		var len = point1.WorldPosition().Distance( point2.WorldPosition() );
		len = MathF.Max( 1.0f, len + Slack );

		//
		// If it's ourself - we want to create the rope, but no joint between
		//
		if ( point1.GameObject != point2.GameObject )
		{
			var joint = go1.AddComponent<SpringJoint>();
			joint.Body = go2;
			joint.MinLength = Rigid ? len : 0;
			joint.MaxLength = len;
			joint.RestLength = len;
			joint.Frequency = 0;
			joint.Damping = 0;
			joint.EnableCollision = true;
		}

		if ( !Rigid )
		{
			var vertletRope = go1.AddComponent<VerletRope>();
			vertletRope.Attachment = go2;
			vertletRope.SegmentCount = Math.Max( 2, MathX.CeilToInt( len / 16.0f ) );
			vertletRope.SegmentLength = (len / vertletRope.SegmentCount);
			vertletRope.ConstraintIterations = 2;
		}

		var lineRenderer = go1.AddComponent<LineRenderer>();
		lineRenderer.Points = [go1, go2];
		lineRenderer.Width = 1f;
		lineRenderer.Color = Color.White;
		lineRenderer.Lighting = true;
		lineRenderer.CastShadows = true;
		lineRenderer.SplineInterpolation = 4;
		lineRenderer.Texturing = lineRenderer.Texturing with { Material = Material.Load( "materials/default/rope01.vmat" ), WorldSpace = true, UnitsPerTexture = 32 };
		lineRenderer.Face = SceneLineObject.FaceMode.Cylinder;

		go2.NetworkSpawn( true, null );
		go1.NetworkSpawn( true, null );
	}
}
