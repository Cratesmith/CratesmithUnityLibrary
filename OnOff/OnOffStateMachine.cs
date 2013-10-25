using UnityEngine;
using System.Collections;
using System.Linq;

public class OnOffStateMachine : OnOff
{
	public bool		startWithAllChildrenOff = true;
	public OnOff 	currentState;
	OnOff 			nextState;

	protected override void Initialize()
	{
		if(!initialized && startWithAllChildrenOff)
		{
			var childStates = transform.EnumerateChildren().Select((obj) => obj.gameObject.GetComponent<OnOff>()).Where((obj) => obj != null);
			foreach(var i in childStates)
			{
				i.On = false;
			}
		}
		base.Initialize();
	}
		
	protected override void Start()
	{
		if(!initialized) Initialize ();
	}
	
	protected override void ApplySwitchedOn ()
	{
		base.ApplySwitchedOn();
		RefreshStates(false);
	}
	
	protected override void ApplySwitchedOff ()
	{
		base.ApplySwitchedOff();
		if(currentState)
			currentState.On = false;
	}
	
	public void SetState(OnOff state)
	{	
		Initialize();
		if(state != null && !Enumerable.Range(0, transform.childCount).Any((i) => transform.GetChild(i).gameObject == state.gameObject))
		{
			Debug.LogWarning(string.Format("{0} is not a state of {1}!", state.name, name));
			return;
		}
		
		nextState = state;

		if(!On)
			return;
		
		RefreshStates(true);
	}

	void RefreshStates(bool allowNullNextState)
	{		
		if(nextState || allowNullNextState)
		{
			if(currentState && currentState != nextState)
			{
				currentState.On = false;
			}

			currentState = nextState;
			nextState = null;
		}

		if(currentState)
		{
			currentState.On = true;
		}		
	}
}
