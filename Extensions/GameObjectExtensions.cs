using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T>(this GameObject obj) where T: Component
	{
		var c = obj.GetComponent<T>();
		if(!c) c = obj.AddComponent<T>();
		return c;
	}
}
