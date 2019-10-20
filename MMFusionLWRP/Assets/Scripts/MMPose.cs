using UnityEngine;

public class MMPose
{
    private Vector3 rootPos, lFootPos, rFootPos, neckPos,
        rootVel, lFootVel, rFootVel, neckVel;
    public MMPose(Vector3 rootPos, Vector3 lFootPos, Vector3 rFootPos, Vector3 neckPos, Vector3 rootVel, Vector3 lFootVel, Vector3 rFootVel, Vector3 neckVel)
    {
        this.rootPos = rootPos;
        this.lFootPos = lFootPos;
        this.rFootPos = rFootPos;
        this.neckPos = neckPos;
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
    public Vector3 GetNeckPos()
    {
        return neckPos;
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
    public Vector3 GetNeckVelocity()
    {
        return neckVel;
    }

    public float GetFeetDistance(MMPose otherFeet, Matrix4x4 newSpace, float feetWeight)
    {
        float distance = 0;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootPos()), newSpace.MultiplyPoint3x4(otherFeet.GetLeftFootPos())) * feetWeight;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootPos()), newSpace.MultiplyPoint3x4(otherFeet.GetRightFootPos())) * feetWeight;
        //distance += Vector3.Distance(GetLeftFootPos(), otherFeet.GetLeftFootPos()) / feetWeight;
        //distance += Vector3.Distance(GetRightFootPos(), otherFeet.GetRightFootPos()) / feetWeight;
        return distance;
    }
    public float ComparePoses(MMPose candidatePose, Matrix4x4 newSpace, float weightLFootVel, float weightRFootVel, float weightRootVel)
    {
        float difference = 0;
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootVelocity()) * weightLFootVel,
            newSpace.MultiplyPoint3x4(candidatePose.GetLeftFootVelocity()) * weightLFootVel);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootVelocity()) * weightRFootVel,
            newSpace.MultiplyPoint3x4(candidatePose.GetRightFootVelocity()) * weightRFootVel);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRootVelocity()) * weightRootVel,
            newSpace.MultiplyPoint3x4(candidatePose.GetRootVelocity()) * weightRootVel);

        //difference += Vector3.Distance(GetLeftFootVelocity() * weightLFootVel, candidatePose.GetLeftFootVelocity() * weightLFootVel);
        //difference += Vector3.Distance(GetRightFootVelocity() * weightRFootVel, candidatePose.GetRightFootVelocity() * weightRFootVel);
        //difference += Vector3.Distance(GetRootVelocity() * weightRootVel, candidatePose.GetRootVelocity() * weightRootVel);
        return difference;
    }
}
