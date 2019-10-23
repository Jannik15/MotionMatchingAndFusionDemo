using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    // For every variable used, make a scriptableObject with the type. So make a scriptable object script that takes a single float,
    // For every data type, have a TypeReference for the scriptableObject. Create new SOs through asset menu for each variable here. 


    // GetMovementTrajectoryCharacterSpace function. Currently movement is in worldspace, and animations in character space,
    // We need a function to change that, so the movements are properly applied to the character in the right space. 
    
    // --- References
    private MotionMatching mm;

	// --- Public
    [Tooltip("In degrees")] public float rotateToMoveThreshold = 15f;
    public float rotateSpeed = 2.0f;
    public FloatReference lerpTime, movementSpeed, movementMultiplier;

    // --- Private 
    private Vector3 prevPos, prevRot, goalPos, desiredDir;

	[SerializeField]
    private float speed, angSpeed, rotationValue;

    private string movementType;

    private void Awake()
    {
	    mm = GetComponent<MotionMatching>();
    }
	
    private void FixedUpdate()
    {
	    UpdateSpeed();
        UpdateAngularSpeed();
        switch (movementType) {
            case "wasd":
                KeyBoardMove();
                break;

            case "joystick":
                ClickAndDrag();
                if(transform.position != goalPos) {
                    goalPos = transform.position;
                }
                
                break;

            case "moveToPoint":
                MoveToMouse();
                break;

            default:
                movementType = "wasd";
                //Debug.Log("Unknown movetype");
                break;
        }

        if(Input.GetKeyDown("o")) {
            ChangeMovement();
        }
    }

    private void OnDrawGizmos()
    {
	    if (Application.isPlaying)
	    {
		    Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
		    Matrix4x4 charSpace = transform.localToWorldMatrix;
		    Matrix4x4 newSpace = new Matrix4x4();
		    newSpace.SetTRS(transform.position, Quaternion.identity, transform.lossyScale);

		    Gizmos.color = Color.red; // Movement Trajectory
		    for (int i = 0; i < GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
		    {
			    // Position
			    Gizmos.DrawWireSphere(GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
			    Gizmos.DrawLine(i != 0 ? GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : transform.position,
				    GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());

                // Forward
                Gizmos.DrawLine(GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(),
                 GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
            }

		    Gizmos.color = Color.blue;
		    Gizmos.DrawLine(Vector3.zero, desiredDir);
		    Gizmos.color = Color.magenta;
		    Matrix4x4 transformRotMatrix = new Matrix4x4();
		    transformRotMatrix.SetTRS(Vector3.zero, transform.rotation, Vector3.one);
            Gizmos.DrawLine(Vector3.zero, transformRotMatrix.MultiplyPoint3x4(desiredDir));
	    }
    }

    private void UpdateSpeed()
    {
	    speed = (transform.position - prevPos).magnitude / Time.fixedDeltaTime;
	    speed = speed / 3.0f;
	    prevPos = transform.position;
    }

    private void UpdateAngularSpeed()
    {
        angSpeed = (transform.rotation.eulerAngles - prevRot).magnitude / Time.fixedDeltaTime;
        prevRot = transform.rotation.eulerAngles;
    }

    public float GetSpeed()
    {
	    return speed;
    }

    public float GetAngularSpeed()
    {
        return angSpeed;
    }

    public Trajectory GetMovementTrajectory()
    {
        TrajectoryPoint[] points = new TrajectoryPoint[mm.pointsPerTrajectory];
        float tempSpeed = speed >= 0.1f ? Mathf.Clamp(speed, 0.1f, 1.0f) : 1.0f;

        Matrix4x4 transformRotMatrix = new Matrix4x4();
        transformRotMatrix.SetTRS(Vector3.zero, transform.rotation, Vector3.one);
        Vector3 charDesiredDir = transformRotMatrix.MultiplyPoint3x4(desiredDir);
        // Quaternion.LookRotation spams debug errors when input is vector3.zero, this removes that possibility
        Quaternion lookRotation = charDesiredDir != Vector3.zero ? Quaternion.LookRotation(charDesiredDir) : transform.rotation; // Shorthand if : else
        Debug.Log("rotation: " + transform.rotation + " | lookrotation: " + lookRotation + " | desired dir: " + desiredDir + " | charDesired dir: " + charDesiredDir);
        for (int i = 0; i < points.Length; i++)
		{
			if (i > 0) // TODO: Movement trajectory is too aggressive compared to the actual movement - check the movement script and make sure the desired dir is = used input
			{
                Vector3 tempPos = points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, (float)(i + 1) / points.Length) * (desiredDir * Mathf.Clamp(speed + 0.1f, -1.0f, 1.0f));

                Vector3 tempForward = tempPos + Quaternion.Slerp(transform.rotation, lookRotation, (float)(i + 1) / points.Length) * Vector3.forward;
                points[i] = new TrajectoryPoint(tempPos, tempForward);
			}
			else
				points[i] = new TrajectoryPoint(transform.position, transform.position +  transform.forward);
		}
		return new Trajectory(points);
    }

    public Vector3 GetMovementVelocity()
    {
	    return (transform.position - prevPos) / Time.fixedDeltaTime;
    }

    public void KeyBoardMove()
    {
        rotationValue = (rotationValue + (Input.GetAxis("Horizontal") * rotateSpeed)) % 360;
        Vector3 newRot = new Vector3(0.0f, rotationValue, 0.0f);
        Quaternion rotation = Quaternion.Euler(0.0f,newRot.y,0.0f);

        transform.rotation = rotation;
        prevPos = transform.position;
        prevRot = transform.rotation.eulerAngles;
        Matrix4x4 unrotatedTransform = new Matrix4x4();
        unrotatedTransform.SetTRS(transform.position, Quaternion.identity, Vector3.one);
        Vector3 desiredPos = unrotatedTransform.MultiplyPoint3x4(new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")));
        desiredDir = desiredPos - transform.position;

        if (Input.GetAxis("Vertical") >= 0.1f || Input.GetAxis("Vertical") <= -0.1f)
	        transform.position = prevPos + transform.forward * Input.GetAxis("Vertical") * movementSpeed.value;
    }
    public void MoveToMouse() {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) {
                goalPos = hit.point;
            }
        }

        if(Vector3.Distance(transform.position, goalPos) > 0.2f) {
            Quaternion rotation = goalPos - transform.position != Vector3.zero
                ? Quaternion.LookRotation(goalPos - transform.position) : Quaternion.identity; // Shorthand if : else
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);
            prevPos = transform.position;
            prevRot = transform.rotation.eulerAngles;

            if (CheckRotateToMove(rotation))
                transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value) * Time.fixedDeltaTime);
        }
    }

    public void ClickAndDrag() {
        if(Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) {
                goalPos = new Vector3(hit.point.x, 0.0f, hit.point.z);

                Quaternion rotation = goalPos - transform.position != Vector3.zero
                    ? Quaternion.LookRotation(goalPos - transform.position) : Quaternion.identity; // Shorthand if : else
                //transform.LookAt(new Vector3(hit.point.x, 0, hit.point.z));

                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);


                if (CheckRotateToMove(rotation))
                    transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value ) * Time.fixedDeltaTime);
            }
        }
    }

    public void ChangeMovement() {
        if(movementType == "wasd") {
            movementType = "joystick";
            Debug.Log("Movement type is: " + movementType);
        }

        else if(movementType == "joystick") {
            movementType = "moveToPoint";
            Debug.Log("Movement type is: " + movementType);
        }

        else if(movementType == "moveToPoint") {
            movementType = "wasd";
            Debug.Log("Movement type is: " + movementType);
        }
    }

    private bool CheckRotateToMove(Quaternion rotation)
    {
        bool tempBool = false;
        float rotationChecker = transform.rotation.eulerAngles.y;
        float upperBound = (rotationChecker + rotateToMoveThreshold) % 360f, lowerBound = rotationChecker - rotateToMoveThreshold;

        if (rotationChecker - rotateToMoveThreshold < 0)
        {
            rotationChecker += 360;
            lowerBound = rotationChecker;
        } 
        
        if (lowerBound > upperBound)
        {
            if (rotation.eulerAngles.y <= upperBound)
            {
                lowerBound = upperBound - rotateToMoveThreshold * 2;
            }
            else if (rotation.eulerAngles.y >= lowerBound)
            {
                upperBound = lowerBound + rotateToMoveThreshold * 2;
            }
        }

        if (rotation.eulerAngles.y <= upperBound &&
            rotation.eulerAngles.y >= lowerBound)
            tempBool = true;
        
        return tempBool;
    }

}
