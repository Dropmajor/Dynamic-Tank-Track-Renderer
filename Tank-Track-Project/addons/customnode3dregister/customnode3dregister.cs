#if TOOLS
using Godot;

//gizmo code comes from https://www.dlab.ninja/2025/06/how-to-implement-gizmos-and-handles-in.html
[Tool]
public partial class customnode3dregister : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		var script = GD.Load<Script>("res://scripts/TrackCurve.cs");
		var texture = GD.Load<Texture2D>("res://assets/icon.svg");
		AddCustomType("TrackCurve", "Node3D", script, texture);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("TrackCurve");
	}
}
#endif
