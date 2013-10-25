
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PoolInstance
{	
	GameObject 	instance;
	bool		inUse;
	PoolTable	table;

	public PoolInstance(GameObject instance, bool inUse, PoolTable table) 
	{
		this.instance = instance;
		this.inUse = inUse;
		this.table = table;
	}
	
	public GameObject Instance { get { return this.instance; } }
	public PoolTable Table { get { return this.table; } }
	
	public bool InUse 
	{
		get { return this.inUse; }
		set { inUse = value; }
	}

}
