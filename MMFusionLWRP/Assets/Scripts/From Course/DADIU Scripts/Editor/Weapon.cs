using System.Collections.Generic;
using UnityEngine;

public class Weapon : ScriptableObject {
	public string weaponName;
	public WeaponStats minimumStats;
	public WeaponStats requirementsToMeet;

	public List<Weapon> upgradesTo = new List<Weapon> ();
	public List<Weapon> upgradesFrom = new List<Weapon> ();

	public override string ToString () {
		return weaponName + " with requirements " + requirementsToMeet + ", minimum stats " + minimumStats.ToString () + ", " + upgradesFrom.Count + " downgrades and " + upgradesTo.Count + " upgrades";
	}
}