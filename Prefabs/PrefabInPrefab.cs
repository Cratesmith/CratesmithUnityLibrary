#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class PrefabInPrefab : MonoBehaviour 
{
	[SerializeField] GameObject sourcePrefab;
	[SerializeField] [HideInInspector] GameObject prefabInstance;
	
	void Awake()
	{
		CreateInstance();
	}

	void CreateInstance()
	{
		DestroyInstance();
		prefabInstance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
		prefabInstance.transform.parent = transform;
		prefabInstance.transform.localPosition = sourcePrefab.transform.localPosition;
		prefabInstance.transform.localRotation = sourcePrefab.transform.localRotation;
		prefabInstance.transform.localScale = sourcePrefab.transform.localScale;
		prefabInstance.hideFlags = HideFlags.DontSave;
	}

	void DestroyInstance()
	{
		DestroyImmediate(prefabInstance);
	}

	/*
	[SerializeField] GameObject destChildObject;

	static bool hasProcessedThisBuild = false;
	[MenuItem("Test/Try applying prefab in prefab")]
	[UnityEditor.Callbacks.PostProcessScene]
	static void PostProcessScene()
	{
		if(hasProcessedThisBuild)
			return;

		var lists = Resources.FindObjectsOfTypeAll<PrefabInPrefab>().Where(x => x!=null &&  (PrefabUtility.GetPrefabType(x) == PrefabType.ModelPrefab || PrefabUtility.GetPrefabType(x) == PrefabType.Prefab));
		foreach(var pip in lists)
		{
			pip.Apply();
		}
	}
	void Apply()
	{
		var tempInstance = PrefabUtility.InstantiatePrefab(this) as PrefabInPrefab;
		var newDestChildObject = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
		newDestChildObject.transform.parent 		= tempInstance.destChildObject.transform.parent;
		newDestChildObject.transform.localPosition 	= tempInstance.destChildObject.transform.localPosition;
		newDestChildObject.transform.localScale 	= tempInstance.destChildObject.transform.localScale;
		DestroyImmediate(tempInstance.destChildObject);
		tempInstance.destChildObject = newDestChildObject;
		PrefabUtility.ReplacePrefab(tempInstance.gameObject, this);
		DestroyImmediate(PrefabUtility.FindPrefabRoot(tempInstance.gameObject));
	}
	*/
}
#endif