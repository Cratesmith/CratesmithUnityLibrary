using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class OnOffInherited : OnOff
{
	public enum InheritanceType
	{
		DirectChildren,
		AllChildrenRecursively
	}
	public InheritanceType inheritanceType;
	
	IEnumerable<OnOff> GetOnOffSet()
	{
		switch(inheritanceType)
		{
		case InheritanceType.AllChildrenRecursively: return GetComponentsInChildren<OnOff>(true);
		case InheritanceType.DirectChildren: return transform.EnumerateChildren().Select((arg) => arg.GetComponent<OnOff>()).Where((arg) => arg != null);
		}
		throw new System.Exception("Unknown inheritance type");
	}
	
	protected override void ApplySwitchedOff ()
	{
		base.ApplySwitchedOff();
		foreach(var i in GetOnOffSet()) 
		{
#if UNITY_EDITOR
			if(debug)
				Debug.Log(name+": Sending OFF to "+i.name);
#endif
			i.On = false;
		}
	}
	
	protected override void ApplySwitchedOn ()
	{
		base.ApplySwitchedOn();
		foreach(var i in GetOnOffSet()) 
		{
#if UNITY_EDITOR
				if(debug)
					Debug.Log(name+": Sending ON to "+i.name);
#endif
			i.On = true;
		}
	}
}
