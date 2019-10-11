using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	public float lerpTime = 1, movementSpeed = 0.01f, joyMovementSpeed;
	private Vector3 prevPos, goalPos;

    private string movementType;
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
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
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
        if(Input.GetMouseButton(0)) {
            goalPos = Input.mousePosition;
            transform.LookAt(goalPos);
            transform.position = Vector3.Lerp(transform.position, goalPos, movementSpeed*Time.deltaTime);
        }
    }

    public void ClickAndDrag() {
        // Centered on player character/ middle of screen.
        // Could work with Vector3.Distance(transform.position, Input.MousePosition);
        // moveSpeed increments with the Distance, maybe addForce?

        goalPos = Input.mousePosition;
        transform.LookAt(goalPos);
        joyMovementSpeed = movementSpeed * (Vector3.Distance(transform.position, goalPos) / 2);
        transform.position = Vector3.Lerp(transform.position, goalPos, movementSpeed * Time.deltaTime);

    }
}
