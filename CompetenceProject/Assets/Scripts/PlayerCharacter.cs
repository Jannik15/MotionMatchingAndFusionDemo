using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

	public string playerName;
	public string playerDescription;
	public int playerID;

	public Stats playerStats;

}

public class Stats {
	public int strength;
	public int magic;
	public int defence;
	public int magicDefence;
}