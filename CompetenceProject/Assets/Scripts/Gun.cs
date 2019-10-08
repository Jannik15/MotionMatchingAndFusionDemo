using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : ScriptableObject
{
	public string gunName;
	public GunStats stats;
	public GameObject bullet;
	public float bulletSpeed = 10;

	public List<Gun> upgradesTo = new List<Gun>();
	public List<Gun> upgradesFrom = new List<Gun>();
}
