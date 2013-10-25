using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OnOffTestingWindow : EditorWindow 
{
	[SerializeField] Vector2 scroll = Vector2.zero;
	[SerializeField] List<Transform> foldouts = new List<Transform>();
	
	[MenuItem("Window/On Off Debugger")]
	static void CreateWindow()
	{
		GetWindow<OnOffTestingWindow>().Show();		
	}
	
	void OnGUI()
	{
		var roots = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)).Select((arg) => ((GameObject)arg).transform).Where((arg) => arg.parent == null);

		GUILayout.BeginHorizontal("box");
		if(GUILayout.Button("Expand all"))
		{
			foreach(var i in roots)
				ExpandAll(i);
		}
		if(GUILayout.Button("Collapse all"))
		{
		 	foldouts.Clear();
		}
		GUILayout.EndHorizontal();
		
		scroll = GUILayout.BeginScrollView(scroll);
		foreach(var i in roots)
		{
			ProcessTransform (i);
		}
		GUILayout.EndScrollView();
	}

	void ExpandAll (Transform transform)
	{
		var transformsWithStates = transform.EnumerateChildren().Where((arg) => arg.GetComponentsInChildren<OnOff>(true).Length > 0);
		foreach(var i in transformsWithStates)
		{
			foldouts.Add(i);
			ExpandAll(i);
		}
	}

	void ProcessTransform (Transform transform)
	{
		GUILayout.BeginVertical();
		var transformsWithStates = transform.EnumerateChildren().Where((arg) => arg.GetComponentsInChildren<OnOff>(true).Length > 0);
		foreach(var i in transformsWithStates)
		{			
			GUILayout.BeginHorizontal();
			bool foldout = foldouts.Contains(i);
			bool newFoldout = EditorGUILayout.Toggle(foldout, "foldout", GUILayout.Width(10));
			if(newFoldout!=foldout)
			{
				foldout = newFoldout; 
				if(!foldout)
					foldouts.Remove(i);
				else 
					foldouts.Add(i);
			}

			//GUILayout.BeginHorizontal("box",GUILayout.ExpandWidth(false));
			i.gameObject.SetActive(GUILayout.Toggle (i.gameObject.activeSelf, "", GUILayout.ExpandWidth (false)));
			GUI.enabled = i.gameObject.activeInHierarchy;
			GUILayout.Label(i.name, GUILayout.ExpandWidth(false), GUILayout.Height(20));
			//GUILayout.EndHorizontal();

			var onOff = i.GetComponent<OnOff>();
			if(onOff)
			{
				try
				{
					onOff.On = GUILayout.Toggle(onOff ? onOff.On:false, onOff.On ? "on":"off", "button", GUILayout.ExpandWidth(false));
				}
				catch(System.Exception e)
				{
					Debug.LogError(e);
				}
			}

			var statemachine = i.GetComponent<OnOffStateMachine>();
			if(statemachine)
			{
				OnOffStateMachineInspector.StateMachinePopup(statemachine);
			}
			
			var inherited = i.GetComponent<OnOffInherited>();
			if(inherited)
			{
				GUILayout.Label("(inherited: "+inherited.inheritanceType.ToString()+" )", GUILayout.ExpandWidth(false));
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
			if(foldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				ProcessTransform(i);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
