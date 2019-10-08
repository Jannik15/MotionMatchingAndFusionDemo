using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPoint
{
    private Vector3 point, forward;
    public TrajectoryPoint()
    {
        point = Vector3.zero;
        forward = Vector3.zero;
    }
    public TrajectoryPoint(Vector3 _point, Vector3 _forward)
    {
        point = _point;
        forward = _forward;
    }
    public Vector3 GetPoint()
    {
        return point;
    }
    public Vector3 GetForward()
    {
        return forward;
    }
    public float GetDiff(TrajectoryPoint otherPoint)
    {
        float diff = 0;
        diff += Vector3.Distance(point, otherPoint.point);
        diff += Vector3.Angle(forward, otherPoint.forward);
        return diff;
    }
    public float GetDiffWithWeights(TrajectoryPoint otherPoint, float pointWeight, float forwardWeight)
    {
        float diff = 0;
        diff += Vector3.Distance(point, otherPoint.point) / pointWeight;
        diff += Vector3.Angle(forward, otherPoint.forward) / forwardWeight;
        return diff;
    }
}
