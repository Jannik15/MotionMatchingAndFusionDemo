public class FeatureVector
{
    private MMPose pose;
    private Trajectory trajectory;
    private int id;
    private string clipName;
    private float frame;

    FeatureVector(MMPose _pose, Trajectory _trajectory, int _id, string _clipName, float _frame)
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
}
