using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(LayerId))]
public class LayerIdDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) 
	{
		EditorGUI.BeginProperty (position, label, prop);
		
		var valueProp = prop.FindPropertyRelative("value");
		valueProp.intValue = EditorGUI.LayerField(position, label, valueProp.intValue);
		
		EditorGUI.EndProperty();
	}
}