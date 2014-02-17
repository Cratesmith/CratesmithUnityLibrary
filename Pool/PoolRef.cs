using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PoolRefBase<T>
{
	protected T 			cachedObj;
	protected Pool 			cachedPool;
	protected PoolInstance 	ptr;
	
	public PoolRefBase(PoolRefBase<T> other)
	{
		ptr = other.ptr;
		cachedPool = Pool.Instance;
		cachedObj = other.cachedObj;
	}
	
	public PoolRefBase(T src)
	{
		cachedObj = src;
	}
	
	abstract public T Ref { get; }
}

// A pool reference for game objects, works just fine for non-pooled objects too!
public class PoolRef : PoolRefBase<GameObject>
{
	public override GameObject Ref { get {return (GameObject)this;} }
	
	public PoolRef(PoolRef other) : base ((PoolRefBase<GameObject>)other)
	{		
	}
	
	public PoolRef(GameObject src) : base(src)
	{
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

// A pool reference components, works just fine for non-pooled components too!
public class PoolRef<T> : PoolRefBase<T> where T:Component
{
	public override T Ref { get {return (T)this;} }
	
	public PoolRef(PoolRef<T> other) : base ((PoolRefBase<T>)other)
	{		
	}
	
	public PoolRef(T src) : base(src)
	{
		cachedPool = Pool.Instance;
		if(src != null)
			ptr = cachedPool.GetPoolInstance(src.gameObject);
	}
	
	public static implicit operator PoolRef<T>(T other) 
	{
		return new PoolRef<T>(other);
	}
	
	public static implicit operator bool(PoolRef<T> me) 
	{
		return ((T)me) != null;
	}
	
	public static implicit operator T(PoolRef<T> me) 
	{
		if(me == null)
			return null;
		
		if(me.ptr == null && me.cachedObj != null)
			me.ptr = me.cachedPool.GetPoolInstance(me.cachedObj.gameObject);
		
		// no pointer, this is a non-pool object
		if(me.ptr == null)
		{
			if(me.cachedObj)
				return me.cachedObj.GetComponent<T>();
			else 
				return null;
		}
		
		// check that the object hasn't been despawned
		if(me.ptr.Instance == null || me.ptr.InUse == false)
			return null;
		
		// return the component from the object
		return me.ptr.Instance.GetComponent<T>();
	}
}