using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : BehaviourSingleton<Pool>
{
	/*
	private static Pool _instance = null;
	
    public static Pool Instance 
	{
		get 
		{ 	
			if(_instance != null)
				return _instance;
			else 
				_instance =  (Pool)GameObject.FindObjectOfType(typeof(Pool));

			if(_instance == null && Application.isPlaying)
			{
				GameObject obj = new GameObject(typeof(Pool).Name);
				_instance = obj.AddComponent<Pool>();
			}
			
			return _instance;
		}
	}*/
	
	[System.Serializable]
	public class Preallocation
	{
		public GameObject prefab;
		public int count;
	}
	
	public Preallocation[] preallocations = new Preallocation[0];
	
	[SerializeField]
    List<PoolTable> items = new List<PoolTable>();
    Dictionary<GameObject, PoolTable> poolTables = new Dictionary<GameObject, PoolTable>();
    Dictionary<GameObject, PoolTable> overriddenPoolTables = new Dictionary<GameObject, PoolTable>();
    Dictionary<GameObject, PoolInstance> poolInstances = new Dictionary<GameObject, PoolInstance>();
 
	public List<PoolTable> Items
	{
		get { return items; }
	}
	
	private PoolTable GetOrCreateTable(GameObject prefab)
	{   
        PoolTable table;
        if (!poolTables.TryGetValue(prefab, out table))
        {
            table = new PoolTable(prefab);
            poolTables [prefab] = table;
            items.Add(table);
        }
		
		return table;
	}
	
    private void DoPreallocate(GameObject prefab, int count)
    {   
        GetOrCreateTable(prefab).Preallocate(count);
    }
    
    private GameObject DoSpawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        enabled = true;
     
		PoolTable table = GetOrCreateTable(prefab);
        GameObject obj = table.Spawn(position, rotation);
        poolInstances [obj] = new PoolInstance(obj, true, table);
        return obj;
    }
 
    private void DoDespawn(GameObject obj)
    {    
        PoolInstance inst;
        if (poolInstances.TryGetValue(obj, out inst))
        {
            PoolTable table = inst.Table;
            if (table != null)
            {
                inst.InUse = false;
                table.Despawn(obj);  
                enabled = true;
                return;
            }
        }
         
		//Debug.LogError("Could not find obj to despawn in pool: " + obj.name);
        GameObject.Destroy(obj);
    }
	
	private void DoDespawnAll()
	{
		foreach (var pair in poolInstances)
		{
			PoolInstance inst = pair.Value;
			if (inst.Table != null)
			{
                inst.InUse = false;
                inst.Table.Despawn(pair.Key);
			}
		}
		
        enabled = true;
	}
	
	private void DoReplace(GameObject prefab, GameObject otherPrefab)
	{
		Debug.Log("Replacing " + prefab.name + " with " + otherPrefab.name);
		
        PoolTable table;
        if (!poolTables.TryGetValue(prefab, out table))
        {
			Debug.LogError("Prefab does not exist to replace: " + prefab.name + " with: " + otherPrefab.name);
			return;
		}
		
		if (table.Prefab == otherPrefab)
		{
			Debug.Log("Prefab to replace already matches the new prefab, ignoring");
			return;
		}
		
		// Despawn current instances
		foreach (var pair in poolInstances)
		{
			if (pair.Value.Table == table)
			{
                pair.Value.InUse = false;
                table.Despawn(pair.Key);
			}
		}
		
		// Process despawns next update
        Instance.enabled = true;
		
		// Check overriden pool tables so see if other prefab already has a table
		PoolTable otherTable;
		if (overriddenPoolTables.TryGetValue(otherPrefab, out otherTable))
		{
			Debug.Log("Using existing overridden pool table");
			overriddenPoolTables.Remove(otherPrefab);
		}
		else
		{
			Debug.Log("Creating new pool table");
            otherTable = new PoolTable(otherPrefab);
        	items.Add(otherTable);
			
			// Preallocate the same number of instances
			otherTable.Preallocate(table.ActiveCount + table.FreeCount);
		}
		
		// Move the old table to the overriden tables
		overriddenPoolTables[table.Prefab] = table;
		
		// Replace the pool table reference
        poolTables[prefab] = otherTable;
	}
    
    protected override void Awake ()
	{
		base.Awake ();
		foreach (Preallocation preallocation in preallocations)
		{
			DoPreallocate(preallocation.prefab, preallocation.count);
		}
    }
 
    private void LateUpdate()
    {
        foreach (var table in items)
        {
            table.Update();
        }
     
        enabled = false;
    }
	
	public static PoolTable GetTable(GameObject prefab)
	{
		return Instance ? Instance.GetOrCreateTable(prefab) : null;
	}
 
    public static GameObject Spawn(GameObject prefab)
    {
        return Instance ? Instance.DoSpawn(prefab, Vector3.zero, Quaternion.identity) : null;
    }
 
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instance ? Instance.DoSpawn(prefab, position, rotation) : null;
    }
    
    public static void Despawn(GameObject obj)
    {
        if (Instance)
        {
            Instance.DoDespawn(obj);
        }
    }
    
    public static T Spawn<T>(T prefab) where T : Component
    {
        if (Instance)
        {
            GameObject obj = Instance.DoSpawn(prefab.gameObject, Vector3.zero, Quaternion.identity);
            if (obj)
            {
                return obj.GetComponent<T>();
            }
        }
        
        return null;
    }
    
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (Instance)
        {
            GameObject obj = Instance.DoSpawn(prefab.gameObject, position, rotation);
            if (obj)
            {
                return obj.GetComponent<T>();
            }
        }
        
        return null;
    }
    
	public static void Despawn(PoolRef obj) 
    {
        if (Instance)
        {
            Instance.DoDespawn(obj.Ref);
        }
    }
	
    public static void Despawn<T>(T obj) where T : Component
    {
        if (Instance)
        {
            Instance.DoDespawn(obj.gameObject);
        }
    }
	
	public static void DespawnAll()
	{
		if (Instance)
		{
			Instance.DoDespawnAll();
		}
	}
	
	public static void Replace(GameObject prefab, GameObject otherPrefab)
	{
		if (Instance)
		{
			Instance.DoReplace(prefab, otherPrefab);
		}
	}
	
	public static void Revert(GameObject prefab)
	{
		if (Instance)
		{
			Instance.DoReplace(prefab, prefab);
		}
	}
    
    public int GetActiveCount(GameObject prefab)
    {
        PoolTable table;
        if (poolTables.TryGetValue(prefab, out table))
        {
            return table.ActiveCount;
        }
        
        return 0;
    }
 
    public PoolInstance GetPoolInstance(GameObject obj)
    {
        PoolInstance inst;
        poolInstances.TryGetValue(obj, out inst);
        return inst;
    }
}