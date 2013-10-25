using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.IO;
using System.Collections.Generic;

public class CustomAssetBuilder : AssetPostprocessor
{
	private static List<string> recentlyImportedTypeNames = new List<string>();
	
	static System.Type[] EnumerateAssetTypes()
	{
		var assembly = System.Reflection.Assembly.GetAssembly(typeof(CustomAsset));
		return (
			from t in assembly.GetTypes() 
			where t.IsSubclassOf(typeof(CustomAsset)) || t.GetCustomAttributes(typeof(CustomAssetAttribute),true).Length>0
			select t
			).Cast<System.Type>().ToArray();
	}
	
	static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
	{
		recentlyImportedTypeNames.AddRange(importedAssets);
		EditorApplication.update += ProcessTypes; 
	} 
	
	static void ProcessTypes ()
	{
		EditorApplication.update -= ProcessTypes;
		
		foreach(var path in recentlyImportedTypeNames)
		{
			if(path==null)
				continue;

			var extension = Path.GetExtension(path);
			if(extension != ".cs" && extension != ".js")
				continue;
			
			var name = Path.GetFileNameWithoutExtension(path);
			var type = typeof(CustomAsset).Assembly.GetType(name, false, true);
			if(type==null || !type.IsSubclassOf(typeof(ScriptableObject)))
				continue;
			
			Generate();
			break;
		}
		
		recentlyImportedTypeNames.Clear();
	}
	
	
	
	//[MenuItem("Test/Generate test assembly")]
	static void Generate() 
	{
        /*
        public sealed class AssetTypeBuilder
        {
            public static void Create()
            {
                ScriptableObjectUtils.CreateTheAsset<AssetType>();
            }
        }
        */
		
		string dirName = "Assets/Plugins/Editor";
		AssemblyName aName = new AssemblyName("ScriptableObjectBuilder");
		string dllName = aName.Name+".dll";
		
		// cleanup, remove existing dlls and ensure the output directory exists 
		System.IO.File.Delete("Assets/Plugins/Editor/"+dllName);
		System.IO.Directory.CreateDirectory(dirName);

        AssemblyBuilder ab = 
            System.AppDomain.CurrentDomain.DefineDynamicAssembly(
                aName, 
                AssemblyBuilderAccess.RunAndSave,
				dirName);
			
        // For a single-module assembly, the module name is usually 
        // the assembly name plus an extension.
        ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, dllName);
		
		foreach(var assetType in EnumerateAssetTypes())
		{
			string className = assetType.Name+"Builder";
			TypeBuilder tb = mb.DefineType(className, TypeAttributes.Public | TypeAttributes.Sealed);
			
			// Define a default constructor.
			// For parameter types, pass the empty 
			// array of types or pass null.
			ConstructorBuilder ctor0 = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, System.Type.EmptyTypes);
			ILGenerator ctor0IL = ctor0.GetILGenerator();
				ctor0IL.Emit(OpCodes.Ldarg_0);
				ctor0IL.Emit(OpCodes.Call, typeof(object).GetConstructor(System.Type.EmptyTypes));
				ctor0IL.Emit(OpCodes.Ret);
			
			// Define a method that accepts an integer argument and returns 
			// the product of that integer and the private field m_number. This 
			// time, the array of parameter types is created on the fly.
			MethodBuilder meth = tb.DefineMethod(
				"Create",
				MethodAttributes.Public | MethodAttributes.Static,
			  	typeof(void), 
			  	System.Type.EmptyTypes);
			
			var attribParams = new object[] {"Assets/Create/"+assetType.Name};
			var attribCtorInfo = typeof(MenuItem).GetConstructor(new System.Type[] {typeof(string)});
			var methAttrib = new CustomAttributeBuilder(attribCtorInfo, attribParams);
			meth.SetCustomAttribute(methAttrib);
			
			ILGenerator methIL = meth.GetILGenerator();
				methIL.Emit(OpCodes.Ldstr, "Creating scripted type: "+assetType.Name);
				methIL.Emit(OpCodes.Call, typeof(Debug).GetMethod("Log", new System.Type[] {typeof(string)}));
				methIL.Emit(OpCodes.Call, typeof(CustomAssetBuilder).GetMethod("CreateTheAsset").MakeGenericMethod(new System.Type[] {assetType}));
				methIL.Emit(OpCodes.Ret);		
			
			// Finish the type.
        	tb.CreateType();
		}
		
        // The following line saves the single-module assembly. This 
        // requires AssemblyBuilderAccess to include Save. You can now 
        // type "ildasm MyDynamicAsm.dll" at the command prompt, and 
        // examine the assembly. You can also write a program that has 
        // a reference to the assembly, and use the MyDynamicType type. 
        // 
        ab.Save(aName.Name + ".dll");
	}
		
	/*
	[MenuItem("Window/Asset Builder...")]
	public static void OpenAssetBuilder()
	{
		EditorWindow.GetWindow<CustomAssetBuilder>();
	}
	
	// Use this for initialization
	void Start () 
	{
		name = "Custom Asset Builder";
	}
	
	int current = -1;
	
	void OnGUI () 
	{
		var types = EnumerateAssetTypes();
		var typeNames = (from t in types select t.Name).Cast<string>().ToArray();

		current = EditorGUILayout.Popup(current, typeNames);
		if(current<0)
			return;
		
		foreach(var item in Selection.objects) 
		{
		    var selpath = AssetDatabase.GetAssetPath(item);
		    if (selpath == "")
			{
				// not an asset
				GUILayout.Label("Non-Asset: " + item.name);
		        continue;
			}
		
		    var dummypath = System.IO.Path.Combine(selpath, "fake.asset");
		    var assetpath = AssetDatabase.GenerateUniqueAssetPath(dummypath);
			
		    if (assetpath == "") 
			{
		        // couldn't generate a path, current asset must be a file
				GUILayout.Label("File: " + item.name);
		    }
		    else 
			{
		        GUILayout.Label("Directory: " + item.name);
    		}
		}
	
		var type = types[current];
		if(GUILayout.Button("Create"))
		{
			var result = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(result, "Assets/HiThere."+type.Name+".asset");
		}
	}
	*/
	
	
	public static void CreateTheAsset<T> () where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();
 
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") 
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") 
		{
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}
 
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New" + typeof(T).ToString() + "."+typeof(T).ToString()+".asset");
 
		AssetDatabase.CreateAsset (asset, assetPathAndName);
 
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}
}
