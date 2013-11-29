using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

//
// Data structures for runtime
//

[System.Serializable]
public class ControllerState
{
	public string uniqueName;
	public string name;
	public int hash;
}

public enum ControllerParameterType
{
	#if UNITY_4_2
	Vector,
	#else
	Trigger,
	#endif
	Float,
	Int,
	Bool
}

[System.Serializable]
public class ControllerParameter
{
	public string name;
	public ControllerParameterType type;
}

[System.Serializable]
public class Controller
{
	public RuntimeAnimatorController 	controller;
	public ControllerState[] 			states;
	public ControllerParameter[]		parameters;
	public string[]						layers;
}

//
// Database (contains building code)
// gets added into every scene as a gameobject
//

public class RuntimeAnimatorControllerExtensionData : MonoBehaviour
{
	[SerializeField] Controller[] controllers;
	
	static RuntimeAnimatorControllerExtensionData _instance = null;	
	static RuntimeAnimatorControllerExtensionData Instance 
	{
		get 
		{
			if(!_instance)
				_instance = (RuntimeAnimatorControllerExtensionData)FindObjectOfType(typeof(RuntimeAnimatorControllerExtensionData));
			
			return _instance;
		}
	}	
	
	Dictionary<RuntimeAnimatorController, Controller> _controllersTable;
	static Dictionary<RuntimeAnimatorController, Controller> ControllersTable 
	{
		get 
		{
			if(Instance._controllersTable==null)
			{
				Instance._controllersTable = new Dictionary<RuntimeAnimatorController, Controller>();
				foreach(var i in Instance.controllers)
				{
					Instance._controllersTable.Add(i.controller,i);
				}
			}
			
			return Instance._controllersTable;
		}
	}	
	
	#if UNITY_EDITOR
	[UnityEditor.Callbacks.PostProcessScene]
	static void BuildTable()
	{
		if(FindObjectOfType(typeof(RuntimeAnimatorControllerExtensionData)))
			return;
		
		GameObject obj = new GameObject("_RuntimeAnimatorControllerExtensionData");
		var table = obj.AddComponent<RuntimeAnimatorControllerExtensionData>();
		
		var list = new List<Controller>();
		
		foreach(var i in UnityEditor.AssetDatabase.GetAllAssetPaths().Where(j => System.IO.Path.GetExtension(j).ToLower()==".controller"))
		{
			var ac = (UnityEditorInternal.AnimatorController)UnityEditor.AssetDatabase.LoadAssetAtPath(i, typeof(UnityEditorInternal.AnimatorController));
			if(!ac) continue;
			
			var entry = new Controller();
			list.Add(entry);
			entry.controller = ac;
			entry.layers 	 = ac.EnumerateLayers().Select((arg) => arg.name).ToArray();
			entry.parameters = ac.EnumerateParameters().Select((param) => new ControllerParameter() { name = param.name, type = ConvertParameterType(param.type)}).ToArray();
			entry.states	 = ac.EnumerateStatesRecursive().Select((arg) => new ControllerState() { name = arg.name, uniqueName = arg.stateMachine.name+"."+arg.name, hash = Animator.StringToHash(arg.stateMachine.name+"."+arg.name)}).ToArray();
		}
		
		table.controllers = list.ToArray();
	}	
	
	static ControllerParameterType ConvertParameterType(UnityEditorInternal.AnimatorControllerParameterType paramType)
	{
		switch(paramType)
		{
		case UnityEditorInternal.AnimatorControllerParameterType.Bool: 		return ControllerParameterType.Bool;
		case UnityEditorInternal.AnimatorControllerParameterType.Float: 	return ControllerParameterType.Float;
			#if UNITY_4_2
		case UnityEditorInternal.AnimatorControllerameterType.Vector: 		return ControllerParameterType.Vector;
			#else 
		case UnityEditorInternal.AnimatorControllerParameterType.Trigger: 		return ControllerParameterType.Trigger;
			#endif
		case UnityEditorInternal.AnimatorControllerParameterType.Int: 		return ControllerParameterType.Int;
			
		}
		throw new System.Exception("Unknown type");
	}
	#endif
	
	public static IEnumerable<ControllerParameter> GetParameters(RuntimeAnimatorController rac)
	{
		var controller = ControllersTable[rac];
		return controller.parameters.AsEnumerable();
	}
	
	public static IEnumerable<ControllerState> GetStates(RuntimeAnimatorController rac)
	{
		var controller = ControllersTable[rac];
		return controller.states.AsEnumerable();
	}
	
	public static IEnumerable<string> GetLayers(RuntimeAnimatorController rac)
	{
		var controller = ControllersTable[rac];
		return controller.layers.AsEnumerable();
	}
	
	public static int GetLayerCount(RuntimeAnimatorController rac)
	{
		var controller = ControllersTable[rac];
		return controller.layers.Length;
	}
}

// Extensions to the animation controller class
// these are helper functions used to build the database.
#if UNITY_EDITOR
public static class AnimationControllerExtensions
{
	public static IEnumerable<UnityEditorInternal.State> EnumerateStatesRecursive(this UnityEditorInternal.AnimatorController ac)
	{
		for(int i=0; i<ac.layerCount;++i)
		{
			var layer = ac.GetLayer(i);
			foreach(var j in EnumerateStatesRecursive(layer.stateMachine))
				yield return j;
		}
	}
	
	public static IEnumerable<UnityEditorInternal.State> EnumerateStatesRecursive(this UnityEditorInternal.StateMachine sm) 
	{
		for(int i=0; i<sm.stateCount; ++i)
		{
			yield return sm.GetState(i);
		}
		
		for(int i=0; i<sm.stateMachineCount; ++i)
		{
			foreach(var j in EnumerateStatesRecursive(sm.GetStateMachine(i)))
				yield return j;
		}
	}
	
	public static IEnumerable<KeyValuePair<string, string>> EnumerateStateNamesRecursive(this UnityEditorInternal.AnimatorController ac)
	{
		for(int i=0; i<ac.layerCount;++i)
		{
			var layer = ac.GetLayer(i);
			foreach(var j in EnumerateStateNamesRecursive(layer.stateMachine))
				yield return j;
		}
	}
	
	public static IEnumerable<KeyValuePair<string, string>> EnumerateStateNamesRecursive(this UnityEditorInternal.StateMachine sm) 
	{
		for(int i=0; i<sm.stateCount; ++i)
		{
			yield return new KeyValuePair<string, string>(sm.name+"."+sm.GetState(i).name, sm.GetState(i).name);
		}
		
		for(int i=0; i<sm.stateMachineCount; ++i)
		{
			var ssm = sm.GetStateMachine(i);
			foreach(var j in EnumerateStateNamesRecursive(ssm))
				yield return j;
		}
	}
	
	public static IEnumerable<UnityEditorInternal.AnimatorControllerParameter> EnumerateParameters(this UnityEditorInternal.AnimatorController ac)
	{
		for(int i=0; i<ac.parameterCount; ++i)
		{
			yield return ac.GetParameter(i);
		}
	}
	
	public static IEnumerable<UnityEditorInternal.AnimatorControllerLayer> EnumerateLayers(this UnityEditorInternal.AnimatorController ac)
	{
		for(int i=0; i<ac.layerCount; ++i)
		{
			yield return ac.GetLayer(i);
		}
	}
}
#endif
