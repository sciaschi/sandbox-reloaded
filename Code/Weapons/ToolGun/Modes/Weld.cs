
[Icon( "🥽" )]
[ClassName( "weld" )]
[Group( "Constraints" )]
public class Weld : BaseConstraintToolMode
{
	[Property, Sync]
	public bool EasyMode { get; set; } = true;

	[Property, Sync]
	public bool Rigid { get; set; } = false;


	public override void OnControl()
	{
		base.OnControl();

		if ( EasyMode && Stage == 1 && IsValidState )
		{
			var select = TraceSelect();
			if ( !select.IsValid() ) return;

			var go = Point1.GameObject.Network.RootGameObject ?? Point1.GameObject;

			var local = GetEasyModePlacement( Point1, select );

			DebugOverlay.GameObject( go, transform: local, color: Color.White.WithAlpha( 0.3f ) );
		}
	}


	protected override void CreateConstraint( SelectionPoint point1, SelectionPoint point2 )
	{
		if ( EasyMode )
		{
			var local = GetEasyModePlacement( point1, point2 );
			var moving = point1.GameObject.Network.RootGameObject ?? point1.GameObject;
			moving.WorldTransform = local;
		}

		var go1 = new GameObject( false, "weld" );
		go1.Parent = point1.GameObject;
		go1.LocalTransform = point1.LocalTransform;
		go1.LocalRotation = Rotation.Identity;

		var go2 = new GameObject( false, "weld" );
		go2.Parent = point2.GameObject;
		go2.LocalTransform = point2.LocalTransform;
		go2.LocalRotation = Rotation.Identity;

		var joint = go1.AddComponent<FixedJoint>();
		joint.Attachment = Joint.AttachmentMode.Auto;
		joint.Body = go2;
		joint.EnableCollision = true;
		joint.AngularFrequency = Rigid ? 0 : 10;
		joint.LinearFrequency = Rigid ? 0 : 10;

		go2.NetworkSpawn();
		go1.NetworkSpawn();
	}
}
