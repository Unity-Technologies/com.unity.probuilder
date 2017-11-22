using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UObject = UnityEngine.Object;

namespace UnityEditor.GuidRemap
{
	[Serializable]
	class AssetIdentifier : IEquatable<AssetIdentifier>
	{
		/// <summary>
		/// A path relative to the root asset directory (ex, ProBuilder/About/Hello.cs).
		/// Stored per-asset because the path may change between upgrades. A single file name is stored at the tuple
		/// level.
		/// </summary>
		public string localPath;

		/// <summary>
		/// The asset fileId.
		/// </summary>
		public string fileId;

		/// <summary>
		/// Asset GUID.
		/// </summary>
		public string guid {
			get { return m_Guid; }
		}

		[SerializeField]
		string m_Guid;

		public AssetIdentifier(string guid)
		{
			Assert.IsFalse(string.IsNullOrEmpty(guid), "Cannot initialize an AssetIdentifier without a GUID");
			m_Guid = guid;
		}

		/// <summary>
		/// Populate any vacant fields on this object with other.
		/// </summary>
		/// <param name="other"></param>
		public void UnionWith(AssetIdentifier other)
		{
			if (string.IsNullOrEmpty(localPath))
				localPath = other.localPath;

			if (string.IsNullOrEmpty(fileId))
				fileId = other.fileId;

			// JsonUtility doesn't serialize null values, it serializes an empty AssetIdentifier
			if (string.IsNullOrEmpty(m_Guid))
				m_Guid = other.guid;
		}

		public bool Equals(AssetIdentifier other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(m_Guid, other.m_Guid);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((AssetIdentifier) obj);
		}

		public override int GetHashCode()
		{
			return (guid != null ? guid.GetHashCode() : 0);
		}
	}

	[Serializable]
	class AssetIdentifierTuple
	{
		public AssetIdentifier source;
		public AssetIdentifier destination;

		public AssetIdentifierTuple()
		{
			source = null;
			destination = null;
		}

		public AssetIdentifierTuple(AssetIdentifier src, AssetIdentifier dest)
		{
			source = src;
			destination = dest;
		}
	}

	[Serializable]
	class GuidRemapObject
	{
		public string directory;
		public List<AssetIdentifierTuple> map = new List<AssetIdentifierTuple>();
	}

	public class RemapGuids : Editor
	{
		const string k_RemapFilePath = "Assets/remap.json";
		const bool k_CollectFolderIds = false;

		[MenuItem("Assets/Print GUID &d")]
		static void ShowGuid()
		{
			Debug.Log(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
		}

		[MenuItem("Assets/GUID Remap Utility/Collect Old GUIDs")]
		static void GetRemapSource()
		{
			GuidRemapObject remapObject = GetGuidRemapObject(k_RemapFilePath);
			remapObject.directory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets");
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiers(GetSelectedDirectory()))
			{
				AssetIdentifierTuple existing = map.FirstOrDefault(x =>
					(x.source != null && x.source.Equals(id)) ||
					(x.destination != null && x.destination.localPath.Equals(id.localPath)));

				if (existing == null)
				{
					map.Add(new AssetIdentifierTuple(id, null));
				}
				else
				{
					if (existing.source == null)
						existing.source = id;
					else
						existing.source.UnionWith(id);
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
			remapObject.directory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets");
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiers(GetSelectedDirectory()))
			{
				AssetIdentifierTuple existing = map.FirstOrDefault(x =>
					(x.destination != null && x.destination.Equals(id)) ||
					(x.source != null && x.source.localPath.Equals(id.localPath)));

				if (existing == null)
				{
					map.Add(new AssetIdentifierTuple(null, id));
				}
				else
				{
					if (existing.destination == null)
						existing.destination = id;
					else
						existing.destination.UnionWith(id);
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
		static IEnumerable<AssetIdentifier> GetAssetIdentifiers(string directory)
		{
			string[] extensionsToScanForGuidRemap = new string[]
			{
				"*.meta",
				"*.asset",
				"*.mat",
				"*.unity",
			};

			string[] directoryExcludeFilter = new string[]
			{
				"ProBuilder/About",
				"ProBuilder/Icons"
			};

			string fullRootDirectory = Path.GetFullPath(directory).Replace("\\", "/") + "/";

			Dictionary<string, AssetIdentifier> identifiers = new Dictionary<string, AssetIdentifier>();

			foreach (string extension in extensionsToScanForGuidRemap)
			{
				foreach (string filePath in Directory.GetFiles(directory, extension, SearchOption.AllDirectories))
				{
					if (directoryExcludeFilter.Any(x => filePath.Contains(x)))
						continue;

					if (filePath.EndsWith(".meta"))
					{
						if (!k_CollectFolderIds && Directory.Exists(filePath.Replace(".meta", "")))
							continue;

						AssetIdentifier id = GetAssetIdentifierFromMetaFile(fullRootDirectory, filePath);
						AssetIdentifier existing;

						if (identifiers.TryGetValue(id.guid, out existing))
							existing.UnionWith(id);
						else
							identifiers.Add(id.guid, id);
					}
					else
					{
						GetFileIdsFromAsset(filePath, identifiers);
					}
				}
			}

			return identifiers.Values;
		}

		static AssetIdentifier GetAssetIdentifierFromMetaFile(string rootDirectory, string filePath)
		{
			using (var sr = new StreamReader(filePath))
			{
				while (sr.Peek() > -1)
				{
					string line = sr.ReadLine();

					if (line.StartsWith("guid:"))
					{
						AssetIdentifier id = new AssetIdentifier(line.Replace("guid:", "").Trim())
						{
							localPath = filePath.Replace("\\", "/").Replace(rootDirectory, "").Replace(".meta", "")
						};

						return id;
					}
				}
			}

			return null;
		}

		static Regex m_FileAndGuidYamlExpr = new Regex("{fileID: [0-9]*, guid: [a-zA-Z0-9]*, type: [0-9]*}", RegexOptions.Compiled | RegexOptions.Multiline);
		static Regex m_FileIdRegex = new Regex("(?<=fileID: )[0-9]*", RegexOptions.Compiled);
		static Regex m_GuidRegex = new Regex("(?<=fileID: [0-9]*, guid: )[0-9a-zA-Z]*", RegexOptions.Compiled);

		static void GetFileIdsFromAsset(string assetPath, Dictionary<string, AssetIdentifier> files)
		{
			try
			{
				string contents = File.ReadAllText(assetPath);

				foreach (Match match in m_FileAndGuidYamlExpr.Matches(contents))
				{
					Match file = m_FileIdRegex.Match(match.Value);
					Match guid = m_GuidRegex.Match(match.Value);

					if (file.Success && guid.Success)
					{
						AssetIdentifier id;

						if (files.TryGetValue(guid.Value, out id))
							id.fileId = file.Value;
						else
							files.Add(guid.Value, new AssetIdentifier(guid.Value) { fileId = file.Value });
					}
				}
			}
			catch (SystemException exception)
			{
				Debug.LogError("Failed parsing asset: " + assetPath);
			}
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