#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Utility class for creating GUID remap files.
	/// </summary>
	class pb_RemapGuidsEditor : Editor
	{
		const string k_RemapFilePath = "Assets/remap.json";

		static string[] k_DirectoryExcludeFilter = new string[]
		{
			"ProBuilder/About",
			"ProBuilder/API Examples",
			"ProBuilder/Data",
			"ProBuilder/Icons",
			"ProBuilder/Material"
		};

		[MenuItem("Assets/GUID Remap Utility/Collect Old GUIDs")]
		static void GetRemapSource()
		{
			GuidRemapObject remapObject = GetGuidRemapObject(k_RemapFilePath);
			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			remapObject.sourceDirectory.Add(localDirectory);
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.source.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				// the only time where a destination can exist with a null source is when a single destination is in the
				// map, so it's okay to grab the first and not bother searching for more dangling destination entries
				AssetIdentifierTuple matchingDestination =
					map.FirstOrDefault(x =>
					{
						return x.destination != null &&
						       x.destination.localPath.Equals(id.localPath);
					});

				if (matchingDestination != null)
				{
					if (matchingDestination.source != null)
						map.Add(new AssetIdentifierTuple(id, matchingDestination.destination));
					else
						matchingDestination.source = id;
				}
				else
				{
					map.Add(new AssetIdentifierTuple(id, null));
				}
			}

			File.WriteAllText(k_RemapFilePath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(k_RemapFilePath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(k_RemapFilePath));
		}

		[MenuItem("Assets/GUID Remap Utility/Collect New GUIDs")]
		static void GetRemapDestination()
		{
			GuidRemapObject remapObject = GetGuidRemapObject(k_RemapFilePath);

			if (!string.IsNullOrEmpty(remapObject.destinationDirectory))
			{
				if (!EditorUtility.DisplayDialog("Destination Directory Already Mapped",
					"The destination directory has already been mapped. Continuing will overwrite the existing data. Are you sure you wish to continue?",
					"Continue", "Cancel"))
					return;
			}

			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			remapObject.destinationDirectory = localDirectory;
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.destination.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				IEnumerable<AssetIdentifierTuple> matchingSources =
					map.Where(x => x.source != null && x.source.localPath.Equals(id.localPath));

				if (matchingSources.Any())
				{
					foreach (var tup in matchingSources)
						tup.destination = id;
				}
				else
				{
					map.Add(new AssetIdentifierTuple(null, id));
				}
			}

			File.WriteAllText(k_RemapFilePath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(k_RemapFilePath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(k_RemapFilePath));
		}

		/// <summary>
		/// Collect asset identifier information from all files in a directory.
		/// </summary>
		/// <param name="directory"></param>
		static List<AssetIdentifier> GetAssetIdentifiersInDirectory(string directory, string[] directoryIgnoreFilter = null)
		{
			List<AssetIdentifier> ids = new List<AssetIdentifier>();

			string unixPath = directory.Replace("\\", "/");

			if (directoryIgnoreFilter != null && directoryIgnoreFilter.Any(x => unixPath.Contains(x)))
				return ids;

			foreach (string file in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
			{
				if (file.EndsWith(".meta"))
					continue;

				string localPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");
				ids.AddRange(GetAssetIdentifiers(localPath));
			}

			foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
				ids.AddRange(GetAssetIdentifiersInDirectory(dir, directoryIgnoreFilter));

			return ids;
		}

		static List<AssetIdentifier> GetAssetIdentifiers(string assetPath)
		{
			List<AssetIdentifier> ids = new List<AssetIdentifier>();

			if (assetPath.EndsWith(".unity"))
				return ids;

			foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
			{
				GUID g;
				long file;

				if (AssetDatabase.GetGUIDAndLocalIdentifierInFile(o.GetInstanceID(), out g, out file))
					ids.Add(new AssetIdentifier(o, file.ToString(), g.ToString(), assetPath));
			}

			return ids;
		}

		/// <summary>
		/// Load a remap json file from a relative path (Assets/MyRemapFile.json).
		/// </summary>
		/// <param name="path">Path relative to the project.</param>
		/// <returns>A GuidRemapObject from the path, or if not found, a new GuidRemapObject</returns>
		static GuidRemapObject GetGuidRemapObject(string path)
		{
			GuidRemapObject remap = new GuidRemapObject();

			try
			{
				TextAsset o = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				JsonUtility.FromJsonOverwrite(o.text, remap);
			}
			catch
			{}

			return remap;
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

#endif