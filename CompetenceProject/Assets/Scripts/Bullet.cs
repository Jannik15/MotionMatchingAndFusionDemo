using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Vector3 startingPos;
	public float decayDist = 100;

	private void Start()
	{
		startingPos = transform.position;
	}
	private void Update()
	{
		if (Vector3.Distance(startingPos,transform.position) >= decayDist)
			Destroy(gameObject);
	}
	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Enemy")
        {
	        Destroy(col.gameObject);	
			Destroy(gameObject);
        }

	}
}
