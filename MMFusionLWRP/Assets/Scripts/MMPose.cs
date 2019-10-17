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

    public float GetFeetDistance(MMPose otherFeet, Matrix4x4 newSpace, float feetWeight)
    {
        float distance = 0;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootPos()), newSpace.MultiplyPoint3x4(otherFeet.GetLeftFootPos())) / feetWeight;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootPos()), newSpace.MultiplyPoint3x4(otherFeet.GetRightFootPos())) / feetWeight;
        //distance += Vector3.Distance(GetLeftFootPos(), otherFeet.GetLeftFootPos()) / feetWeight;
        //distance += Vector3.Distance(GetRightFootPos(), otherFeet.GetRightFootPos()) / feetWeight;
        return distance;
    }
}
