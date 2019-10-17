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
    public FloatReference lerpTime, movementSpeed, movementMultiplier;
    public Vector3 posHolder;

    // --- Private 
    private Vector3 prevPos, goalPos;

    private float speed;

    private string movementType;

    private void Awake()
    {
	    mm = GetComponent<MotionMatching>();
    }
    private void FixedUpdate()
    {
	    UpdateSpeed();
        switch (movementType) {
            case "wasd":
                posHolder = transform.position;
                KeyBoardMove();
                break;

            case "joystick":
                posHolder = transform.position;
                ClickAndDrag();
                if(transform.position != goalPos) {
                    goalPos = transform.position;
                }
                
                break;

            case "moveToPoint":
                posHolder = transform.position;
                MoveToMouse();
                break;

            default:
                posHolder = transform.position;
                movementType = "wasd";
                //Debug.Log("Unknown movetype");
                break;
        }

        if(Input.GetKeyDown("o")) {
            ChangeMovement();
        }
    }

    private void UpdateSpeed()
    {
	    speed = (transform.position - prevPos).magnitude / Time.fixedDeltaTime;
	    prevPos = transform.position;
    }

    public float GetSpeed()
    {
	    return speed;
    }

    public Trajectory GetMovementTrajectory()
    {
		TrajectoryPoint[] points = new TrajectoryPoint[mm.pointsPerTrajectory];
		for (int i = 0; i < points.Length; i++)
		{
			if (i > 0)
			{
				// Quaternion.LookRotation spams debug errors when input is vector3.zero, this removes that possibility
				Quaternion lookRotation = posHolder + transform.forward != Vector3.zero
					? Quaternion.LookRotation(posHolder + transform.forward) : Quaternion.identity; // Shorthand if : else

				Vector3 tempPos = points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, movementSpeed.value/*(mm.framesBetweenTrajectoryPoints / 100.0f)*/ * i) * Vector3.forward * Mathf.Clamp(speed + 0.1f, 0.0f, 1.0f);
				Vector3 tempForward = tempPos + Quaternion.Slerp(transform.rotation, lookRotation, (mm.framesBetweenTrajectoryPoints / 100.0f) * i) * Vector3.forward * Mathf.Clamp(speed + 0.1f, 0.0f, 1.0f);
                points[i] = new TrajectoryPoint(tempPos, tempForward);
			}
			else
				points[i] = new TrajectoryPoint(posHolder, posHolder + transform.forward);
		}
		return new Trajectory(points);
    }

    public Vector3 GetMovementVelocity()
    {
	    return (transform.position - prevPos) / Time.fixedDeltaTime;
    }

    public void KeyBoardMove() {
        prevPos = posHolder;
	    Vector3 newPos = Vector3.Lerp(posHolder, posHolder + new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * movementSpeed.value, lerpTime.value);
        Quaternion rotation = newPos - posHolder != Vector3.zero
            ? Quaternion.LookRotation(newPos - posHolder) : Quaternion.identity; // Shorthand if : else
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);
        //transform.LookAt(newPos);
        posHolder = newPos;

        if (CheckRotateToMove(rotation))
            transform.position = newPos; // Just draw curves simulating the movement, instead of actually moving the player
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
                    transform.LookAt(goalPos);
                    transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value) * Time.deltaTime);
            }
                
            
        


    }

    public void ClickAndDrag() {
        if(Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                goalPos = hit.point;
                transform.LookAt(new Vector3(hit.point.x, 0, hit.point.z));
                transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value ) * Time.deltaTime);
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

        //Debug.Log(rotation.eulerAngles.y + " " + lowerBound + " " + upperBound);

        if (rotation.eulerAngles.y <= upperBound &&
            rotation.eulerAngles.y >= lowerBound)
            tempBool = true;
        
        return tempBool;
    }

}
