using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtensions 
{
	public static IEnumerable<Transform> EnumerateChildrenRecursive(this Transform transform)
	{
		for(int i=0; i<transform.childCount; ++i)
		{
			foreach(var j in EnumerateChildrenRecursive(transform.GetChild(i)))
			{
				yield return j;
			}
			
			yield return transform.GetChild(i);
		}
	}
	
	public static IEnumerable<Transform> EnumerateChildren(this Transform transform)
	{
		for(int i=0; i<transform.childCount; ++i)
		{
			yield return transform.GetChild(i);
		}
	}
	
	public static IEnumerable<Transform> EnumerateParents(this Transform transform)
	{
		Transform current = transform;
		current = current.parent;
		while(current != null)
		{
			yield return current;
			current = current.parent;	
		}
	}

	
	public static Transform[] GetChildren(this Transform transform)
	{
		var output = new Transform[transform.childCount];
		for(int i=0; i<transform.childCount; ++i)
		{
			output[i] = transform.GetChild(i);
		}
		return output;
	}
}
