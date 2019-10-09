using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionMatching : MonoBehaviour
{
    // --- References
    private Movement movement;
    private PreProcessing preProcessing;

    // --- Collections
    private List<FeatureVector> featureVectors;
    public AnimationClip[] allClips;
    public string[] jointNames;

    // --- Public 
    public bool _preProcess;
    public int pointsPerTrajectory = 4;
    public int framesBetweenTrajectoryPoints = 10;

    void Awake() // Load animation data
    {
		preProcessing = new PreProcessing();
		#if UNITY_EDITOR
        if (_preProcess)
        {
            preProcessing.Preprocess(allClips, jointNames);
        }
		#endif
        featureVectors = preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);
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
