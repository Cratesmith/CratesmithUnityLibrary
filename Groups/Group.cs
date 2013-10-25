using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Groups
// Objects can belong to multiple groups.
public class Group : ScriptableObject, IEnumerable<GameObject>
{
	#region static		
	#region private_static
	static Dictionary<string, Group> groups = new Dictionary<string, Group>();
	
	private static Group CreateEmptyGroup(string id)
	{
		var group = ScriptableObject.CreateInstance<Group>();
		group.name = id;
		
		groups.Add(id, group);
		return group;
	}
	
	private static Group GetOrCreateEmptyGroup(string id)
	{
		Group group = null;
		if(!groups.TryGetValue(id, out group))
			group = CreateEmptyGroup(id);
		
		return group;
	}
	#endregion
	public static void DestroyGroup(GroupId id, bool despawnObjects)
	{
		DestroyGroup(id.name, despawnObjects);	
	}
	
	public static void DestroyGroup(string id, bool despawnObjects)
	{
		var group = Get(id);
		if(group==null)
			return;
		
		if(despawnObjects)
		{
			group.DespawnObjects();
		}
		
		ScriptableObject.Destroy(group);
	}
	
	public static Group Get(GroupId id) 
	{
		return Get(id.name);
	}
	
	public static Group Get(string id)
	{
/*		Group result = null;
		groups.TryGetValue(id, out result);
		return result;
		*/
		return GetOrCreateEmptyGroup(id);
	}
	
	public static GameObject Spawn(string id, GameObject prefab, Vector3 position, Quaternion rotation)
	{
		var group = GetOrCreateEmptyGroup(id);
		return group.Spawn(prefab, position, rotation);
	}
	
	public static GameObject Spawn(GroupId id, GameObject prefab, Vector3 position, Quaternion rotation)
	{
		var group = GetOrCreateEmptyGroup(id.name);
		return group.Spawn(prefab, position, rotation);
	}
	
	public static GameObject Spawn(string id, GameObject prefab)
	{
		var group = GetOrCreateEmptyGroup(id);
		return group.Spawn(prefab);
	}
	
	public static GameObject Spawn(GroupId id, GameObject prefab)
	{
		return Spawn(id.name, prefab);
	}
	
	public static void AddToGroup(GroupId id, GameObject obj)
	{
		AddToGroup(id.name, obj);
	}
	
	public static void AddToGroup(string id, GameObject obj)
	{
		var group = GetOrCreateEmptyGroup(id);
		group.Add(obj);	
	}
	
	public static void LoadSceneAsGroup(GroupId id, string sceneName)
	{
		LoadSceneAsGroup(id.name, sceneName);
	}
	
	public static void LoadSceneAsGroup(string id, string sceneName)
	{
		GroupLevelLoader.Create(id, sceneName);
	}

	public static void LoadTagFromSceneAsGroup(GroupId id, string sceneName, string tagName)
	{
		LoadTagFromSceneAsGroup(id.name, sceneName, tagName);
	}
	
	public static void LoadTagFromSceneAsGroup(string id, string sceneName, string tagName)
	{
		GroupLevelLoader.Create(id, sceneName, tagName);
	}

	public static void AddToGroup(GroupId id, IEnumerable<GameObject> objs)
	{
		AddToGroup(id.name, objs);
	}
	
	public static void AddToGroup(string id, IEnumerable<GameObject> objs)
	{
		var group = GetOrCreateEmptyGroup(id);
		group.Add(objs);	
	}
	#endregion

	#region instance
	public int NumActiveObjects {get {return objects.Count((a) => a && a.activeInHierarchy); } }
	public int NumObjects {get {return objects.Count((a) => a); } }
	public string Id {get {return name; } }
	
	public GameObject[] Objects 
	{ 
		get 
		{ 
			
			// cleanup dead items
			objects.RemoveWhere((obj) => !obj);
			return objects.ToArray();
		} 
	}
	
	public GameObject[] ActiveObjects 
	{ 
		get 
		{ 
			// cleanup dead items
			objects.RemoveWhere((obj) => !obj);
			return objects.Where((obj) => obj.activeInHierarchy).ToArray();
		} 
	}
	
	private HashSet<GameObject> objects = new HashSet<GameObject>();
	
	public void Add(GameObject obj)
	{
		objects.Add(obj);	
	}

	public void Add(IEnumerable<GameObject> range)
	{
		foreach(GameObject i in range)
			objects.Add(i);	
	}
	
	public void Remove(GameObject obj)
	{
		objects.Remove(obj);
	}
	
	public void Remove(IEnumerable<GameObject> range)
	{
		foreach(GameObject i in range)
			objects.Remove(i);	
	}
	
	public void DespawnObjects()
	{
		foreach(var i in objects)
			Pool.Despawn(i);
		
		objects.Clear();
	}
	
	public void GroupSendMessage(string methodName, SendMessageOptions options)
	{
		GroupSendMessage(methodName, null, options);
	}
	
	public void GroupSendMessage(string methodName, Object parameter, SendMessageOptions options)
	{
		foreach(var i in objects)
		{
			i.SendMessage(methodName, parameter, options);
		}
	}
	
	public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		var result = Pool.Spawn(prefab, position, rotation);
		if(result)
		{
			Add(result);
		}
		
		return result;
	}
	
	public GameObject Spawn(GameObject prefab)
	{
		var result = Pool.Spawn(prefab);
		if(result)
		{
			Add(result);
		}
		
		return result;
	}
	
	public GameObject Random()
	{
		return objects.ElementAt(UnityEngine.Random.Range(0, NumObjects));
	}
	
	void OnDestroy()
	{
		groups.Remove(Id);
	}

	#region IEnumerable[GameObject] implementation
	IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator ()
	{
		return objects.GetEnumerator();
	}
	#endregion

	#region IEnumerable implementation
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return objects.GetEnumerator();
	}
	#endregion
	
	#endregion
	
	
}
