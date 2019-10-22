﻿using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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
	    Gizmos.color = Color.blue;
	    Gizmos.DrawLine(transform.position, desiredDir);
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

        for (int i = 0; i < points.Length; i++)
		{
			if (i > 0)
			{
				// Quaternion.LookRotation spams debug errors when input is vector3.zero, this removes that possibility
                Quaternion lookRotation = desiredDir != Vector3.zero ? Quaternion.LookRotation(desiredDir) : Quaternion.identity; // Shorthand if : else

                Vector3 tempPos = points[i - 1].GetPoint() + Vector3.Slerp(transform.position, desiredDir,
                                      mm.framesBetweenTrajectoryPoints / 100.0f * i) * speed;
                //Vector3 tempPos = points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation,
                //                      mm.framesBetweenTrajectoryPoints / 100.0f * i) * Vector3.forward * Mathf.Clamp(speed + 0.01f, -1.0f, 1.0f);

                Vector3 tempForward = tempPos + Quaternion.Slerp(transform.rotation, lookRotation, 
                                          mm.framesBetweenTrajectoryPoints / 100.0f * i) * Vector3.forward * Mathf.Clamp(speed + 0.01f, -1.0f, 1.0f);
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

        //Vector3 newPos = Vector3.Lerp(transform.position, transform.position + new Vector3(0.0f, 0.0f, Input.GetAxis("Vertical")) * movementSpeed.value, lerpTime.value);

        Quaternion rotation = Quaternion.Euler(0.0f,newRot.y,0.0f);
        
        transform.rotation = rotation /*Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime)*/;
        prevPos = transform.position;
        prevRot = transform.rotation.eulerAngles;

        //Debug.Log("Position is: " + transform.position + " | Desired direction is: " + (desiredDir * speed));

        desiredDir =
	        transform.worldToLocalMatrix.inverse.MultiplyPoint3x4(new Vector3(Input.GetAxis("Horizontal"), 0.0f,
		        Input.GetAxis("Vertical")) * Mathf.Clamp(speed + 0.1f, 0.0f, 1.0f));

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
            //transform.LookAt(goalPos);

            Quaternion rotation = goalPos - transform.position != Vector3.zero
                ? Quaternion.LookRotation(goalPos - transform.position) : Quaternion.identity; // Shorthand if : else
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);
            //transform.LookAt(newPos);
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
