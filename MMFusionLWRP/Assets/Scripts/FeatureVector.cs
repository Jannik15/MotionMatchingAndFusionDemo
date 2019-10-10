using UnityEngine;

public class FeatureVector
{
    private MMPose pose;
    private Trajectory trajectory;
    private int id;
    private string clipName;
    private float frame;

    public FeatureVector(MMPose _pose, Trajectory _trajectory, int _id, string _clipName, float _frame)
    {
        pose = _pose;
        trajectory = _trajectory;
        id = _id;
        clipName = _clipName;
        frame = _frame;
    }

    public MMPose GetPose()
    {
        return pose;
    }
    public Trajectory GetTrajectory()
    {
        return trajectory;
    }
    public int GetID()
    {
        return id;
    }
    public string GetClipName()
    {
        return clipName;
    }
    public float GetFrame()
    {
        return frame;
    }

    public Trajectory CreateTrajectory(TrajectoryPoint pointAtNextStep, int i)
    {
	    if (i == 0) // We check for index, since we do not want to override the initial trajectory point of the id.
	    {
		    if (trajectory.GetTrajectoryPoints()[0] == null) // This statement should never be true, if it is the instantiation of the trajectories is incorrect
				Debug.Log("Trajectory with ID: " + id + " is missing it's first component!");
	    }
	    else if (pointAtNextStep != null)
	    {
		    TrajectoryPoint tempPoint = pointAtNextStep;
		    if (trajectory.GetTrajectoryPoints()[i] == null || trajectory.GetTrajectoryPoints()[i].GetPoint() == Vector3.zero)
			    trajectory.GetTrajectoryPoints()[i] = tempPoint;
        }
	    else
		    Debug.Log("When trying to populate Trajectory of ID: " + id + " the Point at next step, with index " + i + " is null");
        return trajectory;
    }
}
