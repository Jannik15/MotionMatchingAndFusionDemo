using UnityEngine;

public class MMPose
{
    private Vector3 rootPos, lFootPos, rFootPos;
    public MMPose(Vector3 _rootPos, Vector3 _lFootPos, Vector3 _rFootPos)
    {
	    rootPos = _rootPos;
        lFootPos = _lFootPos;
        rFootPos = _rFootPos;
    }
    public Vector3 GetRootPos()
    {
        return rootPos;
    }
    public Vector3 GetLeftFootPos()
    {
        return lFootPos;
    }
    public Vector3 GetRightFootPos()
    {
        return rFootPos;
    }
}
