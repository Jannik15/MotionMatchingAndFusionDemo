using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorPanel : EditorWindow
{
	private Color color;
	private static bool selectionHasRenderer;

	[MenuItem("Window/ColorObjects %#e", true, 0)]
	static bool ValidateColorObject()
	{
		if (Selection.activeTransform != null)
		{
			Transform[] selectedTransforms = Selection.transforms;
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
	            if (selectedTransforms[i].GetComponent<Renderer>() != null)
		            return true;
            }
		}
		return false;
    }

    [MenuItem("Window/ColorObjects %#e", false, 0)]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<EditorPanel>("Color Selected Objects");
	}
	void OnGUI()
	{
		GUILayout.Label("Header", EditorStyles.boldLabel);
		color = EditorGUILayout.ColorField("Choose a color for the selected objects: ", color);
		selectionHasRenderer = false;
		List<Renderer> renderersInSelection = new List<Renderer>();
		Transform[] selectedTransforms = Selection.transforms;
		for (int i = 0; i < selectedTransforms.Length; i++)
		{
			if (selectedTransforms[i].GetComponent<Renderer>() != null)
			{
				selectionHasRenderer = true;
			}
		}
        if (Selection.activeGameObject != null && selectionHasRenderer)
		{
			if (GUILayout.Button("Apply color to selected objects!"))
			{
				foreach (var obj in Selection.gameObjects)
				{
					Renderer rend = obj.GetComponent<Renderer>();
					if (rend != null)
						rend.material.color = color;
				}
			}
		}
		else
		{
			EditorWindow.GetWindow<EditorPanel>("Color Selected Objects").Close();
		}
    }
}
