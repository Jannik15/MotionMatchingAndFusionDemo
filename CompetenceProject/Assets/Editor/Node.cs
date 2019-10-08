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
	protected float height = 220f;
	public GUIStyle style;

	public abstract void Draw ( Rect rect, GUIStyle style );
	public abstract float GetHeight ();
}

public class DrawableGun : DrawableInfo {

	private GunStats gunStats;

	public DrawableGun() {
		title = "New Gun";
		gunStats = new GunStats ();
	}

	public override void Draw ( Rect rect, GUIStyle style ) {
		Rect baseRect = new Rect ( 10f, 30f, rect.size.x - 30f, 30f );

		baseRect = new Rect ( baseRect.position.x, baseRect.position.y, baseRect.size.x, baseRect.size.y );

		EditorGUI.LabelField ( baseRect, "Gun" );

		float marginLeft = 5f;
		float marginRight = marginLeft + 0f;

		GUI.Label ( new Rect ( baseRect.position.x + marginLeft, baseRect.position.y + 30f, baseRect.size.x - marginRight, baseRect.size.y / 2f ), "Title" );
		title = EditorGUI.TextField ( new Rect ( baseRect.position.x + marginLeft, baseRect.position.y + 45f, baseRect.size.x - marginRight, baseRect.size.y / 2f ), title );

		gunStats.damage.stat = DrawStat ( "Damage", baseRect, marginLeft, marginRight, 60f, gunStats.damage.stat, 0, 255 );
		gunStats.clipSize.stat = DrawStat ( "Clipsize", baseRect, marginLeft, marginRight, 30f + 60f, gunStats.clipSize.stat, 1, 255 );
		gunStats.firingRate.stat = DrawStat ( "Firing Rate", baseRect, marginLeft, marginRight, 60f +  60f, gunStats.firingRate.stat, 0, 255 );
		gunStats.spread.stat = DrawStat ( "Spread", baseRect, marginLeft, marginRight, 90f +  60f, gunStats.spread.stat, 0, 255 );
		gunStats.bulletsPerShot.stat = DrawStat ( "Bullets per shot", baseRect, marginLeft, marginRight, 120f + 60f, gunStats.bulletsPerShot.stat, 1, 255 );
	}

	private int DrawStat ( string name, Rect rect, float marginLeft, float marginRight, float topOffset, int value, int minValue, int maxValue ) {
		GUI.Label ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset, rect.size.x - marginRight, rect.size.y / 2f ), name );
		return EditorGUI.IntSlider ( new Rect ( rect.position.x + marginLeft, rect.position.y + topOffset + 15f, rect.size.x - marginRight, rect.size.y / 2f ), value, minValue, maxValue );
	}

	public override float GetHeight () {
		return height;
	}

	public Gun GetGun () {
		Gun gun = ScriptableObject.CreateInstance<Gun> ();
		gun.gunName = title;
		gun.stats = gunStats;
		return gun;
	}
}

[Serializable]
public class GunStats {
	public StatContainer damage;
	public StatContainer clipSize;
	public StatContainer firingRate;
	public StatContainer spread;
	public StatContainer bulletsPerShot;

	public GunStats () {
		damage = new StatContainer ( 10 );
		clipSize = new StatContainer ( 30 );
		firingRate = new StatContainer ( 1 );
		spread = new StatContainer ( 1 );
		bulletsPerShot = new StatContainer ( 1 );
	}

	public override string ToString () {
		return "(DMG: " + damage.stat + ", Clip size: " + clipSize.stat + ", Rate of fire: " + firingRate.stat + ", Spread: " + spread.stat +
		       ", Bullets per shot: " + bulletsPerShot.stat + ")";
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