using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BehaviourSingleton<T> : MonoBehaviour where T:Component
{
	static T instance = null;
	static bool shuttingDown = false;
	
	public static bool Exists { get { return Instance !=null; } }
	
	void OnApplicationQuit()
	{
		shuttingDown = true;
	}
	
	public virtual void OnEnable()
	{
		T comp = this.GetComponent<T>();
		if(instance == null)
			instance = comp;
	}
	
	public virtual void OnDisable()
	{
		if(instance == this)
			instance = null;
	}
	
	public virtual void OnDestroy()
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
		
			/*
			if(instance == null && Application.isPlaying && !shuttingDown)
			{
				GameObject obj = new GameObject(typeof(T).Name);
				instance = obj.AddComponent<T>();
			}
			*/
			
			return instance;
		}
	}	
}
