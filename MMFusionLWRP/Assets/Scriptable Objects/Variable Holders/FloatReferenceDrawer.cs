using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatReference))]
public class FloatReferenceDrawer : PropertyDrawer
{
    private bool propertyUI, showMenu = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {   
        bool useUnique = property.FindPropertyRelative("UseUnique").boolValue;

        
        GUIContent popupIcon = EditorGUIUtility.IconContent("_Popup");
        //propertyUI = EditorGUILayout.BeginFoldoutHeaderGroup(propertyUI, "Property UI");
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var rect = new Rect(position.position, Vector2.one * 15);

        if (
            EditorGUI.DropdownButton(rect,
                popupIcon, 
                FocusType.Passive , 
                new GUIStyle() { fixedWidth = 50f, border = new RectOffset(1,1,1,1)}))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Unique"), 
            useUnique,
            () => SetProperty(property, true));


            menu.AddItem(new GUIContent("Variable"),
            !useUnique,
            () => SetProperty(property,false));

            menu.ShowAsContext();

            
        }

        //position.position = Vector2.right;
        float value = property.FindPropertyRelative("UniqueValue").floatValue;

        if(useUnique) {
            string newValue = EditorGUI.TextField(position, value.ToString());
            float.TryParse(newValue, out value);
            property.FindPropertyRelative("UniqueValue").floatValue = value;
        }
        else {
            EditorGUI.ObjectField(position, property.FindPropertyRelative("Variable"), GUIContent.none);
        }
        EditorGUI.EndProperty();
        //EditorGUILayout.EndFoldoutHeaderGroup(); 
/*         showMenu = EditorGUILayout.Toggle("Use Unique", showMenu);
        if(showMenu) {
            useUnique = false;
        }
        else
            useUnique = true; */
    }

    private void SetProperty(SerializedProperty property, bool value) {
        var propRelative = property.FindPropertyRelative("UseUnique");
        propRelative.boolValue = value;
        property.serializedObject.ApplyModifiedProperties();
    }

/*     private Texture GetTexture() {
        var textures = Resources.FindObjectsOfTypeAll(typeof(Texture))
            .Where(t=> t.name.ToLower().Contains("animationdopesheetkeyframe"))
            .Cast<Texture>().ToList();
            return textures[0];
    } */
}
