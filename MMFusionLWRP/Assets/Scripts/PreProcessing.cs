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

    public void Preprocess(AnimationClip[] allClips, string[] jointNames)
    {
		csvHandler = new CSVHandler();

        allClipNames = new List<string>();
        allFrames = new List<int>();
        allPoses = new List<MMPose>();
        allPoints = new List<TrajectoryPoint>();

        Matrix4x4 charSpace = new Matrix4x4();
        Vector3 startPosForClip = Vector3.zero;
        Quaternion startRotForClip = Quaternion.identity;
        for (int i = 0; i < allClips.Length; i++)
        {
	        startPosForClip = GetJointPositionAtFrame(allClips[i], 0, jointNames[0]);
	        startRotForClip = GetJointQuaternionAtFrame(allClips[i], 0, jointNames[0]);
	        charSpace.SetTRS(startPosForClip, startRotForClip, Vector3.one);

            for (int j = 0; j < (int) (allClips[i].length * allClips[i].frameRate); j++)
            {
                allClipNames.Add(allClips[i].name);
                allFrames.Add(j);
                allPoses.Add(new MMPose(
	                charSpace.MultiplyPoint3x4(GetJointPositionAtFrame(allClips[i], j, jointNames[0])),
	                charSpace.MultiplyPoint3x4(GetJointPositionAtFrame(allClips[i], j, jointNames[1])),
		            charSpace.MultiplyPoint3x4(GetJointPositionAtFrame(allClips[i], j, jointNames[2]))));
                allPoints.Add(new TrajectoryPoint(Vector3.zero,
	                charSpace.MultiplyPoint3x4(GetJointPositionAtFrame(allClips[i], j, jointNames[0]) +
	                                           GetJointQuaternionAtFrame(allClips[i], j, jointNames[0]) *
	                                           Vector3.forward))); // Forward for this point
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
			    arrayEnumerator++;
		    }
	    }
	    return new Quaternion(vectorValues[0], vectorValues[1], vectorValues[2], vectorValues[3]);
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
}
