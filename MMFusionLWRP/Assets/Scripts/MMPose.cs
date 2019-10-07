using UnityEngine;

public class MMPose
{
    private Vector3 rootVel, lFootVel, rFootVel;
    public MMPose(Vector3 _rootVel, Vector3 _lFootVel, Vector3 _rFootVel)
    {
        rootVel = _rootVel;
        lFootVel = _lFootVel;
        rFootVel = _rFootVel;
    }
    public Vector3 GetRootVelocity()
    {
        return rootVel;
    }
    public Vector3 GetLefFootVelocity()
    {
        return lFootVel;
    }
    public Vector3 GetRightFootVelocity()
    {
        return rFootVel;
    }
}
