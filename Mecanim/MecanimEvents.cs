using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

// A class for fixing API issues with the mecanim system
// provides state changed callbacks and the ability to play One Shot animations
//
// Any state who's name ends with *name, where name is the name of a boolean parameter will clear that bool when it begins
// This behaviour can be used to create one shot behaviour as well as "automatic exits" for complex states.
//
public class MecanimUpgrade : MonoBehaviour 
{
	// called when a new state has become the current state,
	// note that this occurs after transitions have ended. 
	// True zero frame states might be missed as comparisons occur on update
	public event System.Action<MecanimUpgrade> OnStateChanged = delegate {};
	
	public ControllerState PrevState 		{ get; private set;}
	public ControllerState CurrentState		{ get; private set;}
	
	Dictionary<string, System.Action> oneShotCallbacks = new Dictionary<string, System.Action>();
	
	public void OneShot(string boolParamName, System.Action callback)
	{
		var animator = GetComponent<Animator>();	
		animator.SetBool(boolParamName, true);
		
		System.Action callbacks = null;
		if(oneShotCallbacks.ContainsKey(boolParamName))
			oneShotCallbacks[boolParamName] += callback;
		else 
			oneShotCallbacks[boolParamName] = callback;
	}
	
	void Update()
	{
		var animator = GetComponent<Animator>();		
		var newState = animator.GetCurrentState(0);
		bool changed = newState!=PrevState;
		PrevState = CurrentState;
		CurrentState = newState;
		if(changed) 
		{
			OnStateChanged(this);
		}

		HandleOneShot(); 
	}
	
	void HandleOneShot ()
	{
		var animator = GetComponent<Animator>();		
		var nextState = animator.GetStates().FirstOrDefault((arg) => arg.uniqueNameHash == animator.GetNextAnimatorStateInfo(0).nameHash);
		
		if(nextState==null)
			return;
		
		var param = animator.GetParameters().FirstOrDefault((arg) => nextState.name.EndsWith("*"+arg.name,System.StringComparison.CurrentCultureIgnoreCase) && arg.type == ControllerParameterType.Bool);
		if(param == null)
			return;
		
		animator.SetBool(param.name, false);
		
		// fire any callbacks associated with this one shot
		System.Action callbacks = null;
		if(oneShotCallbacks.TryGetValue(param.name, out callbacks))
		{
			callbacks();
			oneShotCallbacks.Remove(param.name);
		}
	}
}

public static class MecanimUpgradeAnimatorExtension
{
	public static void OneShot(this Animator animator, string boolParamName, System.Action callback)
	{
		var mu = animator.gameObject.GetComponent<MecanimUpgrade>();
		if(!mu) mu = animator.gameObject.AddComponent<MecanimUpgrade>();
		mu.OneShot(boolParamName, callback);
	}
}

