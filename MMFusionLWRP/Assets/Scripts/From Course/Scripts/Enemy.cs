using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public EnemySO enemy;
	private Vector3 target;

    private void Update()
    {
        target = FindObjectOfType<Player>().gameObject.transform.position;
        Vector3 newPos = Vector3.MoveTowards(transform.position, target, enemy.speed / 100);
		transform.LookAt(newPos);
        transform.position = newPos;
    }
}
