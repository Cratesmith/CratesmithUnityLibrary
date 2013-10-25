using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PoolTable
{
    [SerializeField] string name;
	[SerializeField] GameObject prefab;
	[SerializeField] List<GameObject> inUse = new List<GameObject>();
	[SerializeField] List<GameObject> free = new List<GameObject>();
	[SerializeField] List<GameObject> newObjects = new List<GameObject>();
	List<GameObject> despawnQueue = new List<GameObject>();
	
	public PoolTable(GameObject prefab)
    {
        this.name = prefab.name;
        this.prefab = prefab;
    }
	
	public GameObject Prefab { get { return prefab; } } 
    public int ActiveCount { get { return inUse.Count; } }
    public int FreeCount { get { return free.Count; } }
    public List<GameObject> ActiveObjects { get { return inUse; } }
	
	private GameObject CreateNew(Vector3 position, Quaternion rotation)
	{
		 GameObject obj = (GameObject)GameObject.Instantiate(prefab, position, rotation);

		if(obj.GetComponentInChildren<ParticleEmitter>() != null)
			PooledParticles.Apply(obj);
		
		return obj;
	}
	
    public void Preallocate(int count)
    {
        count -= inUse.Count;
        count -= free.Count;
        
        while (count > 0)
        {
            GameObject obj = CreateNew(Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            obj.transform.parent = null;
            obj.hideFlags = HideFlags.HideInHierarchy;
            free.Add(obj);
            
            --count;
        }
    }
    
	public GameObject Spawn(Vector3 position, Quaternion rotation)
	{
		GameObject obj = null;
		//free.RemoveAll(m => m == null);
		if(free.Count == 0)
		{
#if DEBUG
            Debug.LogError("Spawning new: " + prefab.name);
#endif
			obj = CreateNew(position, rotation);
		}
		else 
		{
			obj = free[0]; 
			free.RemoveAt(0);
            
            obj.transform.position = position;
            obj.transform.rotation = rotation;
			
			
			if(obj.rigidbody)
			{
				obj.rigidbody.velocity = Vector3.zero;
				obj.rigidbody.angularVelocity = Vector3.zero;
			}
			
			obj.SetActive(true);		
            obj.hideFlags = 0;
			//obj.BroadcastMessage("Awake", SendMessageOptions.DontRequireReceiver);			
			newObjects.Add(obj);
            
            //Debug.Log("Spawning existing: " + obj.name);
		}
		
		inUse.Add(obj);
		return obj;
	}
	
	public void Update()
	{
		while(despawnQueue.Count > 0)
		{
			GameObject obj = despawnQueue[0];
            despawnQueue.RemoveAt(0);
			//obj.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);
			//obj.SetActiveRecursively(false);
			//obj.transform.parent = null;
			//obj.hideFlags = HideFlags.HideInHierarchy;
			//obj.transform.position = Vector3.zero;
			//obj.transform.rotation = Quaternion.identity;
			//obj.transform.localScale = Vector3.one;
			
			Rigidbody rb = obj.rigidbody;
			if(rb)
			{
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}
	
			inUse.Remove(obj);
			free.Add(obj);		
		}
		
		newObjects.RemoveAll(m => m == null);
		
		//foreach(var obj in newObjects)	
		//	obj.BroadcastMessage("Start", SendMessageOptions.DontRequireReceiver);
		
		newObjects.Clear();
	}
	
	public void Despawn(GameObject obj)
	{
        //Debug.Log("Despawning: " + obj.name);
        
		if(!despawnQueue.Contains(obj) && obj != null)
        {
            obj.SetActive(false);
            obj.transform.parent = null;
            obj.hideFlags = HideFlags.HideInHierarchy;
            
			despawnQueue.Add(obj);
        }
	}
	
	public void SetPrefab(GameObject newPrefab)
	{
		if (prefab == newPrefab)
		{
			return;
		}
		
		
		prefab = newPrefab;
	}
    
    public override string ToString()
    {
        return name;
    }
}
