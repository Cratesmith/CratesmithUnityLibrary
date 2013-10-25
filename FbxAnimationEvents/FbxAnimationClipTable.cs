using UnityEngine;
using System.Collections;

[System.Serializable]
public class FbxAnimationClip
{
	public AnimationClip 			clip;
	public FbxAnimationClipEvent[] 	events;
}

[System.Serializable]
public class FbxAnimationClipEvent
{
	public string functionName;
	public float percent;
	
	public AnimationEvent CreateEvent(AnimationClip clip)
	{
		var evt = new UnityEngine.AnimationEvent() { functionName = this.functionName, time = this.percent*clip.length}; 
		return evt;
	}
}

public class FbxAnimationClipTable : CustomAsset 
{
	public FbxAnimationClip[] importAnims;

	public void Process ()
	{
		foreach(var anim in importAnims)
		{
			if(anim==null || anim.clip==null)
				continue;
			
			foreach(var evt in anim.events)
			{
				anim.clip.AddEvent(evt.CreateEvent(anim.clip));
			}
		}
	}
}

