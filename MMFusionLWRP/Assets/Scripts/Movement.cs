using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	public float lerpTime = 1, movementSpeed = 0.01f;
    void Update()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, transform.position + new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * movementSpeed, lerpTime);
        transform.LookAt(newPos);
        //transform.position = newPos; // Just draw curves simulating the movement, instead of actually moving the player
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
