using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeEditor : EditorWindow
{
	private List<Node> nodes = new List<Node>();
	private List<Connection> connections = new List<Connection>();

	private GUIStyle nodeStyle;
	private GUIStyle selectedNodeStyle;
	private GUIStyle inPointStyle;
	private GUIStyle outPointStyle;

	private ConnectionPoint selectedInPoint;
	private ConnectionPoint selectedOutPoint;

	private Vector2 offset;
	private Vector2 drag;

	[MenuItem("Window/Node Based Editors/Guns/Editor")]
	private static void OpenWindow()
	{
		NodeEditor window = GetWindow<NodeEditor>();
		window.titleContent = new GUIContent("Node Based Editor for Guns");
    }

	private void OnEnable()
	{
		nodeStyle = new GUIStyle();
		nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
		nodeStyle.border = new RectOffset(12, 12, 12, 12);
		nodeStyle.alignment = TextAnchor.MiddleCenter;

		selectedNodeStyle = new GUIStyle();
		selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
		selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
		selectedNodeStyle.alignment = TextAnchor.MiddleCenter;

		inPointStyle = new GUIStyle();
		inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
		inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
		inPointStyle.border = new RectOffset(4, 4, 12, 12);
		inPointStyle.alignment = TextAnchor.MiddleCenter;

		outPointStyle = new GUIStyle();
		outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
		outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
		outPointStyle.border = new RectOffset(4, 4, 12, 12);
		outPointStyle.alignment = TextAnchor.MiddleCenter;
	}

	private void OnGUI()
	{
		DrawGrid(20, 0.2f, Color.white);
		DrawGrid(100, 0.4f, Color.gray);

		DrawNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.Button(new Rect(0, 0, 100, 30), "Save"))
		{
			SaveNodes();
		}

		if (GUI.changed)
		{
			Repaint();
		}
	}
	private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
	{
		int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
		int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

		Handles.BeginGUI();
		Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

		offset += drag * 0.5f;
		Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

		for (int i = 0; i < widthDivs; i++)
		{
			Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
		}

		for (int j = 0; j < heightDivs; j++)
		{
			Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}
	private void DrawNodes()
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Draw();
			}
		}
	}

	private void DrawConnections()
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Count; i++)
			{
				connections[i].Draw();
			}
		}
	}

	private void ProcessEvents(Event e)
	{
		drag = Vector2.zero;

		switch (e.type)
		{
			case EventType.MouseDown:
				if (e.button == 0)
				{
					ClearConnectionSelection();
				}

				if (e.button == 1)
				{
					ProcessContextMenu(e.mousePosition);
				}
				break;

			case EventType.MouseDrag:
				if (e.button == 0)
				{
					OnDrag(e.delta);
				}
				break;

			case EventType.KeyDown:
				if (e.keyCode == KeyCode.Delete)
				{
					for (int i = 0; i < nodes.Count; i++)
					{
						if (nodes[i].isSelected)
						{
							OnClickRemoveNode(nodes[i]);
						}
					}
				}
				break;
		}
	}

	private void ProcessNodeEvents(Event e)
	{
		if (nodes != null)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				bool guiChanged = nodes[i].ProcessEvents(e);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}
		}
	}

	private void DrawConnectionLine(Event e)
	{
		if (selectedInPoint != null && selectedOutPoint == null)
		{
			Handles.DrawBezier(
				selectedInPoint.rect.center,
				e.mousePosition,
				selectedInPoint.rect.center + Vector2.left * 50f,
				e.mousePosition - Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}

		if (selectedOutPoint != null && selectedInPoint == null)
		{
			Handles.DrawBezier(
				selectedOutPoint.rect.center,
				e.mousePosition,
				selectedOutPoint.rect.center - Vector2.left * 50f,
				e.mousePosition + Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Gun"), false, () => OnClickAddNode(mousePosition, new DrawableGun()));
        //genericMenu.AddItem ( new GUIContent ( "Add node" ), false, () => OnClickAddNode ( mousePosition ) );
        genericMenu.ShowAsContext();
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private void OnClickAddNode(Vector2 mousePosition, DrawableInfo info)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }

        nodes.Add(new Node(mousePosition, 350, 50, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, info));
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        nodes.Remove(node);

        Repaint();
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private void SaveNodes()
    {
        string path = "Assets/Resources/Guns";

        if (!AssetDatabase.IsValidFolder(path))
        {

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            AssetDatabase.CreateFolder("Assets/Resources", "Guns");
        }

        List<Node> nodesPassed = new List<Node>();

        foreach (Node n in nodes)
        {
            if (n.myInfo as DrawableGun != null)
            {
                Gun gun = (n.myInfo as DrawableGun).GetGun();

                gun = CreateOrReplaceAsset(gun, path + "/" + gun.gunName + ".asset");

                foreach (Connection c in connections)
                {
                    if (c.inPoint.node == n)
                    {
                        if (nodesPassed.Contains(c.outPoint.node))
                        {
                            Gun gun2 = LoadAsset<Gun>(path + "/" + c.outPoint.node.myInfo.title + ".asset");
                            gun.upgradesFrom.Add(gun2);
                            gun2.upgradesTo.Add(gun);

                            EditorUtility.SetDirty(gun);
                            EditorUtility.SetDirty(gun2);
                        }
                    }
                    else if (c.outPoint.node == n)
                    {
                        if (nodesPassed.Contains(c.inPoint.node))
                        {
                            Gun gun2 = LoadAsset<Gun>(path + "/" + c.inPoint.node.myInfo.title + ".asset");
                            gun.upgradesTo.Add(gun2);
                            gun2.upgradesFrom.Add(gun);

                            EditorUtility.SetDirty(gun);
                            EditorUtility.SetDirty(gun2);
                        }
                    }
                }

                nodesPassed.Add(n);
            }
        }
    }

    T LoadAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }
}
