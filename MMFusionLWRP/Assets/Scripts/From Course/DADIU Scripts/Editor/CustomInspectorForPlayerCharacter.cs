using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCharacter))]
[CanEditMultipleObjects]
public class CustomInspectorForPlayerCharacter : Editor {

	private PlayerCharacter playerCharacter;

	private bool playerInfoIsOpen = true;
	private bool playerStatsIsOpen = false;

	private void OnEnable () {
		playerCharacter = ( PlayerCharacter ) target;

		if ( playerCharacter.playerStats == null ) {
			playerCharacter.playerStats = new Stats ();
		}
	}

	public override void OnInspectorGUI () {
		PlayerCharacter playerCharacter = ( PlayerCharacter ) target;

		playerInfoIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup ( playerInfoIsOpen, "Player Info" );

		if ( playerInfoIsOpen ) {
			playerCharacter.playerName = EditorGUILayout.TextField ( playerCharacter.playerName );
			playerCharacter.playerDescription = EditorGUILayout.TextArea ( playerCharacter.playerDescription );
		}

		EditorGUILayout.EndFoldoutHeaderGroup ();

		EditorGUILayout.Space ();

		playerStatsIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup ( playerStatsIsOpen, "Player Stats" );

		if ( playerStatsIsOpen ) {
			playerCharacter.playerStats.strength = EditorGUILayout.IntSlider ( playerCharacter.playerStats.strength, 1, 50 );
			playerCharacter.playerStats.magic = EditorGUILayout.IntSlider ( playerCharacter.playerStats.magic, 1, 50 );
			playerCharacter.playerStats.defence = EditorGUILayout.IntSlider ( playerCharacter.playerStats.defence, 0, 20 );
			playerCharacter.playerStats.magicDefence = EditorGUILayout.IntSlider ( playerCharacter.playerStats.magicDefence, 0, 20 );
		}

		EditorGUILayout.EndFoldoutHeaderGroup ();
	}
}