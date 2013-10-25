using UnityEngine;
using System.Collections;
using System.Linq;

public class DontRebuildInPlayMode : MonoBehaviour 
{
#if UNITY_EDITOR
	private static int sceneId = 0;
	
	[UnityEditor.Callbacks.PostProcessScene]
	public static void AddToLoaderScene()
	{
		++sceneId;
		if((Application.isPlaying && sceneId!=1)) // note - this only occurs in editor
			return;
		
		//Debug.Log("Adding FBX processor");
		
		GameObject obj = new GameObject("_DontRebuildInPlayMode");
		obj.AddComponent<DontRebuildInPlayMode>();

		obj.hideFlags = HideFlags.HideAndDontSave;
		DontDestroyOnLoad(obj);
	}


	void Update()
	{
		if(UnityEditor.EditorApplication.isCompiling && UnityEditor.EditorApplication.isPlaying)
		{
		        Debug.Log("Compiled during play; automatically quit.");
		        UnityEditor.EditorApplication.Beep();
		        UnityEditor.EditorApplication.isPlaying = false;
		}
	}
#endif
}
