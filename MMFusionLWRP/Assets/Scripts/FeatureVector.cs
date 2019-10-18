using UnityEngine;

public class FeatureVector
{
	private int id;
	private string clipName;
	private int frame;
	private int allFrames;
    private MMPose pose;
    private Trajectory trajectory;
    private Vector3 rootVel, lFootVel, rFootVel;

    public FeatureVector(MMPose _pose, Trajectory _trajectory, int _id, string _clipName, int _frame)
    {
        pose = _pose;
        trajectory = _trajectory;
        id = _id;
        clipName = _clipName;
        frame = _frame;
    }

    public void SetFrameCount(int frameCountForID)
    {
	    allFrames = frameCountForID;
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
    public int GetFrame()
    {
        return frame;
    }
    public int GetFrameCountForID()
    {
	    return allFrames;
    }

    public Vector3 GetRootVelocity()
    {
	    return rootVel;
    }
    public Vector3 GetLeftFootVelocity()
    {
	    return lFootVel;
    }
    public Vector3 GetRightFootVelocity()
    {
	    return rFootVel;
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

    public void CalculateVelocity(MMPose previousPose, Matrix4x4 newSpace, float sampleRate)
    {
        rootVel = pose.GetRootPos() - previousPose.GetRootPos() * sampleRate;
        lFootVel = (newSpace.MultiplyPoint3x4(pose.GetLeftFootPos()) - newSpace.MultiplyPoint3x4(previousPose.GetLeftFootPos())) * sampleRate;
        rFootVel = (newSpace.MultiplyPoint3x4(pose.GetRightFootPos()) - newSpace.MultiplyPoint3x4(previousPose.GetRightFootPos())) * sampleRate;
        //rootVel = (pose.GetRootPos() - previousPose.GetRootPos()) * sampleRate;
        //lFootVel = (pose.GetLeftFootPos() - previousPose.GetLeftFootPos()) * sampleRate;
        //rFootVel = (pose.GetRightFootPos() - previousPose.GetRightFootPos()) * sampleRate;
    }
    public float ComparePoses(FeatureVector candidateVector, Matrix4x4 newSpace, float weightLFootVel, float weightRFootVel, float weightRootVel)
    {
	    float difference = 0;
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootVelocity()) * weightLFootVel,
         newSpace.MultiplyPoint3x4(candidateVector.GetLeftFootVelocity()) * weightLFootVel);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootVelocity()) * weightRFootVel,
         newSpace.MultiplyPoint3x4(candidateVector.GetRightFootVelocity()) * weightRFootVel);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRootVelocity()) * weightRootVel,
         newSpace.MultiplyPoint3x4(candidateVector.GetRootVelocity()) * weightRootVel);

        difference += Vector3.Distance(GetLeftFootVelocity() * weightLFootVel, candidateVector.GetLeftFootVelocity() * weightLFootVel);
        difference += Vector3.Distance(GetRightFootVelocity() * weightRFootVel, candidateVector.GetRightFootVelocity() * weightRFootVel);
        difference += Vector3.Distance(GetRootVelocity() * weightRootVel, candidateVector.GetRootVelocity() * weightRootVel);
        return difference;
    }
}
