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
		const string k_RemapFilePath = "Upgrade/AssetIdRemap.json";
		const string k_NamespaceRemapFilePath = "Upgrade/NamespaceRemap.json";

		static string remapFilePath
		{
			get { return pb_FileUtil.GetProBuilderInstallDirectory() + k_RemapFilePath; }
		}

		static string namespaceRemapFilePath
		{
			get { return pb_FileUtil.GetProBuilderInstallDirectory() + k_NamespaceRemapFilePath; }
		}

		static string[] k_DirectoryExcludeFilter = new string[]
		{
			"ProBuilder/About",
			"ProBuilder/API Examples",
			"ProBuilder/Data",
			"ProBuilder/Icons",
			"ProBuilder/Material",
			"ProBuilder/Upgrade",
		};

		static TextAsset m_RemapObject = null;
		static TextAsset m_NamespaceRemap = null;

		[MenuItem("Tools/ProBuilder/Debug/GUID Remap Editor")]
		static void MenuOpenGuidEditor()
		{
			m_RemapObject = EditorGUILayout.ObjectField("Remap", m_RemapObject, typeof(TextAsset), false);
			m_NamespaceRemap = EditorGUILayout.ObjectField("Namespace", m_NamespaceRemap, typeof(TextAsset), false);

			
		}


		[MenuItem("Assets/GUID Remap Utility/Collect Old GUIDs")]
		static void GetRemapSource()
		{
			pb_GuidRemapObject remapObject = GetGuidRemapObject();
			GetRemapSource(remapObject);
		}

		[MenuItem("Assets/GUID Remap Utility/Collect New GUIDs")]
		static void MenuGetRemapDestination()
		{
			pb_GuidRemapObject remapObject = GetGuidRemapObject();
			GetRemapDestination(remapObject);
		}

		static void GetRemapSource(pb_GuidRemapObject remapObject)
		{
			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			remapObject.sourceDirectory.Add(localDirectory);
			List<pb_AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.source.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				// the only time where a destination can exist with a null source is when a single destination is in the
				// map, so it's okay to grab the first and not bother searching for more dangling destination entries
				pb_AssetIdentifierTuple matchingDestination =
					map.FirstOrDefault(x =>
					{
						return x.destination != null &&
						       x.destination.AssetEquals(id, remapObject.namespaceMap);
					});

				if (matchingDestination != null)
				{
					if (matchingDestination.source != null)
						map.Add(new pb_AssetIdentifierTuple(id, matchingDestination.destination));
					else
						matchingDestination.source = id;
				}
				else
				{
					map.Add(new pb_AssetIdentifierTuple(id, null));
				}
			}

			pb_FileUtil.WriteAllText(remapFilePath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(remapFilePath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(remapFilePath));
		}

		static void GetRemapDestination(pb_GuidRemapObject remapObject)
		{
			if (!string.IsNullOrEmpty(remapObject.destinationDirectory))
			{
				if (!EditorUtility.DisplayDialog("Destination Directory Already Mapped",
					"The destination directory has already been mapped. Continuing will overwrite the existing data. Are you sure you wish to continue?",
					"Continue", "Cancel"))
					return;
			}

			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			remapObject.destinationDirectory = localDirectory;
			List<pb_AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.destination.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				IEnumerable<pb_AssetIdentifierTuple> matchingSources =
					map.Where(x => x.source != null && x.source.AssetEquals(id, remapObject.namespaceMap));

				if (matchingSources.Any())
				{
					foreach (var tup in matchingSources)
						tup.destination = id;
				}
				else
				{
					map.Add(new pb_AssetIdentifierTuple(null, id));
				}
			}

			pb_FileUtil.WriteAllText(remapFilePath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(remapFilePath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(remapFilePath));
		}

		/// <summary>
		/// Collect asset identifier information from all files in a directory.
		/// </summary>
		/// <param name="directory"></param>
		static List<pb_AssetIdentifier> GetAssetIdentifiersInDirectory(string directory, string[] directoryIgnoreFilter = null)
		{
			List<pb_AssetIdentifier> ids = new List<pb_AssetIdentifier>();

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

		static List<pb_AssetIdentifier> GetAssetIdentifiers(string assetPath)
		{
			List<pb_AssetIdentifier> ids = new List<pb_AssetIdentifier>();

			if (assetPath.EndsWith(".unity"))
				return ids;

			foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
			{
				GUID g;
				long file;

				if (AssetDatabase.GetGUIDAndLocalIdentifierInFile(o.GetInstanceID(), out g, out file))
					ids.Add(new pb_AssetIdentifier(o, file.ToString(), g.ToString(), assetPath));
			}

			return ids;
		}

		/// <summary>
		/// Load a remap json file from a relative path (Assets/MyRemapFile.json).
		/// </summary>
		/// <returns>A GuidRemapObject from the path, or if not found, a new GuidRemapObject</returns>
		static pb_GuidRemapObject GetGuidRemapObject()
		{
			pb_GuidRemapObject remap = new pb_GuidRemapObject();

			TextAsset o = AssetDatabase.LoadAssetAtPath<TextAsset>(remapFilePath);

			if (o != null)
			{
				JsonUtility.FromJsonOverwrite(o.text, remap);
			}
			else
			{
				TextAsset namespaceRemapJson = AssetDatabase.LoadAssetAtPath<TextAsset>(namespaceRemapFilePath);

				if (namespaceRemapJson != null)
				{
					remap.namespaceMap = JsonUtility.FromJson<pb_NamespaceRemapObject>(namespaceRemapJson.text);
				}
			}

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
