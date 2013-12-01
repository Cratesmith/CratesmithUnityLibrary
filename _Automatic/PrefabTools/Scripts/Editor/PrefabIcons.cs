using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[InitializeOnLoad]
public sealed class PrefabIcons 
{
// Const parameters
    const int 	width = 48;
	const int 	height = 16;
    const int 	borderSize = 2;
	const double redrawFreq = 2.0f;
	
	static PrefabIcons mInstance;
	
	double lastRedawTime = -1.0f;
	
	static PrefabIcons()
	{
		if (mInstance == null)
		{
			mInstance = new PrefabIcons();
		}
	} 
	
    private PrefabIcons()
    {
        // Add delegates
        //EditorApplication.projectWindowItemOnGUI += ProjectWindowListElementOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowListElementOnGUI;
		EditorApplication.update += RedrawCheck;

        // Request repaint of project and hierarchy windows 
        //EditorApplication.RepaintProjectWindow();
        EditorApplication.RepaintHierarchyWindow();
    }
    
	~PrefabIcons()
    {
        EditorApplication.projectWindowItemOnGUI -= ProjectWindowListElementOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowListElementOnGUI;
    }

    static void ProjectWindowListElementOnGUI(string guid, Rect selectionRect)
    {
        DrawElement(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object)), selectionRect);
    }

    static void HierarchyWindowListElementOnGUI(int instanceID, Rect selectionRect)
    {
        DrawElement(EditorUtility.InstanceIDToObject(instanceID), selectionRect);
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
		
		rect = DrawLeftPanel_SelectPrefab(obj, rect);
		rect = DrawLeftPanel_MergeIntoParentPrefab(obj, rect);
	}
	
	static Rect DrawLeftPanel_SelectPrefab(Object obj, Rect rect)
	{
		return rect;
		/*
		var prefab = obj != null ? PrefabUtility.GetPrefabParent(obj):null;
		if(!prefab)
		{
			return rect;
		}
		
		Rect buttonRect = GetLeftAligned(rect, 12, 12);
		var icon = GUI.skin.GetStyle("TL Playhead").normal.background; //var icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(prefab));
		var description = string.Concat("Select Prefab");
		
		var result = GUI.Button(buttonRect, new GUIContent(" ", description));
		if(icon)
		{
			GUI.DrawTexture(buttonRect, icon);
		}
		
		if(result)
		{
			Selection.objects = new Object[] { prefab };
		}
				
		return new Rect(rect.x+12+4, rect.y, rect.width-14, rect.height);
		*/		
	}
	
	static Rect DrawLeftPanel_MergeIntoParentPrefab(Object obj, Rect rect)
	{
		return rect;
/*		
		Transform parent = (obj as GameObject).transform.parent;
		var parentPrefab = parent != null ? PrefabUtility.GetPrefabParent(parent.gameObject):null;
		var prefab = PrefabUtility.GetPrefabParent(PrefabUtility.FindPrefabRoot(obj as GameObject));
		if(!parentPrefab || parentPrefab == prefab)
		{
			return rect;
		}
		
		Rect buttonRect = GetLeftAligned(rect, 12, 12);
		var icon = GUI.skin.GetStyle("AC LeftArrow").normal.background;
		var description = string.Concat("Merge into prefab: ", AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(obj)));
		
		var result = GUI.Button(buttonRect, new GUIContent(" ", description));
		if(icon)
		{
			GUI.DrawTexture(buttonRect, icon);
		}
		
		if(result)
		{
		}
				
		return new Rect(rect.x+12+4, rect.y, rect.width-14, rect.height);	
*/
	}
	
	static void DrawRightPanel (Object obj, Rect rect)
	{
		if(!obj)
    		return;
    	
    	var prefabType = PrefabUtility.GetPrefabType(obj);
    	switch(prefabType)
    	{
    	case PrefabType.MissingPrefabInstance: return;
    	case PrefabType.None: return;
    	case PrefabType.ModelPrefab: return;
    	case PrefabType.Prefab: return;
    	}
		
		rect = DrawElement_Modified(obj, rect);
		
    	foreach(var comp in (obj as GameObject).GetComponents<Component>())
    	{
    		rect = DrawElement_Modified(comp, rect);
    	}
    	
    	foreach(var comp in PrefabUtilityEx.GetRemovedComponentsFromPrefab(obj, true))
    	{
    		rect = DrawElement_Removed(obj as GameObject, comp, rect);
    	}
    	
    	if(PrefabUtilityEx.IsPrefabRoot(obj) && (prefabType == PrefabType.DisconnectedPrefabInstance || prefabType == PrefabType.DisconnectedModelPrefabInstance))
    	{
    		rect = DrawElement_Reconnect(obj,rect);
    	}
	}
	
	static Rect DrawElement_Reconnect(Object obj, Rect rect)
	{
		Rect buttonRect = GetRightAligned(rect, 12, 12);
		var icon = GUI.skin.GetStyle("CN EntryWarn").normal.background;
		var description = string.Concat("Disconnected Prefab: ", AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(obj)), "\n\n", PrefabUtilityEx.GetModificationDescription(obj, false));
		
		var result = GUI.Button(buttonRect, new GUIContent(" ", description));
		if(icon)
		{
			GUI.DrawTexture(buttonRect, icon);
		}
		
		if(result)
		{
			var selection = Selection.objects;
			Selection.objects = new[] { obj };
//			EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Prefab/Disconnected Prefab", null);
			Selection.objects = selection;
		}
				
		return new Rect(rect.x, rect.y, buttonRect.x-rect.x-2, rect.height);	
	}
		
	static Rect DrawElement_Removed(GameObject gameObject, Object removedObjectOnPrefab, Rect rect)
	{
		Rect buttonRect = GetRightAligned(rect, 12, 12);
		var icon = EditorGUIUtility.ObjectContent(removedObjectOnPrefab, removedObjectOnPrefab.GetType()).image;
		var description = string.Concat("Removed: ", removedObjectOnPrefab.GetType().Name);

		var result = GUI.Button(buttonRect, new GUIContent(" ", description), "flow node 6");
		if(icon)
		{
			GUI.DrawTexture(buttonRect, icon);
		}
		
		if(result)
		{
			var prefab = PrefabUtility.GetPrefabParent(gameObject);
			//var selection = Selection.objects;
			Selection.objects = new[] { gameObject };
//			PrefabMenu.missingComponentToRemove = removedObjectOnPrefab;
//			EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Prefab/Removed Component", null);
			//Selection.objects = selection;
		}
				
		return new Rect(rect.x, rect.y, buttonRect.x-rect.x-2, rect.height);
	}
	
	static Rect DrawElement_Modified(Object obj, Rect rect)
	{
		if(!PrefabUtilityEx.IsPrefabInstanceModified(obj, true))
			return rect;
		
		Rect buttonRect = GetRightAligned(rect, 12, 12);
		
		var icon = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
		if(!icon)
		{
			icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(obj)));
		}
		
		var description = PrefabUtilityEx.GetModificationDescription(obj, true);
		
		bool result = false;
		
		bool isAdded = PrefabUtility.IsComponentAddedToPrefabInstance(obj);
		
		result = GUI.Button(buttonRect, new GUIContent(" ", description), isAdded?"flow node 0 on":"flow node 0");
		if(icon)
		{
			GUI.DrawTexture(buttonRect, icon);
		}
		
		if(result)
		{
			if(isAdded)
			{
				Selection.objects = new[] { obj };
//				EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Prefab/Added Component", null);				
			}
			else 
			{
				Selection.objects = new[] { obj };
//				EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Prefab/Modified Component", null);
			}
		}
		
		return new Rect(rect.x, rect.y, buttonRect.x-rect.x-2, rect.height);
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