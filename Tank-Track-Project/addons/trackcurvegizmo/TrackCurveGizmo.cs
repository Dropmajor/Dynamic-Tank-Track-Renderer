using Godot;
using System;

public partial class TrackCurveGizmo : EditorNode3DGizmoPlugin
{
    private EditorUndoRedoManager _undoRedo;

    public TrackCurveGizmo(EditorUndoRedoManager undoRedo)
    {
        CreateMaterial("NodeMainPointMaterial", new Color(1, 0, 0));
        CreateHandleMaterial("HandleMainPointMaterial");
        var handleMainPointMaterial = GetMaterial("HandleMainPointMaterial");
        handleMainPointMaterial.AlbedoColor = new Color(1, 0, 0);
        _undoRedo = undoRedo;
    }

    public TrackCurveGizmo()
    {
        CreateMaterial("NodeMainPointMaterial", new Color(1, 0, 0));
        CreateHandleMaterial("HandleMainPointMaterial");
        var handleMainPointMaterial = GetMaterial("HandleMainPointMaterial");
        handleMainPointMaterial.AlbedoColor = new Color(1, 0, 0);
        //_undoRedo = undoRedo;
    }

    public override void _Redraw(EditorNode3DGizmo gizmo)
    {
        base._Redraw(gizmo);
        gizmo.Clear();
        var track = (TrackCurve)gizmo.GetNode3D();

        // Draw NodeMainPoint.
        if (track != null && track.trackPoints.Length > 2)
        {
            var handles = new Vector3[track.trackPoints.Length];
            handles[0] = new Vector3(0, track.trackPoints[0].Y, track.trackPoints[0].X);
            for (int i = 1; i < track.trackPoints.Length; i++)
            {
                var currentLine = new[]
                {
                    new Vector3(0, track.trackPoints[i].Y, track.trackPoints[i].X),
                    new Vector3(0, track.trackPoints[i - 1].Y, track.trackPoints[i - 1].X),
                };
                gizmo.AddLines(currentLine, GetMaterial("NodeMainPointMaterial", gizmo));
                handles[i] =
                    new Vector3(0, track.trackPoints[i].Y, track.trackPoints[i].X);
            }
            gizmo.AddHandles(handles,
                    GetMaterial("HandleMainPointMaterial", gizmo),
                    []);

            var lastLine = new[]
                {
                    new Vector3(0, track.trackPoints[track.trackPoints.Length - 1].Y, track.trackPoints[track.trackPoints.Length - 1].X),
                    new Vector3(0, track.trackPoints[0].Y, track.trackPoints[0].X),
                };
            gizmo.AddLines(lastLine, GetMaterial("NodeMainPointMaterial", gizmo));
        }
    }

    public override string _GetGizmoName()
    {
        return "TrackCurve Gizmo";
    }

    public override bool _HasGizmo(Node3D forNode3d)
    {
        return forNode3d is TrackCurve;
    }

    public override void _CommitHandle(EditorNode3DGizmo gizmo, int handleId,
        bool secondary, Variant restore, bool cancel)
    {
        //need to fix this later so that you can redo after building
        if (_undoRedo == null)
            return;

        var track = (TrackCurve)gizmo.GetNode3D();
        _undoRedo.CreateAction("Change Track Curve");
        var oldPoints = (Vector2[]) track.trackPoints.Clone();
        oldPoints[handleId] = (Vector2)restore;

        _undoRedo.AddUndoProperty(track,
                    TrackCurve.PropertyName.trackPoints, oldPoints);
        _undoRedo.AddDoProperty(track,
                    TrackCurve.PropertyName.trackPoints,
                    track.trackPoints);
        _undoRedo.CommitAction();
        // customNode3D.UpdateGizmos();
    }

    /// <summary>
    /// Get the name of the gizmo handle
    /// </summary>
    /// <param name="gizmo"></param>
    /// <param name="handleId"></param>
    /// <param name="secondary"></param>
    /// <returns></returns>
    public override string _GetHandleName(EditorNode3DGizmo gizmo, int handleId,
        bool secondary)
    {
        return "Track Point: " + handleId;
    }

    public override Variant _GetHandleValue(EditorNode3DGizmo gizmo, int handleId,
        bool secondary)
    {
        var track = (TrackCurve)gizmo.GetNode3D();
        return track.trackPoints[handleId];
    }

    public override void _SetHandle(EditorNode3DGizmo gizmo, int handleId,
        bool secondary, Camera3D camera, Vector2 screenPos)
    {
        var track = (TrackCurve)gizmo.GetNode3D();

        Vector3 newPos = camera.ProjectPosition(screenPos,
                    GetZDepth(camera, track.GlobalPosition +
                                      new Vector3(0, track.trackPoints[handleId].Y, track.trackPoints[handleId].X)));
        track.trackPoints[handleId] = new Vector2(newPos.Z, newPos.Y);
        // customNode3D.UpdateGizmos();
    }

    private float GetZDepth(Camera3D camera, Vector3 position)
    {
        Vector3 cameraPosition = camera.GlobalPosition;
        // Remember Camera3D looks towards its -Z local axis.
        Vector3 cameraForwardVector = -camera.GlobalTransform.Basis.Z;

        Vector3 vectorToPosition = position - cameraPosition;
        float zDepth = vectorToPosition.Dot(cameraForwardVector);
        return zDepth;
    }
}
