using Godot;
using System;

[Tool]
public partial class TrackCurveGizmoRegister : EditorPlugin
{
    private TrackCurveGizmo _gizmoPlugin;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        _gizmoPlugin = (TrackCurveGizmo)
            GD.Load<CSharpScript>("res://addons/trackcurvegizmo/TrackCurveGizmo.cs").New(GetUndoRedo());
        AddNode3DGizmoPlugin(_gizmoPlugin);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        RemoveNode3DGizmoPlugin(_gizmoPlugin);
    }
}
