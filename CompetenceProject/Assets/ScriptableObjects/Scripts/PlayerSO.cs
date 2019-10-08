using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Player", order = 0)]
public class PlayerSO : ScriptableObject
{
	public string playerName;
	public string backStory;
	public float speed;
	public float turnTime;
}
