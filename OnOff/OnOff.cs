using UnityEngine;
using System.Collections;

public class OnOff : MonoBehaviour 
{
	public event System.Action<OnOff> OnSwitchedOn = delegate {};
	public event System.Action<OnOff> OnSwitchedOff = delegate {};
	
	public bool deactivateIfSwitchedOff = false;
	public bool activateIfSwitchedOn = true;
	
	[SerializeField] private bool on = false;
	protected bool initialized = false;
	
#if UNITY_EDITOR
	[SerializeField] protected bool debug = false;
	
	[SerializeField] bool switchOnOffNow = false;
	void OnDrawGizmosSelected()
	{
		if(switchOnOffNow)
			On = !On;
		
		switchOnOffNow = false;
	}
#endif
	
	public bool On 
	{
		get 
		{
			return on;	
		}
		
		set
		{
			Initialize();
			
			if(value == on)
				return;
			
			if(value)
			{
#if UNITY_EDITOR
				if(debug)
					Debug.Log(name+ " ON");
#endif
				ApplySwitchedOn ();
			}
			else 
			{
#if UNITY_EDITOR
				if(debug)
					Debug.Log(name+ " OFF");
#endif
				ApplySwitchedOff ();
			}
		}
	}	

	protected virtual void ApplySwitchedOn ()
	{
		if(activateIfSwitchedOn) 
		{
			gameObject.SetActive(true);
		}
		on = true;
		OnSwitchedOn(this);
	}

	protected virtual void ApplySwitchedOff ()
	{
		on = false;
		OnSwitchedOff(this);
		if(deactivateIfSwitchedOff)
		{
			gameObject.SetActive(false);
		}
	}
	
	protected virtual void Start()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		if(initialized)
			return;
		
		initialized = true;
//		Debug.Log(name+ " INITIALIZING");
		if(!On) ApplySwitchedOff();
		else ApplySwitchedOn();
	}
}
