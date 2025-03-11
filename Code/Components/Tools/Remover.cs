[Library( "tool_remover", Description = "Remove entities", Group = "construction" )]
public class Remover : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Component is MapCollider )
				return true;

			Remove( trace.GameObject );

			return true;
		}

		return false;
	}

	[Rpc.Broadcast]
	void Remove( GameObject g )
	{
		// TODO: Fix this for other clients

		g.Destroy();

		Particles.MakeParticleSystem( "particles/physgun_freeze.vpcf", g.WorldTransform );
	}
}
