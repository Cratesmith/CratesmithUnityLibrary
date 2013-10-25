using UnityEngine;
using System.Collections;
using System.Linq;

public class FbxAnimationProcessor : MonoBehaviour 
{
	public FbxAnimationClipTable[] tables;
	static bool applied = false;
	
#if UNITY_EDITOR
	private static int sceneId = 0;
	
	[UnityEditor.Callbacks.PostProcessScene]
	public static void AddToLoaderScene()
	{
		++sceneId;
		if((Application.isPlaying && sceneId!=1) || (!Application.isPlaying && sceneId!=2))
			return;
		
		//Debug.Log("Adding FBX processor");
		
		GameObject obj = new GameObject("_FbxAnimationProcessor");
		var processor = obj.AddComponent<FbxAnimationProcessor>();
		
		processor.tables = UnityEditor.AssetDatabase.GetAllAssetPaths()
				.Where(i => i.EndsWith(".asset"))
				.Select(i => UnityEditor.AssetDatabase.LoadAssetAtPath(i, typeof(FbxAnimationClipTable)))
				.Where(i => i != null)
				.Cast<FbxAnimationClipTable>()
				.ToArray();
		
		if(Application.isPlaying)
		{
			processor.Apply();
		}
	}
#endif
	
	
	void Awake()
	{
		if(tables !=null)
			Apply();
	}
	
	void Apply()
	{
		if(applied) return;
		foreach(var i in tables)
		{
			i.Process();
		}
		applied = true;
	}
}
