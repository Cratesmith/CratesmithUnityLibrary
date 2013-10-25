using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

[InitializeOnLoad]
public class SceneSanityCheck : UnityEditor.AssetModificationProcessor 
{
	const float SANITY_THRESHOLD = 0.0000025f;
	static SceneSanityData sanityData;

	static SceneSanityCheck()
	{
		EditorApplication.update += SceneSanityCheck.Update;	
	}
	 
	static void Update()
	{	
		//
		// THIS IS A HACK. IT DETECTS WHEN A NEW SCENE IS LOADED
		//		
		if(Time.realtimeSinceStartup <= EditorPrefs.GetFloat("LastLoadTime", float.MaxValue))
		{
			EditorPrefs.SetFloat("LastLoadTime", Time.realtimeSinceStartup);
			sanityData = SceneSanityData.Create();
		}
	}
	
	[System.Serializable]
	public class SceneSanityItem
	{
		public string 	name;
		public int 		instanceID;
		public Vector3 	localPosition;
		
		public SceneSanityItem()
		{
		}
		
		public SceneSanityItem(GameObject obj)
		{
			name = obj.name;
			instanceID = obj.GetInstanceID();
			localPosition = obj.transform.localPosition;
		}

		public void ApplySanityChanges(float sanityThrehold)
		{
			var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if(!obj)
			{
				return;
			}
			
			bool modified = false;
			Vector3 objPos = obj.transform.localPosition;
			if(objPos.x != localPosition.x && Mathf.Abs(objPos.x-localPosition.x) <= sanityThrehold)
			{
				objPos.x = localPosition.x;
				modified = true;
			}
			
			if(objPos.y != localPosition.y && Mathf.Abs(objPos.y-localPosition.y) <= sanityThrehold)
			{
				objPos.y = localPosition.y;
				modified = true;
			}

			if(objPos.z != localPosition.z && Mathf.Abs(objPos.z-localPosition.z) <= sanityThrehold)
			{
				objPos.z = localPosition.z;
				modified = true;
			}		
			
			if(modified)
			{
				obj.transform.position = objPos;
			}
		}
	}
	
	[System.Serializable]
	public class SceneSanityData 
	{
		public SceneSanityItem[] dataItems = new SceneSanityItem[0];
		
		public void ApplySanityChanges(float sanityThrehold)
		{
			foreach(var i in dataItems)
			{
				i.ApplySanityChanges(sanityThrehold);
			}
		}
		
		public static SceneSanityData Create()
		{
			var data = new SceneSanityData();
			var objects = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
			var items = new List<SceneSanityItem>();
			foreach(var i in objects)
			{
				items.Add(new SceneSanityItem(i));	
			}
			
			items.Sort((x, y) => x.instanceID.CompareTo(y.instanceID));
			data.dataItems = items.ToArray();
			return data;
		}
	}
	
	public static string[] OnWillSaveAssets(string[] paths)
	{
		/*
		// no point if not using pro
		if(!Application.HasProLicense())
			return paths;
		
		var scenePath = System.Array.Find(paths, (obj) => obj.EndsWith(".unity"));
		if(scenePath == null)
			return paths;
		

		var xml = new XmlSerializer(typeof(SceneSanityData));
		
		var sanityPath = Path.Combine(Path.GetDirectoryName(scenePath),Path.GetFileNameWithoutExtension(scenePath)+".sanity.xml");
		if(File.Exists(sanityPath))
		{
			var readFS = new FileStream(sanityPath, System.IO.FileMode.Open);
			var sanityData = xml.Deserialize(readFS) as SceneSanityData;
			if(sanityData != null)
			{
				sanityData.ApplySanityChanges(SANITY_THRESHOLD);
			}
			readFS.Close();
		}
		
		var writeFS = new FileStream(sanityPath, System.IO.FileMode.Create);
		var newSanityData = SceneSanityData.Create();
		xml.Serialize(writeFS, newSanityData);
		*/
		
		if(sanityData!=null)
		{
			sanityData.ApplySanityChanges(SANITY_THRESHOLD);
		}
		
		return paths;
	}
}
