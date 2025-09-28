using Godot;
using System;

[Tool]
public partial class TrackCurve : Node3D
{
    [ExportCategory("CONFIGURATION:")]
    private Vector2[] _trackPoints = new Vector2[0];

    [Export]
    public Vector2[] trackPoints
    {
        get => _trackPoints;
        set
        {
            if (_trackPoints != value)
            {
                _trackPoints = value;
                //ParamatiseCurve();
                UpdateGizmos();
            }
        }
    }

    Vector2[] paramatisedCurve;
    float[] paramatisedCurveDistances;

    [Export]
    PackedScene trackLinkPrefab;
    Node3D[] trackLinks;
    [Export]
    int numberOfLinks;
    [Export]
    float linkOffset = 0;

    float currentOffset = 0;

    [Export]
    float curveResolution = 1f;

    [ExportCategory("In Editor Visualisation")]
    [ExportToolButton("Create Tracks")]
    public Callable ClickMeButton => Callable.From(CreateLinks);
    [ExportToolButton("Clear Tracks")]
    public Callable delete => Callable.From(ClearLinks);
    [ExportToolButton("Validate Tracks Points")]
    public Callable validate => Callable.From(ClearLinks);


    public override void _Ready()
    {
        CreateLinks();
        MapTracksToCurve();
    }

    void CreateLinks()
    {
        if (numberOfLinks == 0)
            return;

        trackLinks = new Node3D[numberOfLinks];

        for (int i = 0; i < numberOfLinks; i++)
        {
            var trackInstance = trackLinkPrefab.Instantiate();
            AddChild(trackInstance);
            trackLinks[i] = (Node3D)trackInstance;
        }
    }

    void ClearLinks()
    {
        var children = GetChildren();
        foreach(var child in children)
        {
            child.Free();
        }
        trackLinks = Array.Empty<Node3D>();
    }

    public override void _Process(double delta)
    {
        MapTracksToCurve();
    }

    public void DriveTrack(float inputValue)
    {
        currentOffset += inputValue;
        if(Math.Abs(currentOffset) >= linkOffset && Math.Abs(currentOffset) % linkOffset < 0.005f)
        {
            //currentOffset = 0;
        }
    }

    void MapTracksToCurve()
    {
        if (trackPoints.Length > 3)
        {
            int currentPointIndex = 0;
            float distanceToNextPoint = trackPoints[currentPointIndex].DistanceTo(trackPoints[currentPointIndex + 1]);
            float currentDistance = currentOffset;

            if(currentDistance > 0)
            {
                while (currentDistance > distanceToNextPoint)
                {
                    currentPointIndex++;
                    if (currentPointIndex >= trackPoints.Length)
                        currentPointIndex = 0;
                    currentDistance -= distanceToNextPoint;
                    distanceToNextPoint = trackPoints[currentPointIndex].DistanceTo(GetNextTrackPoint(currentPointIndex));
                }
            }
            else if(currentDistance < 0)
            {
                currentPointIndex = trackPoints.Length;
                while(currentDistance < 0)
                {
                    currentPointIndex--;
                    if (currentPointIndex < 0)
                        currentPointIndex = trackPoints.Length - 1;
                    distanceToNextPoint = trackPoints[currentPointIndex].DistanceTo(GetNextTrackPoint(currentPointIndex));
                    currentDistance += distanceToNextPoint;

                }
            }
            
            for (int i = 0; i < trackLinks.Length; i++)
            {
                Vector2 firstPoint = trackPoints[currentPointIndex].MoveToward(GetNextTrackPoint(currentPointIndex), currentDistance);
                Vector2 secondPoint;
                currentDistance += linkOffset;
                if(currentDistance < distanceToNextPoint)
                {
                    secondPoint = trackPoints[currentPointIndex].MoveToward(GetNextTrackPoint(currentPointIndex), currentDistance);
                }
                else
                {
                    currentPointIndex++;
                    if (currentPointIndex >= trackPoints.Length)
                        currentPointIndex = 0;

                    while(firstPoint.DistanceTo(trackPoints[currentPointIndex]) < linkOffset
                        && firstPoint.DistanceTo(GetNextTrackPoint(currentPointIndex)) < linkOffset)
                    {
                        currentPointIndex++;
                        if (currentPointIndex >= trackPoints.Length)
                            currentPointIndex = 0;
                    }

                    distanceToNextPoint = trackPoints[currentPointIndex].DistanceTo(GetNextTrackPoint(currentPointIndex));
                    secondPoint = GetIntersectPosition(GetNextTrackPoint(currentPointIndex), trackPoints[currentPointIndex], firstPoint, linkOffset);
                    currentDistance = secondPoint.DistanceTo(trackPoints[currentPointIndex]);
                }

                float y = secondPoint.Y - firstPoint.Y;
                float x = secondPoint.X - firstPoint.X;
                float angle = (float)Math.Atan2(y, x);

                Vector2 position = new Vector2(
                    (secondPoint.X - firstPoint.X) / 2,
                    (secondPoint.Y - firstPoint.Y) / 2);
                trackLinks[i].Position = new Vector3(0, position.Y + firstPoint.Y, position.X + firstPoint.X);
                trackLinks[i].Rotation = new Vector3(-angle, 0, 0);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector2 GetNextTrackPoint(int index)
    {
        int nextPointIndex = index + 1;
        if(nextPointIndex >= trackPoints.Length)
            nextPointIndex = 0;
        return trackPoints[nextPointIndex];
    }

    /// <summary>
    /// 
    /// https://math.stackexchange.com/questions/311921/get-location-of-vector-circle-intersection
    /// </summary>
    /// <param name="lineStart"></param>
    /// <param name="lineEnd"></param>
    /// <param name="circleOrigin"></param>
    /// <returns></returns>
    Vector2 GetIntersectPosition(Vector2 lineStart, Vector2 lineEnd, Vector2 circleOrigin, float radius)
    {

        var a = Math.Pow(lineEnd.X - lineStart.X, 2) + Math.Pow(lineEnd.Y - lineStart.Y, 2);
        var b = 2 * (lineEnd.X - lineStart.X) * (lineStart.X - circleOrigin.X)
              + 2 * (lineEnd.Y - lineStart.Y) * (lineStart.Y - circleOrigin.Y);
        var c = Math.Pow(lineStart.X - circleOrigin.X, 2) + Math.Pow(lineStart.Y - circleOrigin.Y, 2) - Math.Pow(radius, 2);

        var t = (2 * c) / (-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * c));

        var xt = (lineEnd.X - lineStart.X) * t + lineStart.X;
        var xy = (lineEnd.Y - lineStart.Y) * t + lineStart.Y;
        return new Vector2((float)xt, (float)xy);
    }

    /// <summary>
    /// Paramatise the curve 
    /// </summary>
    void ParamatiseCurve()
    {
        paramatisedCurve = new Vector2[(int)(trackPoints.Length * curveResolution)];
        float curveLength = CalculateCurveLength();
        float pointDistance = curveLength / trackPoints.Length;
        float distanceToNext = pointDistance;
        int paramatisedCurveIndex = 1;
        paramatisedCurve[0] = trackPoints[0];
        for(int i = 0; i < trackPoints.Length; i++)
        {
            
        }
    }

    float CalculateCurveLength()
    {
        float length = 0;
        for (int i = 0; i < trackPoints.Length; i++)
        {
            length += trackPoints[i].DistanceTo(GetNextTrackPoint(i));
        }
        return length;
    }
}
