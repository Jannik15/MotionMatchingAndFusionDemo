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
    [SerializeField] private int queryRateInFrames = 10, bannedIDsLength = 10, previousFramesToIgnore = 10;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    private Queue<int> bannedIDs;
    private List<bool> enumeratorBools;
    private AnimationClip currentClip;
    private int currentFrame;

    private void Awake() // Load animation data
    {
	    movement = GetComponent<Movement>();
	    animator = GetComponent<Animator>();
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

    List<FeatureVector> TrajectoryMatching(Trajectory movement, float candidatesPerMisc)
    {
        /* Culled candidates:
         * 3. Candidates that have been added the culledIDs queue (these have already been used)
         * 4. Candidates pertaining to the same animation as the current animation, but are too close to the current frame (previous)
        */


        //for (int i = 0; i < animTrajectoriesInCharSpace.Length; i++)
        //{
        //    if (animTrajectoriesInCharSpace[i].GetTrajectoryId() >= allClips[0].length * allClips[0].frameRate &&
        //        animTrajectoriesInCharSpace[i].GetTrajectoryId() != currentAnimId &&
        //        !culledIDs.Contains(animTrajectoriesInCharSpace[i].GetTrajectoryId()))
        //    {
        //        if (animTrajectoriesInCharSpace[i].GetClipName() == currentClip.name)
        //        {
        //            if (animTrajectories[i].GetTrajectoryId() >= currentAnimId - 10 && animTrajectories[i].GetTrajectoryId() < currentAnimId)
        //            {
        //                continue; // Skip this candidate if it belong to the same animation, but at a previous frame
        //            }
        //        }
        //        if (animTrajectoriesInCharSpace[i].CompareTrajectoryPoints(movement) +
        //            animTrajectoriesInCharSpace[i].CompareTrajectoryForwards(movement) < threshold)
        //        { // TODO: Change to best # (KNN) for each anim type (misc tag, like left, forward, right) instead of threshold
        //            //Debug.Log("TrajComparisonDist: " + animTrajectoriesInCharSpace[i].CompareTrajectoryPoints(movement) +
        //            //          animTrajectoriesInCharSpace[i].CompareTrajectoryForwards(movement));
        //            trajectoryCandidates.Add(animTrajectoriesInCharSpace[i]);
        //        }
        //    }
        //}
        //return trajectoryCandidates;
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
