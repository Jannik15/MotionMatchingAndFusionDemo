﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MotionMatching : MonoBehaviour
{
    // --- References
    private Movement movement;
    private PreProcessing preProcessing;
    private Animator animator;

    // --- Collections
    private List<FeatureVector> featureVectors;
    private AnimationClip[] allClips;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
    public string[] jointNames;

    // --- Public 
    public bool _preProcess;
    public int pointsPerTrajectory = 4;
    public int framesBetweenTrajectoryPoints = 10;
    [SerializeField] private int queryRateInFrames = 10, bannedIDsLength = 10;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    private Queue<int> bannedIDs;
    private List<bool> enumeratorBools;
    private AnimationClip currentClip;
    private int currentFrame, currentID = -1;

    // --- Weights
    [Range(0, 1)]
    public float weightLFootVel = 1, weightRFootVel = 1, weightRootVel = 1;
    private int trajectoryCandidates;

    private void Awake() // Load animation data
    {
	    movement = GetComponent<Movement>();
	    animator = GetComponent<Animator>();
        preProcessing = new PreProcessing();

        if (animContainer != null)
            allClips = animContainer.animationClips;
        if (allClips == null)
        {
            Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");
            return;
        }

#if UNITY_EDITOR
        if (_preProcess)
        {
            animContainer.animationClips = preProcessing.FindClipsFromAnimatorController();
            preProcessing.Preprocess(allClips, jointNames);
        }
#endif
        featureVectors = preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);
		enumeratorBools = AddEnumeratorBoolsToList();
		bannedIDs = new Queue<int>(bannedIDsLength);
    }

    private void Start()
    {
        // --- Instantiation
        StartCoroutine(MotionMatch());
    }

    private void Update()
    {
	    if (!_isMotionMatching)
	    {
			StopAllCoroutines();
		    StartCoroutine(MotionMatch());
	    }
		// TODO: Add !isIdling based on movement (velocity?)
		Debug.Log("Movement base velocity: " + movement.GetMovementVelocity());
		Debug.Log("Movement divided velocity: " + movement.GetMovementVelocity() / Time.fixedDeltaTime);
    }

    private void UpdateAnimation(int id, int frame)
    {
	    for (int i = 0; i < allClips.Length; i++)
	    {
		    if (allClips[i].name == featureVectors[id].GetClipName())
		    {
			    currentClip = allClips[i];
			    break;
		    }
	    }
	    currentFrame = frame;
		bannedIDs.Enqueue(id);
        currentID = id;
		Debug.Log("Banned ID Queue count: " + bannedIDs.Count);
		animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0, currentFrame / currentClip.length); // 0.3f was recommended by Magnus
    }

    #region IEnumerators
    private List<bool> AddEnumeratorBoolsToList()
    {
		List<bool> list = new List<bool>();
		list = new List<bool>();
		list.Add(_isMotionMatching);
		list.Add(_isIdling);
		return list;
    }
    private void SetBoolsInList(List<bool> list, bool booleanVal)
    {
	    for (int i = 0; i < list.Count; i++)
	    {
		    list[i] = booleanVal;
	    }
    }
    private IEnumerator MotionMatch()
    {
	    SetBoolsInList(enumeratorBools, false);
	    _isMotionMatching = true;
	    while (true)
	    {
            currentID += queryRateInFrames;
            List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(), trajectoryCandidates);
            int candidateID = PoseMatching(candidates);
			UpdateAnimation(candidateID, (int)featureVectors[candidateID].GetFrame());
		    yield return new WaitForSeconds(queryRateInFrames / allClips[0].frameRate);
	    }
    }

    private IEnumerator Idle()
    {
	    SetBoolsInList(enumeratorBools, false);
        _isIdling = true;
	    while (true)
	    {
			yield return new WaitForSeconds((currentFrame - currentClip.length) / currentClip.frameRate);
	    }
    }

    #endregion

    List<FeatureVector> TrajectoryMatching(Trajectory movement, int candidatesPerMisc)
    {
		List<FeatureVector> candidates = new List<FeatureVector>();

        for (int i = 0; i < featureVectors.Count; i++)
        {
            if (featureVectors[i].GetID() > currentID || featureVectors[i].GetID() < featureVectors[i].GetID() - queryRateInFrames)
            {
                if (featureVectors[i].GetClipName() == currentClip.name)
                {
                    if (featureVectors[i].GetID() >= currentID - 10 && featureVectors[i].GetID() < currentID)
                    {
                        continue; // Skip this candidate if it belong to the same animation, but at a previous frame
                    }
                }
                if (featureVectors[i].GetTrajectory().CompareTrajectories(movement) +
                    featureVectors[i].GetTrajectory().CompareTrajectories(movement) < candidatesPerMisc)
                { // TODO: Change to best # (KNN) for each anim type (misc tag, like left, forward, right) instead of threshold
                    //Debug.Log("TrajComparisonDist: " + featureVectors[i].CompareTrajectoryPoints(movement) +
                    //          featureVectors[i].CompareTrajectoryForwards(movement));
                    candidates.Add(featureVectors[i]);
                }
            }
        }
        return candidates;
    }

    private int PoseMatching(List<FeatureVector> candidates)
    {

        int bestId = 0;
        float currentDif = 9999999;

        foreach (var candidate in candidates)
        {
            float candidateDif = ComparePoses(featureVectors[currentID], candidate);
            if (candidateDif < currentDif)
            {
                bestId = candidate.GetID();
            }
        }

        return bestId;

    }

    private float ComparePoses(FeatureVector currentVector, FeatureVector candidateVector)
    {
        float difference = 0;
        
        if (currentVector.GetID() == 0)
            currentVector.CalculateVelocity(featureVectors[currentVector.GetID()].GetPose(), allClips[0].frameRate);
        else
            currentVector.CalculateVelocity(featureVectors[currentVector.GetID() - 1].GetPose(), allClips[0].frameRate);
 
        difference += Vector3.Distance(currentVector.GetLeftFootVelocity() * weightLFootVel, candidateVector.GetLeftFootVelocity() * weightLFootVel);
        difference += Vector3.Distance(currentVector.GetRightFootVelocity() * weightRFootVel, candidateVector.GetRightFootVelocity() * weightRFootVel);
        difference += Vector3.Distance(currentVector.GetRootVelocity() * weightRootVel, candidateVector.GetRootVelocity() * weightRootVel);

        return difference;
    }

    private void SaveAllAnimClipsToContainer(AnimationClip[] animClips)
    {
        if (animContainer == null)
            return;

        animContainer.animationClips = animClips;
        Debug.Log("AnimationClips saved to scriptable object " + animContainer.name);
    }
}
