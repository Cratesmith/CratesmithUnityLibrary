using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// A pool reference for game objects, works just fine for non-pooled objects too!
public class PoolRef
{
    protected GameObject cachedObj;
    protected Pool cachedPool;
    protected PoolInstance ptr;
    
	public GameObject Ref { get {return (GameObject)this;} }
	
	public PoolRef(PoolRef other)
	{		
        ptr = other.ptr;
        cachedPool = Pool.Instance;
        cachedObj = other.cachedObj;
	}
	
	public PoolRef(GameObject src)
	{
        cachedObj = src;
		cachedPool = Pool.Instance;
		if(src != null)
			ptr = cachedPool.GetPoolInstance(src);
	}
	
	public static implicit operator PoolRef(GameObject other)
	{
		return new PoolRef(other);
	}
	
	public static implicit operator bool(PoolRef me) 
	{
		return ((GameObject)me) != null;
	}	
		
	public static implicit operator GameObject(PoolRef me) 
	{
		if(me == null)
			return null;
		
		if(me.ptr == null && me.cachedObj != null)
			me.ptr = me.cachedPool.GetPoolInstance(me.cachedObj);
		
		// no pointer, this is a non-pool object
		if(me.ptr == null)
		{
			return me.cachedObj;
		}
		
		// check that the object hasn't been despawned
		if(me.ptr.Instance == null || me.ptr.InUse == false)
			return null;
		
		// return the component from the object
		return me.ptr.Instance;
	}
}
