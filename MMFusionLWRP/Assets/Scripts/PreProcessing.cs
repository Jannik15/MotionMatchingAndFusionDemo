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

    public void Preprocess(AnimationClip[] allClips)
    {
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
                // TODO: Get data from clip bindings, and add it to the relevant lists
                allPoses.Add(new MMPose());
                allPoints.Add(new TrajectoryPoint());
                i++;
            }

        }

        csvHandler.WriteCSV(allPoses, allPoints, allClipNames, allFrames);
    }
    public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
    {
        csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints);
        List<FeatureVector> featureVector = new List<FeatureVector>();
        return featureVector;
    }
    public Vector3 GetJointPositionAtFrame(AnimationClip clip, int frame, string jointName)
    {
        // Bindings are inherited from a clip, and the AnimationCurve is inherited from the clip's binding
        float[] vectorValues = new float[3];
        int arrayEnumerator = 0;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.propertyName.Contains(jointName))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                vectorValues[arrayEnumerator] = curve.Evaluate(frame / clip.frameRate);
                arrayEnumerator++;
            }
        }
        return new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]);
    }
    public Vector3 CalculateVelocityFromVectors(Vector3 currentPos, Vector3 prevPos)
    {
        return (currentPos - prevPos) / 1 / sampleRate;
    }
}
