using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[CustomEditor(typeof(OnOffStateMachine))]
public class OnOffStateMachineInspector : Editor
{
	public override void OnInspectorGUI()
	{
		var statemachine = target as OnOffStateMachine;
		if(!statemachine) return;

		StateMachinePopup (statemachine);
		
		base.OnInspectorGUI ();
	}

	public static void StateMachinePopup (OnOffStateMachine statemachine)
	{
		var states = statemachine.transform.EnumerateChildren().Select((obj) => obj.GetComponent<OnOff>()).Where((arg) => arg!=null).ToList();
		states.Add(null);
		
		var currentId = states.IndexOf(statemachine.currentState);
		var strings =  states.Select((arg) => arg != null ? new GUIContent(arg.name):new GUIContent("Null")).ToArray();
		var newId = EditorGUILayout.Popup(currentId, strings, GUILayout.ExpandWidth(false));
		if(currentId!=newId)
		{
			if(Application.isPlaying)
				statemachine.SetState(states[newId]);
			else 
				statemachine.currentState = states[newId];
		}
	}
}
