﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MotionMatching : MonoBehaviour
{
    // TODO: Create LookUp system in preproccesing, that can be used instead of pose matching during runtime
    // TODO: Convert system to Unity Jobs - can only take NativeArrays<float3>
    // TODO: When preprocessing, also store the data that is being written to CSV as return to feature vector (do load and write step together when preprocessing)
    // TODO: Do correct char space conversion
    // TODO: Check that forwards for trajectories are being created correctly
    // TODO: Create bool for using misc or not, since our current misc system doesn't really make sense
    // TODO: Create some debugger that shows various information about the data, especially the trajectory for each frame
    // BUG: Might need to remove the root position to get correct values for the curves(?)
    // TODO: Collision detection with raycasting between the trajectory points
    // TODO: At frame 0, set the velocity to be frame 0 pos - frame 1 pos (absolute value)
    // TODO: Extrapolate empty trajectorypoints (points that go over the frame size for that clip)

    // TODO: https://docs.unity3d.com/ScriptReference/AnimationClip.SampleAnimation.html
    // TODO: https://docs.unity3d.com/ScriptReference/HumanBodyBones.html


    // --- References
    private Movement movement;
    private PreProcessing preProcessing;
    private Animator animator;

    // --- Collections
    private List<FeatureVector> featureVectors;
    private AnimationClip[] allClips;
    public HumanBodyBones[] joints;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
    public string[] jointNames;
    public string[][] movementTags =
    {
        new []{ "Idle"},                // State 0
        new []{ "Walk", "Run" }         // State 1
    };

    private List<bool> enumeratorBools;

    // --- Variables 
    public bool _preProcess, _playAnimationMode;
    public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
    public float idleThreshold = 0.10f;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc = 10;

    private AnimationClip currentClip;
    private int currentFrame, currentID, currentState;

    // --- Weights
    [Range(0, 1)]
    public float weightLFootVel = 1.0f, weightRFootVel = 1.0f, weightRootVel = 1.0f, 
	    weightFeetPos = 1.0f, weightTrajPoints = 1.0f, weightTrajForwards = 1.0f;

    // --- Debugstuff
    private int animIterator = -1;
    private IEnumerator currentEnumerator;

    private void Awake() // Load animation data
    {
        movement = GetComponent<Movement>();
	    animator = GetComponent<Animator>();
        preProcessing = new PreProcessing();

        allClips = animContainer.animationClips;
#if UNITY_EDITOR
        if (_preProcess)
        {
            // Get animations from animation controller, and store it in a scriptable object
            allClips = preProcessing.FindClipsFromAnimatorController();
            AnimContainer tempAnimContainer = new AnimContainer();
            tempAnimContainer.animationClips = allClips;
            EditorUtility.CopySerialized(tempAnimContainer, animContainer);
            AssetDatabase.SaveAssets();

            preProcessing.Preprocess(allClips, joints, gameObject, animator);
        }

        if (allClips == null)
            Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");
#endif

        allClips = animContainer.animationClips;
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

        //for (int i = 0; i < featureVectors.Count; i++)
        //{
        //    Debug.Log("For clip " + featureVectors[i].GetClipName() + " at frame " + featureVectors[i].GetFrame() + " the frame count is " + featureVectors[i].GetFrameCountForID());
        //}

        for (int i = 0; i < movementTags.Length; i++)
        {
            for (int j = 0; j < movementTags[i].Length; j++)
            {
                movementTags[i][j] = movementTags[i][j].ToLower();
            }
        }

		// TODO: Remove
   //     for (int i = 0; i < featureVectors.Count; i++)
   //     {
			//if (i != 0)
			//	featureVectors[i].CalculateVelocity(featureVectors[i - 1].GetPose(), transform.worldToLocalMatrix, allClips[0].frameRate);
   //         else
   //         {
   //             featureVectors[i].CalculateVelocity(featureVectors[i + 1].GetPose(), transform.worldToLocalMatrix, allClips[0].frameRate);
   //             featureVectors[i].SetRootVelocity(AbsVector3(featureVectors[i].GetRootVelocity()));
   //             featureVectors[i].SetLeftFootVelocity(AbsVector3(featureVectors[i].GetLeftFootVelocity()));
   //             featureVectors[i].SetRightFootVelocity(AbsVector3(featureVectors[i].GetRightFootVelocity()));
   //         }
   //     }
        enumeratorBools = AddEnumeratorBoolsToList();
    }

    private void Start()
    {
        // --- Instantiation
        if (!_playAnimationMode)
        {
	        UpdateAnimation(0, 0);
	        StartCoroutine(MotionMatch());
        }
    }

    private void FixedUpdate()
    {
        if (!_playAnimationMode)
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
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (_playAnimationMode)
	    {
		    if (Input.GetKeyDown(KeyCode.Space))
		    {
			    if (currentEnumerator != null)
				    StopCoroutine(currentEnumerator);
			    transform.position = Vector3.zero;
			    StartCoroutine(PlayAnimation());
		    }
        }
    }
#endif

    private void OnDrawGizmos()
    {
	    if (Application.isPlaying)
	    {
		    Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
		    Matrix4x4 charSpace = transform.localToWorldMatrix;
		    Matrix4x4 newSpace = new Matrix4x4();
		    newSpace.SetTRS(transform.position, Quaternion.identity, transform.lossyScale);

            Gizmos.color = Color.red; // Movement Trajectory
            for (int i = 0; i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
		    {
				// Position
                Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                Gizmos.DrawLine(i != 0 ? movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : transform.position,
	                movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());

                // Forward
				Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(),
					movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
		    }

            Gizmos.color = Color.green; // Animation Trajectory
            for (int i = 0; i < featureVectors[currentID].GetTrajectory().GetTrajectoryPoints().Length; i++)
		    {
				// Position
				Gizmos.DrawWireSphere(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
				//if (i != 0)
				//	Gizmos.DrawLine(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i - 1].GetPoint(), 
				//		featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint());

				// Forward
				Gizmos.DrawLine(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint(),
					featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetForward());
		    }

   //         Gizmos.color = Color.magenta;
			//Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetPose().GetRootPos()), 0.1f);
			//Gizmos.color = Color.yellow; // Pose positions
   //         Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetPose().GetLeftFootPos()), 0.1f);
   //         Gizmos.color = Color.blue;
			//Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetPose().GetRightFootPos()), 0.1f);

			//Gizmos.color = Color.cyan;
			//Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(Vector3.zero), invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetRootVelocity()));
			//Gizmos.color = Color.gray;
			//Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(Vector3.zero), invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetLeftFootVelocity()));
			//Gizmos.color = Color.white;
   //         Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(Vector3.zero), invCharSpace.MultiplyPoint3x4(featureVectors[currentID].GetRightFootVelocity()));

        }
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
		Debug.Log("Updating animation " + currentClip.name + " to frame " + frame + " with ID " + id);
        animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0, frame / currentClip.frameRate); // 0.3f was recommended by Magnus
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
			UpdateAnimation(candidateID, featureVectors[candidateID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
	    }
    }
    private IEnumerator PlayAnimation()
    {
	    animIterator++;
	    if (animIterator == allClips.Length)
	    {
		    animIterator = 0;
		    Debug.Log("Finished playing last animation!");
	    }
        for (int i = 0; i < featureVectors.Count; i++)
	    {
		    if (featureVectors[i].GetClipName() == allClips[animIterator].name)
		    {
			    currentID = i;
			    break;
            }
	    }
	    int startofIdForClip = currentID;
	    Debug.Log("Current playing " + allClips[animIterator].name + ", which is " + allClips[animIterator].length + " seconds long!");
		Debug.Log("While loop condition: Frame " + featureVectors[currentID].GetFrame() + " < " + featureVectors[currentID].GetFrameCountForID() + " && Clip " + featureVectors[currentID].GetClipName() + " == " + allClips[animIterator].name);
        while (featureVectors[currentID].GetFrame() < featureVectors[currentID].GetFrameCountForID() && featureVectors[currentID].GetClipName() == allClips[animIterator].name)
        {
            Debug.Log("Current ID is now " + currentID + ", which started at ID " + startofIdForClip +"!");
		    UpdateAnimation(currentID, featureVectors[currentID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
            currentID += queryRateInFrames;
        }
    }
    #endregion

    List<FeatureVector> TrajectoryMatching(Trajectory movementTraj)
    {
        List<FeatureVector> candidates = new List<FeatureVector>();
        FeatureVector[,] possibleCandidates = new FeatureVector[movementTags[currentState].Length, candidatesPerMisc];
        float[,] values = new float[movementTags[currentState].Length, candidatesPerMisc];
        int currentMisc = currentState;

        for (int i = 0; i < featureVectors.Count; i++)
		{
            if (!TagChecker(featureVectors[i].GetClipName(), currentState))
                continue;
            if ((featureVectors[i].GetID() > currentID ||  featureVectors[i].GetID() < currentID - queryRateInFrames) &&
                 featureVectors[i].GetFrame() + queryRateInFrames <  featureVectors[i].GetFrameCountForID())
            {
                for (int j = 0; j < movementTags[currentState].Length; j++) // This for loop can be used if looking to consider multiple miscs for the current state during trajectory matching
                {
                    if (TagChecker(featureVectors[i].GetClipName(), currentState, j))
                    {
	                    currentMisc = j;
	                    break;
                    }
                }
                float tempVal = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj, transform.worldToLocalMatrix.inverse, weightTrajPoints, weightTrajForwards);
                float comparison = tempVal;
                int indexOfHighestValue = 0;
                for (int j = 0; j < possibleCandidates.GetLength(1); j++)
                {
                    if (possibleCandidates[currentMisc, j] != null)
                    {
                        if (comparison < values[currentMisc, j])
                        {
                            comparison = values[currentMisc, j];
                            indexOfHighestValue = currentMisc;
                        }
                    }
                    else
                    {
                        possibleCandidates[currentMisc, j] = featureVectors[i];
                        values[currentMisc, j] = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj, transform.worldToLocalMatrix, weightTrajPoints, weightTrajForwards);
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
        //Debug.Log("Pose matching for " + candidates.Count + " candidates!");
        foreach (var candidate in candidates)
        {
            //float velDif = featureVectors[currentID].ComparePoses(candidate, transform.worldToLocalMatrix, weightLFootVel, weightRFootVel, weightRootVel);
            float feetPosDif = featureVectors[currentID].GetPose().GetFeetDistance(candidate.GetPose(), transform.worldToLocalMatrix, weightFeetPos);
            float candidateDif = /*velDif*/ + feetPosDif;
            if (candidateDif < currentDif)
            {
				//Debug.Log("Candidate diff: " + velDif + " < " + " Current diff:" + currentDif);
                bestId = candidate.GetID();
                currentDif = candidateDif;
            }
        }
		//Debug.Log("Returning best id from pose matching: " + bestId);
		return bestId;
    }

    private bool TagChecker(string candidateName, int stateNumber)
    {
        for (int i = 0; i < movementTags[stateNumber].Length; i++)
        {
            if (candidateName.ToLower().Contains(movementTags[stateNumber][i]))
                return true;
        }
        return false;
    }
    private bool TagChecker(string candidateName, int stateNumber, int miscNumber)
    {
	    if (candidateName.ToLower().Contains(movementTags[stateNumber][miscNumber]))
		    return true;
        return false;
    }

    private Vector3 AbsVector3(Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Math.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public List<FeatureVector> GetFeatureVectors()
    {
	    return featureVectors;
    }
}