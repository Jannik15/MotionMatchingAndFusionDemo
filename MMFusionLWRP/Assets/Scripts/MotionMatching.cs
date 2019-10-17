﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    //private string[] allClipTags;     // TODO: Remove all out commented code if not needed anymore? - YYY
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
    //public ClipTags tagContainer; // put ref to chosen tag container scriptable object
    public string[] jointNames;

    //public static string[] movementTags = {"Idle", "Walk", "Run"};
    public string[][] movementTags =    // TODO: Changed the above 'movementTag' to this (So we can easily define the different states) - YYY
    {
        new []{ "Idle"},                // State 0
        new []{ "Walk", "Run" }         // State 1
    };
    private int currentState = 0;

    // --- Variables 
    public bool _preProcess;
    public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
    [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    public float idleThreshold = 0.10f;
    private List<bool> enumeratorBools;
    private AnimationClip currentClip;
    private int currentFrame, currentID = 0;

    // --- Weights
    [Range(0, 1)]
    public float weightLFootVel = 1.0f, weightRFootVel = 1.0f, weightRootVel = 1.0f, weightTrajPoints = 1.0f, weightTrajForwards = 1.0f;

    private void Awake() // Load animation data
    {
        movement = GetComponent<Movement>();
	    animator = GetComponent<Animator>();
        preProcessing = new PreProcessing();

        allClips = animContainer.animationClips;
        //allClipTags = tagContainer.clipTags;
        if (allClips == null)
            Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");

#if UNITY_EDITOR
        if (_preProcess)
        {
            animContainer.animationClips = preProcessing.FindClipsFromAnimatorController();
            allClips = animContainer.animationClips;
            //tagContainer.clipTags = preProcessing.GenerateClipTags(allClips, movementTags);
            //allClipTags = tagContainer.clipTags;
            preProcessing.Preprocess(allClips, jointNames);
        }
#endif
        featureVectors = preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);

        for (int i = 0; i < allClips.Length; i++)
        {
            int frames = (int) (allClips[i].length * allClips[i].frameRate);
	        for (int j = 0; j < featureVectors.Count; j++)
	        {
				if (featureVectors[j].GetClipName() == allClips[i].name)
                    featureVectors[j].SetFrameCount(frames);
	        }
        }

        for (int i = 0; i < movementTags.Length; i++)
        {
            for (int j = 0; j < movementTags[i].Length; j++)
            {
                movementTags[i][j] = movementTags[i][j].ToLower();
            }
        }

        for (int i = 0; i < featureVectors.Count; i++)
        {
			if (i != 0)
				featureVectors[i].CalculateVelocity(featureVectors[i - 1].GetPose(), allClips[0].frameRate);
			else
				featureVectors[i].CalculateVelocity(featureVectors[i].GetPose(), allClips[0].frameRate); // Velocity is 0 for frame 0 of all animations
        }
        enumeratorBools = AddEnumeratorBoolsToList();
    }

    private void Start()
    {
        // --- Instantiation
		UpdateAnimation(0, 0);
        StartCoroutine(MotionMatch());
    }

    private void FixedUpdate()
    {
        if (movement.GetSpeed() <= idleThreshold)
        {
            currentState = 0;
        }
        else
        {
            currentState = 1;
        }
        if (!_isMotionMatching /* && movement.GetSpeed() > idleThreshold*/)
	    {
			StopAllCoroutines();
		    StartCoroutine(MotionMatch());
	    }
        //if (!_isIdling && movement.GetSpeed() <= idleThreshold)
        //{
        //    StopAllCoroutines();
        //    StartCoroutine(Idle());
        //}
    }

    private void OnDrawGizmos()
    {
	    if (Application.isPlaying)
	    {
		    for (int i = 0; i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length; i++)
		    {
				// Position
			    Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                Gizmos.DrawLine(i != 0 ? movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : movement.posHolder,
	                movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());

                // Forward
			    Gizmos.color = Color.blue;
				Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 
					movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
		    }
        }
    }

    private void UpdateAnimation(int id, int frame)
    {
		//Debug.Log("Updating animation (ID): "+ currentID + " -> " + id);
		for (int i = 0; i < allClips.Length; i++)
	    {
		    if (allClips[i].name == featureVectors[id].GetClipName())
		    {
			    currentClip = allClips[i];
			    break;
		    }
		}
		animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0, frame / currentClip.length); // 0.3f was recommended by Magnus
        currentID = id;
        currentFrame = frame;
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
		    list[i] = booleanVal;
    }
    private IEnumerator MotionMatch()
    {
        SetBoolsInList(enumeratorBools, false);
	    _isMotionMatching = true;
	    while (true)
	    {
		    currentID += queryRateInFrames;
            List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory());
            int candidateID = PoseMatching(candidates);
			UpdateAnimation(candidateID, (int)featureVectors[candidateID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
	    }
    }

    private IEnumerator Idle()
    {
        SetBoolsInList(enumeratorBools, false);
        _isIdling = true;
        while (true)
	    {
            int candidateID = PoseMatching(featureVectors);
            UpdateAnimation(candidateID, (int)featureVectors[candidateID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
	    }
    }
    #endregion

    List<FeatureVector> TrajectoryMatching(Trajectory movementTraj)
    {
        List<FeatureVector> candidates = new List<FeatureVector>();
        FeatureVector[,] possibleCandidates = new FeatureVector[movementTags.Length, candidatesPerMisc];
        float[,] values = new float[movementTags.Length, candidatesPerMisc];
        int currentMisc = currentState;
        //Debug.Log("Current ID is " + currentID);
        for (int i = 0; i < featureVectors.Count; i++)
		{
            //Debug.Log("ID " + i + " with name: " + featureVectors[i].GetClipName() + " compared to state: " + currentState);
            if (!TagChecker(featureVectors[i].GetClipName(), currentState)
            ) // TODO: Added this tag checker bool - We need to optimize it maybe. See further down
            {
                continue;
            }
            //Debug.Log("FV passed: ID " + featureVectors[i].GetID() + " at frame " + featureVectors[i].GetFrame() + " of " + featureVectors[i].GetFrameCountForID() + " frames.");
            if ((featureVectors[i].GetID() > currentID ||  featureVectors[i].GetID() < currentID - queryRateInFrames) &&
                 featureVectors[i].GetFrame() + queryRateInFrames <  featureVectors[i].GetFrameCountForID())
            {
                //for (int j = 0; j < movementTags.Length; j++) // This for loop can be used if looking to consider multiple miscs for a single trajectory match
                //{
                //    if (TagChecker(featureVectors[i].GetClipName(), j))
                //    {
                //        currentMisc = j;
                //    }
                //}
                float tempVal = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj, weightTrajPoints, weightTrajForwards);
                float comparison = tempVal;
                int indexOfHighestValue = 0;
                for (int j = 0; j < possibleCandidates.GetLength(1); j++)
                {
                    if (possibleCandidates[currentMisc, j] != null)
                    {
                        if (comparison < values[currentMisc, j])
                        {
                            comparison = values[currentMisc, j];
                            indexOfHighestValue = j;
                        }
                    }
                    else
                    {
                        possibleCandidates[currentMisc, j] = featureVectors[i];
                        values[currentMisc, j] = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj, weightTrajPoints, weightTrajForwards);
                    }
                }
                if (tempVal < comparison)
                {
                    possibleCandidates[currentMisc, indexOfHighestValue] = featureVectors[i];
                    values[currentMisc, indexOfHighestValue] = tempVal;
                }
            }
        }
        foreach (var candidate in possibleCandidates)
        {
            if (candidate != null)
                candidates.Add(candidate);
        }
        return candidates;
    }

    private int PoseMatching(List<FeatureVector> candidates)
    {
        int bestId = -1;
        float currentDif = float.MaxValue;
        foreach (var candidate in candidates)
        {
            float velDif = featureVectors[currentID].ComparePoses(candidate, weightLFootVel, weightRFootVel, weightRootVel);
            float feetPosDif = featureVectors[currentID].GetPose().GetFeetDistance(candidate.GetPose());
            float candidateDif = velDif + feetPosDif;
            if (candidateDif < currentDif)
            {
				//Debug.Log("Candidate diff: " + velDif + " < " + " Current diff:" + currentDif);
                bestId = candidate.GetID();
                currentDif = candidateDif;
            }
        }
		Debug.Log("Returning best id from pose matching: " + bestId);
		return bestId;
    }

    private bool TagChecker(string candidateName, int stateNumber)
    {
        for (int i = 0; i < movementTags[stateNumber].Length; i++)
        {
            if (candidateName.ToLower().Contains(movementTags[stateNumber][i]))   // TODO: Not sure if this is a good solution performance-wise. - YYY
            {                                                                     // TODO: movementTags set to lower case during awake. Not sure how else to optimize?
                //Debug.Log(candidateName.ToLower() + " Contains " + movementTags[stateNumber][i]);
                return true;
            }
        }
        return false;
    }

}