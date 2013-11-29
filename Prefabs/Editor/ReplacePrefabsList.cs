using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class ReplacePrefabsList : CustomAsset 
{
	[System.Serializable]
	public class Item
	{
		public GameObject 	sourcePrefab;
		public GameObject[] destPrefabs;
	}
	public GameObject[] dragNewItemsHere;
	public Item[] items;

	static bool hasProcessedThisBuild = false;
	[UnityEditor.Callbacks.PostProcessScene]
	public static void PostProcessScene()
	{
		if(hasProcessedThisBuild)
			return;

		var lists = Resources.FindObjectsOfTypeAll<ReplacePrefabsList>();
		foreach(var i in lists.Where(x => x!=null))
		{
			foreach(var j in i.items.Where(x => x!=null))
			{
				foreach(var k in j.destPrefabs.Where(x => x!=null))
				{
					PrefabUtility.ReplacePrefab(j.sourcePrefab, k);
				}
			}
		}

		hasProcessedThisBuild = true;
	}
}