using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Node {
	public Rect nodeRect;
	public Rect collectiveRect;
	public string title;
	public bool isDragged;
	public bool isSelected;

	public ConnectionPoint inPoint;
	public ConnectionPoint outPoint;

	public GUIStyle style;
	public GUIStyle defaultNodeStyle;
	public GUIStyle selectedNodeStyle;

	public Action<Node> OnRemoveNode;

	public DrawableInfo myInfo;

	public Node ( Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode, DrawableInfo info ) {
		nodeRect = new Rect ( position.x, position.y, width, height );
		style = nodeStyle;
		inPoint = new ConnectionPoint ( this, ConnectionPointType.In, inPointStyle, OnClickInPoint );
		outPoint = new ConnectionPoint ( this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint );
		defaultNodeStyle = nodeStyle;
		selectedNodeStyle = selectedStyle;
		OnRemoveNode = OnClickRemoveNode;
		myInfo = info;
		myInfo.style = style;
		collectiveRect = new Rect ( nodeRect.position.x, nodeRect.position.y, nodeRect.size.x, nodeRect.size.y + myInfo.GetHeight () + ( nodeRect.size.y / 2f ) );
	}

	public void Drag ( Vector2 delta ) {
		nodeRect.position += delta;
		collectiveRect = new Rect ( nodeRect.position.x, nodeRect.position.y, nodeRect.size.x, nodeRect.size.y + myInfo.GetHeight () + ( nodeRect.size.y / 2f ) );
	}

	public virtual void Draw () {
		inPoint.Draw ();
		outPoint.Draw ();

		GUI.BeginGroup ( new Rect ( nodeRect.position.x + 5f, nodeRect.position.y + nodeRect.size.y / 2f, nodeRect.size.x - 10f, nodeRect.size.y + myInfo.GetHeight () ), style );
		//GUILayout.BeginArea ( collectiveRect );

		myInfo.Draw ( collectiveRect, style );

		//GUI.Box ( new Rect ( 0, 0, collectiveRect.size.x - 10f, myInfo.height ), "I am box!", style );
		//GUI.Button ( new Rect ( 0, myInfo.height, 100f, myInfo.height ), "I am button!", style );

		//GUILayout.Button ( "", style );
		//myInfo.Draw ( nodeRect, style );

		//GUILayout.EndArea ();
		GUI.EndGroup ();

		GUI.Box ( nodeRect, "", style );
		EditorGUI.LabelField ( nodeRect, myInfo.title, style );
	}

	public bool ProcessEvents ( Event e ) {
		switch ( e.type ) {
			case EventType.MouseDown:
				if ( e.button == 0 ) {
					if ( collectiveRect.Contains ( e.mousePosition ) ) {
						isDragged = true;
						GUI.changed = true;
						isSelected = true;
						style = selectedNodeStyle;
					} else {
						GUI.changed = true;
						isSelected = false;
						style = defaultNodeStyle;
						GUI.FocusControl ( null );
					}
				}

				if ( e.button == 1 && isSelected && collectiveRect.Contains ( e.mousePosition ) ) {
					ProcessContextMenu ();
					e.Use ();
				}
				break;

			case EventType.MouseUp:
				isDragged = false;
				break;

			case EventType.MouseDrag:
				if ( e.button == 0 && isDragged ) {
					Drag ( e.delta );
					e.Use ();
					return true;
				}
				break;
		}

		return false;
	}

	private void ProcessContextMenu () {
		GenericMenu genericMenu = new GenericMenu ();
		genericMenu.AddItem ( new GUIContent ( "Remove node" ), false, OnClickRemoveNode );
		genericMenu.ShowAsContext ();
	}

	private void OnClickRemoveNode () {
		if ( OnRemoveNode != null ) {
			OnRemoveNode ( this );
		}
	}
}

public abstract class DrawableInfo {
	public string title;
	protected float height = 120f;
	public GUIStyle style;

	public abstract void Draw ( Rect rect, GUIStyle style );
	public abstract float GetHeight ();
}

public class DrawableWeapon : DrawableInfo {

	private DrawableRequirements requirements;
	private WeaponStats minimumStats;

	public DrawableWeapon () {
		title = "New Weapon";
		minimumStats = new WeaponStats ();
		requirements = new DrawableRequirements ();
		height = 300f + requirements.GetHeight ();
	}

	public override void Draw ( Rect rect, GUIStyle style ) {
		Rect baseRect = new Rect ( 10f, 30f, rect.size.x - 30f, 30f );

		requirements.Draw ( baseRect, style );

		baseRect = new Rect ( baseRect.position.x, baseRect.position.y + requirements.GetHeight (), baseRect.size.x, baseRect.size.y );

		EditorGUI.LabelField ( baseRect, "Weapon" );

		float marginLeft = 5f;
		float marginRight = marginLeft + 0f;

		GUI.Label ( new Rect ( baseRect.position.x + marginLeft, baseRect.position.y + 30f, baseRect.size.x - marginRight, baseRect.size.y / 2f ), "Title" );
		title = EditorGUI.TextField ( new Rect ( baseRect.position.x + marginLeft, baseRect.position.y + 45f, baseRect.size.x - marginRight, baseRect.size.y / 2f ), title );

		minimumStats.attack.stat = DrawStat ( "Attack", baseRect, marginLeft, marginRight, 60f, minimumStats.attack.stat, 1, 255 );
		minimumStats.durability.stat = DrawStat ( "Durability", baseRect, marginLeft, marginRight, 30f + 60f, minimumStats.durability.stat, 20, 255 );
		minimumStats.flame.stat = DrawStat ( "Flame", baseRect, marginLeft, marginRight, 60f +  60f, minimumStats.flame.stat, 0, 255 );
		minimumStats.chill.stat = DrawStat ( "Chill", baseRect, marginLeft, marginRight, 90f +  60f, minimumStats.chill.stat, 0, 255 );
		minimumStats.lightning.stat = DrawStat ( "Lightning", baseRect, marginLeft, marginRight, 120f + 60f, minimumStats.lightning.stat, 0, 255 );
		minimumStats.cyclone.stat = DrawStat ( "Cyclone", baseRect, marginLeft, marginRight, 150f + 60f, minimumStats.cyclone.stat, 0, 255 );
		minimumStats.exorcism.stat = DrawStat ( "Exorcism", baseRect, marginLeft, marginRight, 180f + 60f, minimumStats.exorcism.stat, 0, 255 );
		minimumStats.beast.stat = DrawStat ( "Beast", baseRect, marginLeft, marginRight, 210f + 60f, minimumStats.beast.stat, 0, 255 );
		minimumStats.scale.stat = DrawStat ( "Scale", baseRect, marginLeft, marginRight, 240f + 60f, minimumStats.scale.stat, 0, 255 );
	}

	private int DrawStat ( string name, Rect rect, float marginLeft, float marginRight, float topOffset, int value, int minValue, int maxValue ) {
		GUI.Label ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset, rect.size.x - marginRight, rect.size.y / 2f ), name );
		return EditorGUI.IntSlider ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset + 15f, rect.size.x - marginRight, rect.size.y / 2f ), value, minValue, maxValue );
	}

	public override float GetHeight () {
		return height + requirements.GetHeight ();
	}

	public Weapon GetWeapon () {
		Weapon weapon = ScriptableObject.CreateInstance<Weapon> ();
		weapon.weaponName = title;
		weapon.minimumStats = minimumStats;
		weapon.requirementsToMeet = requirements.statsToMeet;
		return weapon;
	}
}

public class DrawableRequirements : DrawableInfo {

	public WeaponStats statsToMeet;
	public bool isOpen;

	public DrawableRequirements () {
		title = "Requirements";
		height = 330f;
		statsToMeet = new WeaponStats ();
	}

	public override void Draw ( Rect rect, GUIStyle style ) {
		isOpen = EditorGUI.Foldout ( rect, isOpen, "Requirements" );

		if ( isOpen ) {
			float marginLeft = 5f;
			float marginRight = marginLeft + 0f;

			statsToMeet.attack.stat = DrawStat ( "Attack", rect, marginLeft, marginRight, 30f, statsToMeet.attack.stat, 1, 255 );
			statsToMeet.durability.stat = DrawStat ( "Durability", rect, marginLeft, marginRight, 60f, statsToMeet.durability.stat, 20, 255 );
			statsToMeet.flame.stat = DrawStat ( "Flame", rect, marginLeft, marginRight, 90f, statsToMeet.flame.stat, 0, 255 );
			statsToMeet.chill.stat = DrawStat ( "Chill", rect, marginLeft, marginRight, 120f, statsToMeet.chill.stat, 0, 255 );
			statsToMeet.lightning.stat = DrawStat ( "Lightning", rect, marginLeft, marginRight, 150f, statsToMeet.lightning.stat, 0, 255 );
			statsToMeet.cyclone.stat = DrawStat ( "Cyclone", rect, marginLeft, marginRight, 180f, statsToMeet.cyclone.stat, 0, 255 );
			statsToMeet.exorcism.stat = DrawStat ( "Exorcism", rect, marginLeft, marginRight, 210f, statsToMeet.exorcism.stat, 0, 255 );
			statsToMeet.beast.stat = DrawStat ( "Beast", rect, marginLeft, marginRight, 240f, statsToMeet.beast.stat, 0, 255 );
			statsToMeet.scale.stat = DrawStat ( "Scale", rect, marginLeft, marginRight, 270f, statsToMeet.scale.stat, 0, 255 );
		}
	}

	private int DrawStat ( string name, Rect rect, float marginLeft, float marginRight, float topOffset, int value, int minValue, int maxValue ) {
		GUI.Label ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset, rect.size.x - marginRight, rect.size.y / 2f ), name );
		return EditorGUI.IntSlider ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset + 15f, rect.size.x - marginRight, rect.size.y / 2f ), value, minValue, maxValue );
	}

	public override float GetHeight () {
		return isOpen ? height : 30f;
	}
}

[Serializable]
public class WeaponStats {
	public StatContainer attack;
	public StatContainer durability;
	public StatContainer flame;
	public StatContainer chill;
	public StatContainer lightning;
	public StatContainer cyclone;
	public StatContainer exorcism;
	public StatContainer beast;
	public StatContainer scale;

	public WeaponStats () {
		attack = new StatContainer ( 10 );
		durability = new StatContainer ( 30 );
		flame = new StatContainer ( 5 );
		chill = new StatContainer ( 5 );
		lightning = new StatContainer ( 5 );
		cyclone = new StatContainer ( 5 );
		exorcism = new StatContainer ( 5 );
		beast = new StatContainer ( 5 );
		scale = new StatContainer ( 5 );
	}

	public override string ToString () {
		return "(atk: " + attack.stat + ", dur: " + durability.stat + ", fla: " + flame.stat + ", chi: " + chill.stat + ", lig: " + lightning.stat + ", cyc: " + cyclone.stat + ", exo: " + exorcism.stat + ", bea: " + beast.stat + ", sca: " + scale.stat + ")";
	}
}

[Serializable]
public class StatContainer {
	public int stat;
	protected int buffPool;

	public int statWithBuff {
		get {
			return stat + buffPool;
		}
	}

	/// <summary>
	/// Adds this value to current stat
	/// </summary>
	/// <param name="adjustmentValue"></param>
	public void AdjustStat ( int adjustmentValue ) {
		stat += adjustmentValue;
	}

	/// <summary>
	/// Adds this value to buff pool
	/// </summary>
	/// <param name="buffValue"></param>
	public void AddBuff ( int buffValue ) {
		buffPool += buffValue;
	}

	public StatContainer ( int value ) {
		stat = value;
		buffPool = 0;
	}

	public static implicit operator int ( StatContainer container ) {
		return container.statWithBuff;
	}
}