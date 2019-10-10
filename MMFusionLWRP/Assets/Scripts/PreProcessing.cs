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
    private List<TrajectoryPoint> allPoints;

    // --- Variables
    private int sampleRate = 30;
    private float calucaltedSampleRate = 1 / 30;

    public void Preprocess(AnimationClip[] allClips, string[] jointNames)
    {
		csvHandler = new CSVHandler();

        allClipNames = new List<string>();
        allFrames = new List<float>();
        allPoses = new List<MMPose>();
        allPoints = new List<TrajectoryPoint>();

        sampleRate = (int)allClips[0].frameRate; // Update the sampling rate to the clips framerate 
        int i = 0;
        foreach (var clip in allClips)
        {
            for (int j = 0; j < (int)clip.length * clip.frameRate; j++)
            {
                allClipNames.Add(clip.name);
                allFrames.Add(j);
                Vector3 rootVel = CalculateVelocityFromVectors(GetJointPositionAtFrame(clip, j, jointNames[0]), GetJointPositionAtFrame(clip, j - 1, jointNames[0]));
                Vector3 lFootVel = CalculateVelocityFromVectors(GetJointPositionAtFrame(clip, j, jointNames[1]), GetJointPositionAtFrame(clip, j - 1, jointNames[1]));
                Vector3 rFootVel = CalculateVelocityFromVectors(GetJointPositionAtFrame(clip, j, jointNames[2]), GetJointPositionAtFrame(clip, j - 1, jointNames[2]));

                allPoses.Add(new MMPose(rootVel, lFootVel, rFootVel));
                allPoints.Add(new TrajectoryPoint(GetJointPositionAtFrame(clip,j, jointNames[0]), GetJointQuaternionAtFrame(clip,j,jointNames[0]) * Vector3.forward));
                i++;
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
    public Vector3 CalculateVelocityFromVectors(Vector3 currentPos, Vector3 prevPos)
    {
        return currentPos - prevPos / calucaltedSampleRate;
    }

    private AnimationClip[] FindClipsFromAnimatorController()
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
