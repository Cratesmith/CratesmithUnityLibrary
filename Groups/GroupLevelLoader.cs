using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupLevelLoader : MonoBehaviour 
{
	public static void Create (string id, string sceneName, string tagName)
	{
		var obj = new GameObject("GroupLevelLoader");
		var loader = obj.AddComponent<GroupLevelLoader>();
		loader.DoLoadTag(id, sceneName, tagName);
	}
	
	public static void Create (string id, string sceneName)
	{
		var obj = new GameObject("GroupLevelLoader");
		var loader = obj.AddComponent<GroupLevelLoader>();
		loader.DoLoad(id, sceneName);
	}
	
	
	private void DoLoad(string id, string sceneName)
	{
		StartCoroutine(LoadCoroutine(id,sceneName));
	}

	
	IEnumerator LoadTagCoroutine (string id, string sceneName, string tagName)
	{
		var before = new HashSet<GameObject>((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject)));
		Application.LoadLevelAdditive(sceneName);
		
		yield return new WaitForEndOfFrame();
				
		var after = new HashSet<GameObject>((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject)));
		var newObjects = after.Except(before).ToList();				
		var destroy = newObjects.Where (arg => arg.tag != tagName).ToArray();
				
		foreach(var i in destroy)
		{			
			var parent = i.transform.parent;
			while(parent && parent.tag != tagName)
			{
				parent = parent.transform.parent;
			}
			
			if(parent) 
				continue;
			
			GameObject.DestroyImmediate(i);
		}
		newObjects.RemoveAll((obj) => !obj);
		
		Group.AddToGroup(id, newObjects);
	}
	
	
	private void DoLoadTag(string id, string sceneName, string tagName)
	{
		StartCoroutine(LoadTagCoroutine(id,sceneName, tagName));
	}
	
	
	IEnumerator LoadCoroutine (string id, string sceneName)
	{
		var before = new HashSet<GameObject>((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject)));
		Application.LoadLevelAdditive(sceneName);
		
		yield return 0;
				
		var after = new HashSet<GameObject>((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject)));
		var newObjects = after.Except(before).ToList();
		Group.AddToGroup(id, newObjects);
	}
}
