using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public sealed class OnOffIcons 
{
// Const parameters
    const int 	width = 48;
	const int 	height = 16;
    const int 	borderSize = 2;
	const double redrawFreq = 2.0f;
	
	static OnOffIcons mInstance;
	
	double 	lastRedawTime = -1.0f;
	bool	wasPlaying = false;
	
	static OnOffIcons()
	{
		if (mInstance == null)
		{
			mInstance = new OnOffIcons();
		}
	} 
	
    private OnOffIcons()
    {
        // Add delegates
        //EditorApplication.projectWindowItemOnGUI += ProjectWindowListElementOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowListElementOnGUI;
		EditorApplication.update += RedrawCheck;

        // Request repaint of project and hierarchy windows 
        //EditorApplication.RepaintProjectWindow();
        EditorApplication.RepaintHierarchyWindow();
    }
    
	~OnOffIcons()
    {
        //EditorApplication.projectWindowItemOnGUI -= ProjectWindowListElementOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowListElementOnGUI;
    }
	
	/*
    static void ProjectWindowListElementOnGUI(string guid, Rect selectionRect)
    {
        DrawElement(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object)), selectionRect);
    }
	*/
	
    static void HierarchyWindowListElementOnGUI(int instanceID, Rect selectionRect)
    {
        DrawElement(EditorUtility.InstanceIDToObject(instanceID), selectionRect);
		mInstance.wasPlaying = EditorApplication.isPlaying;
    }

	void RedrawCheck ()
	{
		if(EditorApplication.timeSinceStartup-lastRedawTime > redrawFreq)
		{
			EditorApplication.RepaintHierarchyWindow();
			lastRedawTime = EditorApplication.timeSinceStartup;
		}
	}
	
    static void DrawElement(Object obj, Rect rect)
    {
		DrawLeftPanel(obj, rect);
		DrawRightPanel(obj, rect);
    }
	
	static void DrawLeftPanel(Object obj, Rect rect)
	{
		if(!obj)
    		return;
			
		var textOffset = GUI.skin.GetStyle("label").CalcSize(new GUIContent(obj.name));
		rect.x += textOffset.x;
		rect.width -= textOffset.x;
		
		rect = DrawLeftPanel_OnOff(obj, rect);
		rect = DrawLeftPanel_State(obj, rect);
		rect = DrawLeftPanel_Inheritance(obj, rect);
	}

	static Rect DrawLeftPanel_OnOff (Object obj, Rect rect)
	{
		var go = obj as GameObject;
		if(!go) return rect;
		
		var onOff = go.GetComponent<OnOff>();
		if(!onOff) return rect;
		
		Rect buttonRect = new Rect(rect.x, rect.y, 30, rect.height);
		if(mInstance.wasPlaying == EditorApplication.isPlaying)
			onOff.On = GUI.Toggle(buttonRect, onOff ? onOff.On:false, onOff.On ? "on":"off", "button");		
		rect.x = buttonRect.xMax;

		return rect;
	}

	static Rect DrawLeftPanel_Inheritance (Object obj, Rect rect)
	{
		var go = obj as GameObject;
		if(!go) return rect;
		
		var inherited = go.GetComponent<OnOffInherited>();
		if(!inherited) return rect;
		
		var content = new GUIContent("(to: "+inherited.inheritanceType.ToString()+" )");
		var size = GUI.skin.label.CalcSize(content);
		Rect buttonRect = new Rect(rect.x, rect.y, size.x, size.y);
		
		GUI.Label(buttonRect,content);
		rect.x = buttonRect.xMax;
		
		return rect;
	}

	static Rect DrawLeftPanel_State (Object obj, Rect rect)
	{
		var go = obj as GameObject;
		if(!go) return rect;
		
		var statemachine = go.GetComponent<OnOffStateMachine>();
		if(!statemachine) return rect;

		var states = statemachine.transform.EnumerateChildren().Select((arg) => arg.GetComponent<OnOff>()).Where((arg) => arg!=null).ToList();
		states.Add(null);
		
		var currentId = states.IndexOf(statemachine.currentState);
		var strings =  states.Select((arg) => arg != null ? new GUIContent(arg.name):new GUIContent("Null")).ToArray();
		
		Rect buttonRect = new Rect(rect.x, rect.y, 60, rect.height);
		var newId = EditorGUI.Popup(buttonRect, currentId, strings);
		rect.x = buttonRect.xMax;
		
		if(currentId!=newId)
		{
			if(Application.isPlaying)
				statemachine.SetState(states[newId]);
			else 
				statemachine.currentState = states[newId];
		} 
		
		return rect;
	}

	static void DrawRightPanel (Object obj, Rect rect)
	{
		if(!obj)
    		return;
	}

	
    static void RefreshGUI()
    {
        EditorApplication.RepaintProjectWindow();
        EditorApplication.RepaintHierarchyWindow();
	}

    static Rect GetRightAligned(Rect rect, float width, float height)
    {
		if (rect.height > 20)
        {
            // Unity 4.x large project icons
            rect.y = rect.y - 3;
            rect.x = rect.width + rect.x - 12;
            rect.width = width;
            rect.height = height;
        }
		else
		{
	        rect.x = rect.xMax - width-4;
	        rect.width = width+borderSize*2;
	        rect.y = rect.y;
	        rect.height = height+borderSize*2;
		}
        return rect;
    }

	static Rect GetLeftAligned (Rect rect, float width, float height)
	{
 		rect.x = rect.x;
        rect.width = width + borderSize*2;
        rect.y = rect.y;
        rect.height = height+borderSize*2;
		return rect;
	}
}