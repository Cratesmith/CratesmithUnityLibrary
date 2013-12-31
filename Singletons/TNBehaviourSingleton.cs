using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TNBehaviourSingleton<T> : TNBehaviour where T:Component
{
	static T instance = null;
	static bool shuttingDown = false;
	
	public static bool Exists { get { return Instance !=null; } }
	
	void OnApplicationQuit()
	{
		shuttingDown = true;
	}
	
	protected virtual void Awake()
	{
		T comp = this.GetComponent<T>();
		if(instance == null)
			instance = comp;
	}
	
	protected virtual void OnDestroy()
	{
		if(instance == this)
			instance = null;
	}
	
	public static T Instance 
	{
		get 
		{
			if(instance != null)
				return instance;
			else 
				instance =  (T)GameObject.FindObjectOfType(typeof(T));
			
			if(instance == null && Application.isPlaying && !shuttingDown)
			{
				GameObject obj = new GameObject(typeof(T).Name);
				instance = obj.AddComponent<T>();
			}
			
			return instance;
		}
	}	
}
