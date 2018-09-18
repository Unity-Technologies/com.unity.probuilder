using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.Assertions;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder.MeshOperations;

class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d", false, 1000)]
	static void MenuInit()
	{
		GetWindow<TempMenuItems>().Show();

		if(!Settings.ContainsKey<float>("test"))
			Settings.Set<float>("test", 100f);

		if(!Settings.ContainsKey<int>("test"))
			Settings.Set<int>("test", 100);

		if(!Settings.ContainsKey<Material>("default"))
			Settings.Set<Material>("default", BuiltinMaterials.defaultMaterial);

		Settings.Save();
	}

	void OnGUI()
	{
		var types = Settings.dictionary.dictionary;

		foreach (var kvp in types)
		{
			var dic = kvp.Value;

			GUILayout.Label(kvp.Key, EditorStyles.boldLabel);

			foreach (var entry in dic)
			{
				var value = entry.Value;

				value = EditorGUILayout.TextField(entry.Key, value);

				if (!value.Equals(entry.Value))
				{
					Settings.Set(kvp.Key, entry.Key, value);
					Settings.Save();
					EditorGUIUtility.ExitGUI();
				}
			}
		}
	}

	public static void SaveMeshTemplate(Mesh mesh)
	{
//		StackTrace trace = new StackTrace(1, true);
//		for (int i = 0; i < trace.FrameCount; i++)
//		{
//			StackFrame first = trace.GetFrame(i);
//			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
//		}
	}
}
