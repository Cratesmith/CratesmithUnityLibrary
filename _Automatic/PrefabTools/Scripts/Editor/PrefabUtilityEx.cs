using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class PrefabUtilityEx 
{
	public static bool HasPropertyModifications(Object obj, bool thisObjectOnly)
	{
		if(!obj)
		{
			return false;
		}
		
		var mods = GetPropertyModifications(obj, thisObjectOnly);
		var root = PrefabUtility.FindPrefabRoot(GetGameObject(obj));
		if(mods==null || (root != GetGameObject(obj) && mods.Length > 0))
		{
			return true;
		}
		
		var prefab = (GameObject)PrefabUtility.GetPrefabParent(root);
		
		// root prefabs tend to have overriden transform values all of the time
		// we should only show them if the value is changed though
    	foreach(var i in mods)
		{	
			if(i.target.GetType() != typeof(Transform))
				return true;
						
			float value;
			if(float.TryParse(i.value, out value))
			{
				float baseVal;
				if(i.propertyPath == "m_LocalPosition.x" ) baseVal = prefab.transform.localPosition.x;
				else if(i.propertyPath == "m_LocalPosition.y" ) baseVal = prefab.transform.localPosition.y;
				else if(i.propertyPath == "m_LocalPosition.z" ) baseVal = prefab.transform.localPosition.z;
				else if(i.propertyPath == "m_LocalRotation.x" ) baseVal = prefab.transform.localRotation.x;
				else if(i.propertyPath == "m_LocalRotation.y" ) baseVal = prefab.transform.localRotation.y;
				else if(i.propertyPath == "m_LocalRotation.z" ) baseVal = prefab.transform.localRotation.z;
				else if(i.propertyPath == "m_LocalRotation.w" ) baseVal = prefab.transform.localRotation.w;
				else if(i.propertyPath == "m_LocalScale.x" ) baseVal = prefab.transform.localScale.x;
				else if(i.propertyPath == "m_LocalScale.y" ) baseVal = prefab.transform.localScale.y;
				else if(i.propertyPath == "m_LocalScale.z" ) baseVal = prefab.transform.localScale.z;
				else return true;
				
				if(Mathf.Approximately(baseVal, value))
					continue;
			}
			return true;
		}
		
		return false;
	}
	
	public static bool IsPrefabInstanceModified(Object obj, bool thisObjectOnly)
	{
		if(!obj)
		{
			return false;
		}
		
    	var mods = GetPropertyModifications(obj, thisObjectOnly);
    	bool isValueOverriden = HasPropertyModifications(obj, thisObjectOnly);
    	bool isComponentAdded = false;
		
		var gameObj = obj as GameObject;
		if(gameObj && !thisObjectOnly)
		{
			Component[] components = PrefabUtility.FindPrefabRoot(gameObj).GetComponentsInChildren<Component>();
			isComponentAdded = components.Any((arg) => PrefabUtility.IsComponentAddedToPrefabInstance(arg));
		}
		else 
			isComponentAdded = PrefabUtility.IsComponentAddedToPrefabInstance(obj);
		
		return isValueOverriden || isComponentAdded;
	}
	
	public static void RevertInstance(Object obj)
	{
		PrefabUtility.RevertPrefabInstance(obj as GameObject);
		//PrefabUtility.SetPropertyModifications(obj, new PropertyModification[0]);
		EditorUtility.SetDirty(obj);
	}
	
	public static void ApplyInstance(Object obj)
	{
		var root = PrefabUtility.FindPrefabRoot(obj as GameObject);
		var parent = PrefabUtility.GetPrefabParent(obj);
		PrefabUtility.ReplacePrefab(root, parent, ReplacePrefabOptions.ConnectToPrefab);
		//PrefabUtility.SetPropertyModifications(obj, new PropertyModification[0]);
		EditorUtility.SetDirty(parent);
		EditorUtility.SetDirty(obj);
	}	

	public static PropertyModification[] GetPropertyModifications (Object obj, bool thisObjectOnly)
	{
		var mods = new List<PropertyModification>();
		
		if(thisObjectOnly)
		{
			var prefab = PrefabUtility.GetPrefabParent(obj);
			var modsArray = PrefabUtility.GetPropertyModifications(obj);
			mods.AddRange(modsArray!=null ? modsArray:new PropertyModification[0]);
			mods.RemoveAll((i) => i.target != prefab);
		}
		else
		{
			var prefab = GetRootPrefab(obj);
			var modsArray = PrefabUtility.GetPropertyModifications(obj);
			mods.AddRange(modsArray!=null ? modsArray:new PropertyModification[0]);
		}
		
		return mods.ToArray();
	}	
	
	public static Component[] GetRemovedComponentsFromPrefab(Object obj, bool thisObjectOnly)
	{
		HashSet<Component> instanceComponents;
		HashSet<Component> prefabComponents;
		Object instance;
		GameObject prefab;
		
		if(thisObjectOnly)
		{
			instance 	= obj;
			prefab 		= (GameObject)PrefabUtility.GetPrefabParent(instance);
			if(instance as GameObject)
				instanceComponents = new HashSet<Component>((instance as GameObject).GetComponents<Component>());
			else 
				instanceComponents = new HashSet<Component>((instance as Component).GetComponents<Component>());
			
			prefabComponents = new HashSet<Component>(prefab.GetComponents<Component>());
		}
		else
		{
			if(obj as GameObject)
			{
				instance	= PrefabUtility.FindPrefabRoot(obj as GameObject);
				prefab		= (GameObject)PrefabUtility.GetPrefabParent(instance);
				instanceComponents = new HashSet<Component>((instance as GameObject).GetComponentsInChildren<Component>());
				prefabComponents = new HashSet<Component>(prefab.GetComponentsInChildren<Component>());
			}
			else
			{
				instance	= PrefabUtility.FindPrefabRoot((obj as Component).gameObject);
				prefab		= (GameObject)PrefabUtility.GetPrefabParent(instance);
				instanceComponents = new HashSet<Component>((instance as Component).GetComponentsInChildren<Component>());
				prefabComponents = new HashSet<Component>(prefab.GetComponentsInChildren<Component>());				
			}
		}
		
		prefabComponents.RemoveWhere((i) => instanceComponents.Any((j) => PrefabUtility.GetPrefabParent(j)==i));
		return prefabComponents.ToArray();
	}
	
	public static string GetModificationDescription (Object obj, bool thisObjectOnly)
	{
		string output = "";
		bool modified = false;
		var mods = PrefabUtilityEx.GetPropertyModifications(obj, thisObjectOnly);
		if(mods != null && mods.Length > 0)
		{
			output += "Modified values:";
			foreach(var i in mods)
			{
				
				output += string.Concat("\n", i.target.name, ".", i.target.GetType().Name, " - ", i.propertyPath, ":", i.objectReference? i.objectReference.name:i.value);
			}
			
			modified = true;
		}
		
		if(PrefabUtility.IsComponentAddedToPrefabInstance(obj))
		{
			output += string.Concat("Added: ",obj.GetType().Name);
			modified = true;
		}
		
		var removedChildren = GetRemovedChildrenFromPrefab(obj, thisObjectOnly);
		if(removedChildren.Length > 0)
		{
			output += "Removed Children: ";
			foreach(var i in removedChildren)
			{
				output += string.Concat("\n", i.name);
			}
			modified = true;
		}
		
		if(modified)
			return output;
		
		return "Not modified";
	}
	
	static GameObject[] GetAllChildren(GameObject obj)
	{
		var children = new List<GameObject>(new GameObject[] {obj});
		for(int i=0; i<obj.transform.GetChildCount(); ++i)
		{
			children.AddRange(GetAllChildren(obj.transform.GetChild(i).gameObject));
		}
		return children.ToArray();
	}
	
	public static GameObject GetGameObject(Object obj)
	{
		if(obj as GameObject)
			return obj as GameObject;
		else if(obj as Component)
			return (obj as Component).gameObject;
		else 
			return null;
	}
	
	public static GameObject[] GetRemovedChildrenFromPrefab(Object instnanceObj, bool thisObjectOnly)
	{
		GameObject instance = (thisObjectOnly) ?  GetGameObject(instnanceObj):PrefabUtility.FindPrefabRoot(GetGameObject(instnanceObj));
		GameObject prefab = (GameObject)PrefabUtility.GetPrefabParent(instance);
		
		var prefabChildrenSet = new HashSet<GameObject>(GetAllChildren (prefab));
		var instanceChildrenSet = new HashSet<GameObject>(GetAllChildren(instance).Select((arg) => GetGameObject(PrefabUtility.GetPrefabParent(arg))));
		
		var removedChildren = new HashSet<GameObject>(prefabChildrenSet);
		removedChildren.ExceptWith(instanceChildrenSet);
		
		return removedChildren.ToArray();
	}
	
	public static void RemoveChildrenAndReconnectToLastPrefab(GameObject instanceObj, bool thisObjectOnly)
	{
		GameObject instance = (thisObjectOnly) ? instanceObj:PrefabUtility.FindPrefabRoot(instanceObj);
		GameObject prefab = (GameObject)PrefabUtility.GetPrefabParent(instance);

		var removedChildren = GetRemovedChildrenFromPrefab(instanceObj,false);
		foreach(var i in removedChildren)
		{
			if(i)
			{
				GameObject.DestroyImmediate(i, true);
			}
		}
		EditorUtility.SetDirty(prefab);
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(prefab));
		PrefabUtility.ReconnectToLastPrefab(instance);
	}

	public static bool IsPrefabRoot (Object obj)
	{
		return PrefabUtility.FindPrefabRoot(GetGameObject(PrefabUtility.GetPrefabParent(obj))) == PrefabUtility.GetPrefabParent(obj);
	}
	
	/*
	public static void ApplySpecificChanges (GameObject obj, PropertyModification[] changes)
	{
		
	}
	*/
	
	public static void RevertSpecificChanges (Object obj, PropertyModification[] changes)
	{
		var root = PrefabUtility.FindPrefabRoot(GetGameObject(obj));
		var modsArray = PrefabUtility.GetPropertyModifications(root);
		var mods = new List<PropertyModification>(modsArray != null ? modsArray:new PropertyModification[0]);
		
	
		if(obj == root.transform)
		{
			var prefabTForm = (Transform)PrefabUtility.GetPrefabParent(root.transform);
			var baseMods = new PropertyModification[] 
			{ new PropertyModification() { propertyPath = "m_LocalPosition.x", target = prefabTForm, value=prefabTForm.localPosition.x.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalPosition.y", target = prefabTForm, value=prefabTForm.localPosition.y.ToString() },
		      new PropertyModification() { propertyPath = "m_LocalPosition.z", target = prefabTForm, value=prefabTForm.localPosition.z.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalRotation.x", target = prefabTForm, value=prefabTForm.localRotation.x.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalRotation.y", target = prefabTForm, value=prefabTForm.localRotation.y.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalRotation.z", target = prefabTForm, value=prefabTForm.localRotation.z.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalRotation.w", target = prefabTForm, value=prefabTForm.localRotation.w.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalScale.x", target = prefabTForm, value=prefabTForm.localScale.x.ToString() },
			  new PropertyModification() { propertyPath = "m_LocalScale.y", target = prefabTForm, value=prefabTForm.localScale.y.ToString() },
		      new PropertyModification() { propertyPath = "m_LocalScale.z", target = prefabTForm, value=prefabTForm.localScale.z.ToString() } };
			mods = new List<PropertyModification>(baseMods);
		}	
		else 
		{
			foreach(var i in changes)
			{
				mods.RemoveAll((a) => ComparePropertyModifications(i,a));
			}
		}
		
		var newChanges = mods.ToArray();
		PrefabUtility.SetPropertyModifications(obj, newChanges);
	}
	
	static bool ComparePropertyModifications(PropertyModification a, PropertyModification b)
	{
		return a.objectReference == b.objectReference && a.propertyPath == b.propertyPath && a.target == b.target && a.value == b.value;
	}
	
	static Component[] GetRequiredComponents(Component obj)
	{
		var reqs = new HashSet<Component>();
		foreach(RequireComponent i in obj.GetType().GetCustomAttributes(typeof(RequireComponent), true))
		{
			reqs.Add(obj.gameObject.GetComponent(i.m_Type0));
			reqs.Add(obj.gameObject.GetComponent(i.m_Type1));
			reqs.Add(obj.gameObject.GetComponent(i.m_Type2));
		}
		
		// things that need rigidbodies
		if(obj as Joint)
		{
			reqs.Add(obj.gameObject.rigidbody);
		}
		
		
		
		reqs.Remove(null);
		
		return reqs.ToArray();
	}
	
	public static GameObject GetRootPrefab(Object obj)
	{
		if(PrefabUtility.GetPrefabType(obj)==PrefabType.None)
			return null;
		
		var root = PrefabUtility.FindPrefabRoot(GetGameObject(obj));
		var rootPrefab = root==null?null:(GameObject)PrefabUtility.GetPrefabParent(root);
		return rootPrefab;
	}
	
	public static bool AreInsideSamePrefab(Object a, Object b)
	{
		var prefabA = GetRootPrefab(a);
		var prefabB = GetRootPrefab(b);
		
		return prefabA == prefabB;
	}
	 
	public static void ApplyInstanceModificationsToPrefab(Component objInstance)
	{
	}

	public static bool IsInstance(Object obj)
	{
		PrefabType type = PrefabUtility.GetPrefabType(obj);
		return type != PrefabType.Prefab 
			&& type != PrefabType.ModelPrefab;
	}
	
	public static bool IsPrefabInstance(Object obj)
	{
		PrefabType type = PrefabUtility.GetPrefabType(obj);
		return type == PrefabType.PrefabInstance 
			|| type == PrefabType.ModelPrefabInstance 
			|| type == PrefabType.DisconnectedModelPrefabInstance 
			|| type == PrefabType.DisconnectedPrefabInstance;
	}
	
	public static void AddInstanceComponentToPrefab(Component obj)
	{		
		var gameObject = PrefabUtility.FindPrefabRoot(GetGameObject(obj));
		var prefab = (GameObject)PrefabUtility.GetPrefabParent(gameObject);
		
		foreach(var i in GetRequiredComponents(obj))
		{
			if(i && PrefabUtility.GetPrefabParent(i) == null && prefab.GetComponent(i.GetType()) == null)
				AddInstanceComponentToPrefab(i);
		}

		// add the prefab to the target
		var prefabComp = prefab.AddComponent(obj.GetType());
		if(prefabComp)
		{
			var mods = new List<PropertyModification>(PrefabUtility.GetPropertyModifications(gameObject));
			
			// copy data
			EditorUtility.CopySerialized(obj, prefabComp);
			
			// fix up any references to their prefabs
			SerializedObject so = new SerializedObject(obj);
			SerializedObject prefabCompSo = new SerializedObject(prefabComp);
			prefabCompSo.Update();
			so.Update();
			var i = so.GetIterator();
			while(i.Next(true))
			{
				if(i.propertyType == SerializedPropertyType.ObjectReference)
				{		
					if(i.propertyPath == "m_PrefabParentObject"
						|| i.propertyPath == "m_PrefabInternal"
						|| i.propertyPath == "m_GameObject"
						|| i.propertyPath == "m_Script")
						continue;
					
					var prefabRef = PrefabUtility.GetPrefabParent(i.objectReferenceValue);
					var prefabRefRoot = PrefabUtility.GetPrefabParent(PrefabUtility.FindPrefabRoot(PrefabUtilityEx.GetGameObject(i.objectReferenceValue)));
					var prefabType = i.objectReferenceValue!= null ? PrefabUtility.GetPrefabType(i.objectReferenceValue):PrefabType.None;
					
					if(prefabType != PrefabType.Prefab && prefabType != PrefabType.ModelPrefab)
					{
						// link to an object in the scene.
						// we must add a modification for this.
						if(i.objectReferenceValue != null && (prefabRef == null || prefab != prefabRefRoot))
						{
							var propertyMod = new PropertyModification();
							propertyMod.objectReference = i.objectReferenceValue;
							propertyMod.target = prefabComp;
							propertyMod.propertyPath = i.propertyPath;
							mods.Add(propertyMod);
							
							prefabCompSo.FindProperty(i.propertyPath).objectReferenceValue = null;
						}
						else 
							prefabCompSo.FindProperty(i.propertyPath).objectReferenceValue = prefabRef;
					}
				}
			}
			
			so = null;
			
			// save
			prefabCompSo.ApplyModifiedProperties();
			EditorUtility.SetDirty(prefab);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(prefab));
			// remove the applied value from our instance.
			UnityEngine.GameObject.DestroyImmediate(obj);
			
			// add property mods
			var components = gameObject.GetComponents(prefabComp.GetType());
			Component newComponent = System.Array.Find(components, ((arg) => PrefabUtility.GetPrefabParent(arg)==prefabComp));
			PrefabUtility.SetPropertyModifications(newComponent, mods.ToArray());
		}
		else 
			EditorUtility.DisplayDialog("Error", "Can't apply this component to the prefab as it cannot own more than one prefab of this type", "Ok");
	}
}
