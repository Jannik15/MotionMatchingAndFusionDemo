using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionMatching : MonoBehaviour
{
    // --- References
    private Movement movement;
    private PreProcessing preProcessing;

    // --- Collections
    public AnimationClip[] allClips;
    private List<FeatureVector> featureVectorForAllIDs;

    // --- Public 
    public bool _preProcess;
    public int pointsPerTrajectory = 4;
    public int framesBetweenTrajectoryPoints = 10;

    void Awake() // Load animation data
    {
        if (_preProcess)
        {
            // TODO: Put this in preprocessing, too much for MM script!
            featureVectorForAllIDs = new List<FeatureVector>();
            int i = 0;
            foreach (var clip in allClips)
            {
                for (int j = 0; j < clip.length * clip.frameRate; j++)
                {
                    //featureVectorForAllIDs[i] = new FeatureVector(new MMPose(allClips[j].), );
                    i++;
                }

            }

            preProcessing.Preprocess(featureVectorForAllIDs);
        }
        preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);
    }

    void Start()
    {
        // --- Instantiation
        movement = GetComponent<Movement>();

    }

    void Update()
    {
        
    }
}
