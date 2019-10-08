using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    private TrajectoryPoint[] trajectoryPoints;
    public Trajectory(TrajectoryPoint[] _trajectoryPoints)
    {
        trajectoryPoints = _trajectoryPoints;
    }

    public TrajectoryPoint[] GetTrajectoryPoints()
    {
        return trajectoryPoints;
    }

    public float CompareTrajectories(Trajectory otherTrajectory)
    {
        float dist = 0;
        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            dist += trajectoryPoints[i].GetDiff(otherTrajectory.trajectoryPoints[i]);
        }
        return dist;
    }
    public float CompareTrajectories(Trajectory otherTrajectory, float pointWeight, float forwardWeight)
    {
        float dist = 0;
        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            dist += trajectoryPoints[i].GetDiffWithWeights(otherTrajectory.trajectoryPoints[i], pointWeight, forwardWeight);
        }
        return dist;
    }
}
