using Godot;
using System;

public partial class MovementController : Node3D
{
    [Export]
    TrackCurve leftTrack;
    [Export]
    TrackCurve rightTrack;
    [Export]
    float movementMultiplier = 0.5f;

    public override void _Process(double delta)
    {

        HandleMovement(delta);
    }

    void HandleMovement(double delta)
    {
        var dir = Input.GetAxis("Back", "Forward");

        leftTrack.DriveTrack((float)(movementMultiplier * delta * dir));
    }
}
