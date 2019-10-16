﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PreProcessing
{
    // --- References
    private CSVHandler csvHandler;

    // --- Collections
    private List<string> allClipNames;
    private List<float> allFrames;
    private List<MMPose> allPoses;
    private List<Trajectory> allPoints;
    private List<Vector3> allRootVels, allLFootVels, allRFootVels;
    private float debugFloat;

    // --- Variables
    private float sampleRate = 30;

    public void Preprocess(AnimationClip[] allClips, string[] jointNames)
    {
		csvHandler = new CSVHandler();

        allClipNames = new List<string>();
        allFrames = new List<float>();
        allPoses = new List<MMPose>();
        allPoints = new List<Trajectory>();
        
        sampleRate = allClips[0].frameRate; // Update the sampling rate to the clips framerate 
        for (int i = 0; i < allClips.Length; i++)
        {
            for (int j = 0; j < (int) (allClips[i].length * allClips[i].frameRate); j++)
            {
                allClipNames.Add(allClips[i].name);
                allFrames.Add(j);
                allPoses.Add(new MMPose(GetJointPositionAtFrame(allClips[i], j, jointNames[0]), 
	                GetJointPositionAtFrame(allClips[i], j, jointNames[1]), GetJointPositionAtFrame(allClips[i], j, jointNames[2])));
                allPoints.Add(new Trajectory(new TrajectoryPoint(GetJointPositionAtFrame(allClips[i], j, jointNames[0]), 
	                GetJointQuaternionAtFrame(allClips[i], j, jointNames[0]) * Vector3.forward), // Forward for this point
	                GetJointQuaternionAtFrame(allClips[i], j, jointNames[0]))); // Quaternion for this point
            }
        }
        csvHandler.WriteCSV(allPoses, allPoints, allClipNames, allFrames);
        
    }
    public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
    {
		if (csvHandler == null)
			csvHandler = new CSVHandler();
		List<FeatureVector> featureVector = csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints);
        return featureVector;
    }

    public Vector3 GetJointPositionAtFrame(AnimationClip clip, int frame, string jointName)
    {
        // Bindings are inherited from a clip, and the AnimationCurve is inherited from the clip's binding
        float[] vectorValues = new float[3];
        int arrayEnumerator = 0;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.propertyName.Contains(jointName + "T"))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                vectorValues[arrayEnumerator] = curve.Evaluate(frame / clip.frameRate);
                arrayEnumerator++;
            }
        }
        return new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]);
    }
    public Quaternion GetJointQuaternionAtFrame(AnimationClip clip, int frame, string jointName)
    {
	    // Bindings are inherited from a clip, and the AnimationCurve is inherited from the clip's binding
	    AnimationCurve curve = new AnimationCurve();
	    float[] vectorValues = new float[4];
	    int arrayEnumerator = 0;
	    foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
	    {
		    if (binding.propertyName.Contains(jointName + "Q"))
		    {
			    curve = AnimationUtility.GetEditorCurve(clip, binding);
			    vectorValues[arrayEnumerator] = curve.Evaluate(frame / clip.frameRate);
			    debugFloat = frame / clip.frameRate;
			    arrayEnumerator++;
		    }
	    }
	    return new Quaternion(vectorValues[0], vectorValues[1], vectorValues[2], vectorValues[3]);
    }
    public Vector3 CalculateVelocityFromVectors(Vector3 currentPos, Vector3 prevPos)
    {
        return (currentPos - prevPos) * sampleRate;
    }

    //public string[] GenerateClipTags(AnimationClip[] allClips, string[] allTags)  // TODO: Remove all this code, if not needed? - YYY
    //{
    //    string[] tempClipTags = new string[allClips.Length];

    //    for (int i = 0; i < allClips.Length; i++)
    //    {
    //        for (int j = 0; j < allTags.Length; j++)
    //        {
    //            if (allClips[i].name.ToLower().Contains(allTags[j].ToLower()))
    //            {
    //                tempClipTags[i] += "#" + allTags[j].ToLower();
    //            }
    //        }

    //        if (tempClipTags[i] == null)
    //        {
    //            tempClipTags[i] = "#other";
    //        }
    //    }
    //    debugFloat.Log(tempClipTags[0]);
    //    return tempClipTags;
    //}

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
}
