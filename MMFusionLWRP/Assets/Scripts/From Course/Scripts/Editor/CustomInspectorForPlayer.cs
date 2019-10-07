using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSO))]
[CanEditMultipleObjects]
public class CustomInspectorForPlayer : Editor
{
    private PlayerSO player;
    private SerializedProperty guns;
	private float playerMinSpeed = 0;
	private float playerMaxSpeed = 1;

	private bool playerInfoIsOpen = true;
    private bool playerStatsIsOpen = true;
    private void OnEnable()
	{
		player = (PlayerSO) target;
        guns = serializedObject.FindProperty("guns");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
        playerInfoIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(playerInfoIsOpen, "Player Info");

        if (playerInfoIsOpen)
		{
			player.playerName = EditorGUILayout.TextField("Name: ", player.playerName);
			player.backStory = EditorGUILayout.TextField("Backstory: ", player.backStory);
		}

		EditorGUILayout.EndFoldoutHeaderGroup();

		EditorGUILayout.Space();

		playerStatsIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(playerStatsIsOpen, "Player Stats");

		if (playerStatsIsOpen)
        {
	        EditorGUILayout.PropertyField(guns);
            player.speed = EditorGUILayout.Slider("Speed: ", player.speed, playerMinSpeed, playerMaxSpeed);
            player.turnTime = EditorGUILayout.Slider("Turn speed: ", player.turnTime, playerMinSpeed, playerMaxSpeed);
        }

		EditorGUILayout.EndFoldoutHeaderGroup();

		serializedObject.ApplyModifiedProperties();
	}
}

