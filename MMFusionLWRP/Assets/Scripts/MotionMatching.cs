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
    private AnimationClip[] allClips;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
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
            if (animContainer != null)
                allClips = animContainer.animationClips;
            if (allClips == null)
            {
                Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");
                return;
            }

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

    private void PoseMatching()
    {

    }

    private void SaveAllAnimClipsToContainer(AnimationClip[] animClips)
    {
        if (animContainer == null)
            return;

        animContainer.animationClips = animClips;
        Debug.Log("AnimationClips saved to scriptable object " + animContainer.name);
    }
}
