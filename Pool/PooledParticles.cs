
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PooledParticles : MonoBehaviour
{
	List<ParticleEmitter> 	emitOnStart = new List<ParticleEmitter>();
	List<ParticleEmitter> 	autoStopEmitters = new List<ParticleEmitter>();
	bool 					despawnOnAutoStop = false; 
    ParticleAnimator[]      animators;
    int                     updateCount = 0;
    
	
	public static void Apply(GameObject rootObj)
	{
		PooledParticles pp = rootObj.AddComponent<PooledParticles>();
		
		foreach(var a in pp.animators)
		{
			var e = a.particleEmitter;
			if(!e)
				continue;
			
			if(e.emit)
				pp.emitOnStart.Add(e);
			
			if(a.autodestruct)
			{
				if(a.transform == rootObj.transform)
					pp.despawnOnAutoStop = true;
				
				a.autodestruct = false;
				pp.autoStopEmitters.Add(e);
			}
		}
	}
	
    private void Awake()
    {
        animators = GetComponentsInChildren<ParticleAnimator>();
    }
    
	private void OnEnable()
	{
		ProcessAutodestroy();
        
        foreach(var e in emitOnStart)
            e.emit = true;
        
        updateCount = 0;
	}
	
	private void ProcessAutodestroy()
	{
		foreach(var a in animators)
		{
			if(!a.autodestruct || !a.particleEmitter)
				continue;
			
			a.autodestruct = false;
			if(!autoStopEmitters.Contains(a.particleEmitter))
				autoStopEmitters.Add(a.particleEmitter);
			
			if(a.transform == transform)
				despawnOnAutoStop = true;
		}
	}
	
	private void Update()
	{
        // @HACK: Skip first few updates
        ++updateCount;
        if (updateCount < 3)
        {
            return;
        }
        
		ProcessAutodestroy();			
		foreach(var e in autoStopEmitters)
		{
			if(e.particleCount > 0)
				continue;
			
			if(e.emit)
				e.emit = false;
			
			if(!despawnOnAutoStop || e.transform != transform)
				continue;
			
			Pool.Despawn(gameObject);
		}
	}
	
	public void OnDisable()
	{
		ParticleEmitter[] emitters = GetComponentsInChildren<ParticleEmitter>();
		foreach(var e in emitters)
			e.ClearParticles();
	}
}
