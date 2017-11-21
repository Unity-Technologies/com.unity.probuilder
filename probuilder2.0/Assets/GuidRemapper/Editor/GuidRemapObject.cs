using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProBuilder.EditorCore;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace UnityEditor.GuidRemap
{
	[Serializable]
	class GuidRemap
	{
		// File name
		public string file;

		// Existing GUIDs to be remapped to the new
		public List<string> from;

		// The destination GUID
		public string to;
	}

	[Serializable]
	class GuidRemapObject
	{
		public List<GuidRemap> map = new List<GuidRemap>();
	}

	public class RemapGuids : Editor
	{
		[MenuItem("Assets/Print GUID &d")]
		static void ShowGuid()
		{
			Debug.Log(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
		}

		static GuidRemapObject GetGuidObject(string path)
		{
			GuidRemapObject o = new GuidRemapObject();

			if (File.Exists(path))
			{
				string remapJson = File.ReadAllText(path);
				JsonUtility.FromJsonOverwrite(remapJson, o);
			}

			return o;
		}

		[MenuItem("Assets/GUID Remap Utility/Remap From")]
		static void PopulateFrom()
		{
			string path = "Assets/remap.json";
			string dir = GetSelectedDirectory();

			GuidRemapObject o = GetGuidObject(path);
			Dictionary<string, string> guids = new Dictionary<string, string>();
			ReadGuids(dir, ref guids);
			// rider complains about this statement, but it is valid
			HashSet<string> existing = new HashSet<string>(o.map.SelectMany(x => x.from));

			foreach (var kvp in guids)
				if (existing.Add(kvp.Value))
					o.map.Add(new GuidRemap() {file = kvp.Key, from = new List<string>() {kvp.Value}, to = ""});

			File.WriteAllText(path, JsonUtility.ToJson(o, true));
			AssetDatabase.Refresh();
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)));
		}

		[MenuItem("Assets/GUID Remap Utility/Remap To")]
		static void PopulateTo()
		{
			string path = "Assets/remap.json";
			string dir = GetSelectedDirectory();

			GuidRemapObject o = GetGuidObject(path);
			Dictionary<string, string> guids = new Dictionary<string, string>();
			ReadGuids(dir, ref guids);
			Dictionary<string, GuidRemap> nameMap = new Dictionary<string, GuidRemap>();

			// todo handle duplicate names
			foreach (GuidRemap g in o.map)
			{
				string fn = Path.GetFileName(g.file);
				if (!nameMap.ContainsKey(fn))
					nameMap.Add(fn, g);
			}

			foreach (var kvp in guids)
			{
				string fileName = Path.GetFileName(kvp.Key);
				GuidRemap mapping;

				if (nameMap.TryGetValue(fileName, out mapping))
					mapping.to = kvp.Value;
				else
					nameMap.Add(fileName, mapping = new GuidRemap() {file = kvp.Key, from = new List<string>(), to = kvp.Value});
			}

			File.WriteAllText(path, JsonUtility.ToJson(o, true));
			AssetDatabase.Refresh();
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)));
		}

		static void ReadGuids(string path, ref Dictionary<string, string> guids)
		{
			string[][] contents = new string[2][]
			{
				Directory.GetFiles(path, "*", SearchOption.AllDirectories),
				Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
			};

			foreach (string[] pattern in contents)
			{
				foreach (string str in pattern)
				{
					string relative = str.Replace("\\", "/").Replace(Application.dataPath, "Assets");

					if (!relative.EndsWith(".meta"))
					{
						string id = AssetDatabase.AssetPathToGUID(relative);

						if (!string.IsNullOrEmpty(id))
							guids.Add(relative, id);
					}
				}
			}
		}

		static string GetSelectedDirectory()
		{
			UObject o = Selection.activeObject;

			if (o != null)
			{
				string path = AssetDatabase.GetAssetPath(o.GetInstanceID());

				if (!string.IsNullOrEmpty(path))
				{
					if (Directory.Exists(path))
						return Path.GetFullPath(path);

					string res = Path.GetDirectoryName(path);

					if (!string.IsNullOrEmpty(res) && System.IO.Directory.Exists(res))
						return Path.GetFullPath(res);
				}
			}

			return Path.GetFullPath("Assets");
		}
	}
}