using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    // For every variable used, make a scriptableObject with the type. So make a scriptable object script that takes a single float,
    // For every data type, have a TypeReference for the scriptableObject. Create new SOs through asset menu for each variable here. 
    // --- References
    private MotionMatching mm;

	// --- Public
    public float lerpTime = 1, movementSpeed = 0.01f;

    // --- Private 
    private Vector3 prevPos, goalPos, direction;

    private string movementType;


    private void Awake()
    {
	    mm = GetComponent<MotionMatching>();
        
    }
    private void FixedUpdate()
    {
        
        switch(movementType) {
            case "wasd":
                KeyBoardMove();
                break;

            case "joystick":
                ClickAndDrag();
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
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }

    public Trajectory GetMovementTrajectory()
    {
		TrajectoryPoint[] points = new TrajectoryPoint[mm.pointsPerTrajectory];
		for (int i = 0; i < points.Length; i++)
		{
			if (i > 0)
			{
				// Quaternion.LookRotation spams debug errors when input is vector3.zero, this removes that possibility
				Quaternion lookRotation = transform.position + transform.forward != Vector3.zero
					? Quaternion.LookRotation(transform.position + transform.forward) : Quaternion.identity; // Shorthand if : else

				points[i] = new TrajectoryPoint(points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, (mm.framesBetweenTrajectoryPoints / 100.0f) * i) * Vector3.forward,
					(points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, (mm.framesBetweenTrajectoryPoints / 100.0f) * i) * Vector3.forward - points[i-1].GetPoint()).normalized);
			}
			else
				points[i] = new TrajectoryPoint();
		}
		return new Trajectory(points);
    }

    public Vector3 GetMovementVelocity()
    {
	    return (transform.position - prevPos) / Time.fixedDeltaTime;
    }

    public void KeyBoardMove() {
        prevPos = transform.position;
	    Vector3 newPos = Vector3.Lerp(transform.position, transform.position + new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * movementSpeed, lerpTime);
	    transform.LookAt(newPos);
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
        // Need some way to reset, or tell if other functions affected goalPos. Or reset this goalPos when you change.
            if(Vector3.Distance(transform.position, goalPos) > 0.2f) {
                    transform.LookAt(goalPos);
                    transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed * 5) * Time.deltaTime);
            }
                
            
        


    }

    public void ClickAndDrag() {
        if(Input.GetMouseButton(0)) {
            //goalPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //Vector3 middleScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, transform.position.y, Screen.height/2));
            
            if(Physics.Raycast(ray, out hit)) {
                //Debug.Log(hit.point.x + hit.point.z);
                //Vector3 direction = hit.point - transform.position;
                //direction.y = 0;
                transform.LookAt(new Vector3(hit.point.x, 0, hit.point.z));
                transform.position = Vector3.Lerp(transform.position, hit.point, (movementSpeed * 5 ) * Time.deltaTime);
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
}
