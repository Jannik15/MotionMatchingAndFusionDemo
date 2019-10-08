using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guns", menuName = "Guns", order = 1)]
public class GunSO : ScriptableObject
{
	public Guns guns;
	public enum Guns
	{
		Pistol = 0,
		Automatic = 1,
		Shotgun = 2
	}
	public GameObject bullet;
    public float firingRate;
    public float bulletSpread;
	public float bulletSpeed;
}
