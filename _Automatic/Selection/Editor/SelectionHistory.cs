using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SelectionHistory : ScriptableObject
{
	List<HashSet<Object>> selectionHistory = new List<HashSet<Object>>();
	static SelectionHistory instance = ScriptableObject.CreateInstance<SelectionHistory>();
	int	currentId = -1;
	
	SelectionHistory()
	{
		// EditorApplication.update += UpdateSelectionIfChanged;
	}
	
	bool CurrentSelectionDifferentToHistory ()
	{
		if(Selection.objects.Length == 0)
			return false;
		
		if(selectionHistory.Count == 0)
			return true;
		
		if(selectionHistory[currentId].Count != Selection.objects.Length)
			return true;
		
		if(selectionHistory[currentId].SetEquals(Selection.objects))
			return true;
		
		return false;
	}
	
	void UpdateSelectionIfChanged()
	{
		if(!CurrentSelectionDifferentToHistory())
			return;
		
		selectionHistory.Add(new HashSet<Object>(Selection.objects));
		currentId = selectionHistory.Count-1;
	}
	
	bool CanPrev()
	{
		return selectionHistory.Count != 0 && currentId > 0;
	}
	
	bool CanNext()
	{
		return selectionHistory.Count != 0 && currentId < selectionHistory.Count-1;
	}
	
	void Next()
	{
		if(!CanNext())
			return;
		
		++currentId;
		Selection.objects = (Object[])selectionHistory[currentId].ToArray(); 
	}
	
	void Prev()
	{
		if(!CanPrev())
			return;
		
		--currentId;
		Selection.objects = (Object[])selectionHistory[currentId].ToArray(); 
	}
	
	
	[MenuItem("Edit/Next In Selection History &w", true)]
	public static bool CheckNextMenuItem()
	{
		return (instance!=null && instance.CanNext());
	}
	
	[MenuItem("Edit/Next In Selection History &w")]
	public static void NextMenuItem()
	{
		if(instance!=null)
			instance.Next();
	}

	[MenuItem("Edit/Prev In Selection History &q", true)]
	public static bool CheckPrevMenuItem()
	{
		return (instance!=null && instance.CanPrev());
	}
	
	
	[MenuItem("Edit/Prev In Selection History &q")]
	public static void PrevMenuItem()
	{
		if(instance!=null)
			instance.Prev();
	}
}
