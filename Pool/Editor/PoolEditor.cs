using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
/*
[CustomEditor(typeof(Pool))]
public class PoolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Export Snapshot"))
        {
            Pool pool = target as Pool;
			
			using (CSVWriter writer = new CSVWriter("Assets/Config/pool.csv", "Prefab", "Count"))
			{
				foreach (PoolTable table in pool.Items)
				{
					string prefabPath = AssetDatabase.GetAssetPath(table.Prefab);
					writer.WriteString(prefabPath);
					writer.WriteInt (table.ActiveCount + table.FreeCount);
					writer.EndRow();
				}
			}
        }
        
        if (GUILayout.Button("Import Snapshot as Preallocations"))
        {
            Pool pool = target as Pool;
                                 
			List<Pool.Preallocation> preallocations = new List<Pool.Preallocation>();
			
            using (CSVReader reader = new CSVReader("Assets/Config/pool.csv"))
            {
                while (reader.NextRow())
                {
                    string prefabPath = reader.ReadString();
					GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					int count = reader.ReadInt();
					
					preallocations.Add(new Pool.Preallocation() { prefab = prefab, count = count });
                }
            }
            
            pool.preallocations = preallocations.ToArray();
            EditorUtility.SetDirty(pool);
        }
        
        
        GUILayout.EndHorizontal();
        
        base.OnInspectorGUI();
    }
}
*/