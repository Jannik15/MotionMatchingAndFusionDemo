using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PreProcessing
{
    // --- References
    private CSVHandler csvHandler;

    // --- Collections
    private List<string> allClipNames;
    private List<int> allFrames;
    private List<MMPose> allPoses;
    private List<TrajectoryPoint> allPoints;
    private List<Vector3> allRootVels, allLFootVels, allRFootVels;
    
    // --- Variables
    private const float velFactor = 10.0f;

    public void Preprocess(AnimationClip[] allClips, HumanBodyBones[] joints, GameObject avatar, Animator animator)
    {
		csvHandler = new CSVHandler();

        allClipNames = new List<string>();
        allFrames = new List<int>();
        allPoses = new List<MMPose>();
        allPoints = new List<TrajectoryPoint>();

        Matrix4x4 charSpace = new Matrix4x4();
        for (int i = 0; i < allClips.Length; i++)
        {
            allClips[i].SampleAnimation(avatar, 0); // First frame of currently sampled animation
            Vector3 startPosForClip = animator.GetBoneTransform(joints[0]).position;
            Quaternion startRotForClip = animator.GetBoneTransform(joints[0]).rotation;
            charSpace.SetTRS(startPosForClip, startRotForClip, Vector3.one);

            for (int j = 0; j < (int)(allClips[i].length * allClips[i].frameRate); j++)
            {
                allClips[i].SampleAnimation(avatar, j / allClips[i].frameRate);
                allClipNames.Add(allClips[i].name);
                allFrames.Add(j);
                Vector3 rootPos = charSpace.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position);
                Vector3 lFootPos = charSpace.MultiplyPoint3x4(animator.GetBoneTransform(joints[1]).position);
                Vector3 rFootPos = charSpace.MultiplyPoint3x4(animator.GetBoneTransform(joints[2]).position);
                Vector3 neckPos = charSpace.MultiplyPoint3x4(animator.GetBoneTransform(joints[3]).position);
                //allPoses.Add(new MMPose(rootPos, lFootPos, rFootPos, neckPos,
                //    CalculateVelocity(rootPos, ), Vector3.zero, Vector3.zero, Vector3.zero));
                allPoints.Add(new TrajectoryPoint(rootPos,
                    charSpace.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position + animator.GetBoneTransform(joints[0]).forward)));
            }

			
        }
        csvHandler.WriteCSV(allPoses, allPoints, allClipNames, allFrames);
        
    }
    public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
    {
		if (csvHandler == null)
			csvHandler = new CSVHandler();
        return csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints); ;
    }

    public AnimationClip[] FindClipsFromAnimatorController()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
            return null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player.GetComponent<Animator>() == null)
            return null;

        Animator anim = player.GetComponent<Animator>();

        AnimationClip[] tempAnimClipArr = anim.runtimeAnimatorController.animationClips;

        return tempAnimClipArr;
    }
    public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPose, float velocityFactor)
    {
        return (currentPos - previousPose) * velocityFactor;
    }
    public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPose, Matrix4x4 newSpace, float velocityFactor)
    {
        return (newSpace.MultiplyPoint3x4(currentPos) - newSpace.MultiplyPoint3x4(previousPose)) * velocityFactor;
    }

}
