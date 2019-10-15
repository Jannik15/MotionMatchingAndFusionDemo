using System.Collections;
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
    private string[] allClipTags;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
    public Cliptags tagContainer; // put ref to chosen tag container scriptable object
    public string[] jointNames;
    public static string[] movementTag = {"idle", "walk", "run"};

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
        allClipTags = tagContainer.clipTags;
        if (allClips == null)
            Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");

#if UNITY_EDITOR
        if (_preProcess)
        {
            animContainer.animationClips = preProcessing.FindClipsFromAnimatorController();
            allClips = animContainer.animationClips;
            tagContainer.clipTags = preProcessing.GenerateClipTags(allClips, movementTag);
            allClipTags = tagContainer.clipTags;
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
	    if (!_isMotionMatching && movement.GetSpeed() > idleThreshold)
	    {
			StopAllCoroutines();
		    StartCoroutine(MotionMatch());
	    }
        //if (!_isIdling  && movement.GetSpeed() <= idleThreshold)
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
                //Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                Gizmos.DrawLine(i != 0 ? movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : transform.position,
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
		Debug.Log("Updating animation (ID): "+ currentID + " -> " + id);
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
            List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(), candidatesPerMisc);
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
            int candidateID = PoseMatching(featureVectors);
            UpdateAnimation(candidateID, (int)featureVectors[candidateID].GetFrame());
            yield return new WaitForSeconds((currentFrame - currentClip.length) / currentClip.frameRate);
	    }
    }
    #endregion

    List<FeatureVector> TrajectoryMatching(Trajectory movement, int candidatesPerMisc)
    {
		List<FeatureVector> candidates = new List<FeatureVector>();

		for (int i = 0; i < featureVectors.Count; i++)
		{
            if (( featureVectors[i].GetID() > currentID ||  featureVectors[i].GetID() < currentID - queryRateInFrames) &&
                 featureVectors[i].GetFrame() + queryRateInFrames <=  featureVectors[i].GetFrameCountForID())
            { // TODO: Take KNN candidates for each animation 
	            candidates.Add( featureVectors[i]);
            }
        }
        return candidates;
    }

    private int PoseMatching(List<FeatureVector> candidates)
    {
        int bestId = -1;
        float currentDif = float.MaxValue;

        foreach (var candidate in candidates)
        {
	        float candidateDif =  featureVectors[currentID].ComparePoses(candidate, allClips[0].frameRate,
		        weightLFootVel, weightRFootVel, weightRootVel);
            if (candidateDif < currentDif)
            {
				//Debug.Log("Candidate diff: " + candidateDif + " < " + " Current diff:" + currentDif);
                bestId = candidate.GetID();
                currentDif = candidateDif;
            }
        }
        return bestId;
    }

}
