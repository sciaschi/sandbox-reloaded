
[Icon( "➖" )]
[ClassName( "slider" )]
[Group( "Constraints" )]
public class Slider : BaseConstraintToolMode
{
	protected override void CreateConstraint( SelectionPoint point1, SelectionPoint point2 )
	{
		if ( point1.GameObject == point2.GameObject )
			return;

		var axis = Rotation.LookAt( Vector3.Direction( point1.WorldPosition(), point2.WorldPosition() ) );

		var go1 = new GameObject( false, "slider" );
		go1.Parent = point1.GameObject;
		go1.LocalTransform = point1.LocalTransform;
		go1.WorldRotation = axis;

		var go2 = new GameObject( false, "slider" );
		go2.Parent = point2.GameObject;
		go2.LocalTransform = point2.LocalTransform;
		go2.WorldRotation = axis;

		var len = point1.WorldPosition().Distance( point2.WorldPosition() );

		var joint = go1.AddComponent<SliderJoint>();
		joint.Body = go2;
		joint.MinLength = 0;
		joint.MaxLength = len;
		joint.EnableCollision = true;

		var lineRenderer = go1.AddComponent<LineRenderer>();
		lineRenderer.Points = [go1, go2];
		lineRenderer.Width = 0.5f;
		lineRenderer.Color = Color.Black;
		lineRenderer.Lighting = true;
		lineRenderer.CastShadows = true;

		go2.NetworkSpawn();
		go1.NetworkSpawn();
	}
}
