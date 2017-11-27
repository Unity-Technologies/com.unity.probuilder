using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProBuilder.Core;
using UnityEditor;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	[Serializable]
	class AssetIdentifier : IEquatable<AssetIdentifier>
	{
		/// <summary>
		/// A path relative to the root asset directory (ex, ProBuilder/About/Hello.cs).
		/// Stored per-asset because the path may change between upgrades. A single file name is stored at the tuple
		/// level.
		/// </summary>
		public string localPath { get { return m_LocalPath; } }

		public string name { get { return m_Name; } }

		public string type { get { return m_Type; } }

		/// <summary>
		/// File Ids associated with this asset.
		/// </summary>
		public string fileId { get { return m_FileId;  } }

		/// <summary>
		/// Asset GUID.
		/// </summary>
		public string guid {
			get { return m_Guid; }
		}

		[SerializeField]
		string m_Guid;

		[SerializeField]
		string m_FileId;

		[SerializeField]
		string m_LocalPath;

		[SerializeField]
		string m_Name;

		[SerializeField]
		string m_Type;

		public AssetIdentifier(UObject obj, string file, string guid, string localPath = null)
		{
			if(obj == null)
				throw new SystemException("Cannot initialize an AssetIdentifier with a null object");

			if(string.IsNullOrEmpty(guid))
				throw new SystemException("Cannot initialize an AssetIdentifier without a GUID");

			if(string.IsNullOrEmpty(file))
				throw new SystemException("Cannot initialize an AssetIdentifier without a FileId");

			m_FileId = file;
			m_Guid = guid;
			m_Name = obj.name;
			m_LocalPath = localPath;
			m_Type = obj.GetType().ToString();
		}

		public bool Equals(AssetIdentifier other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return string.Equals(m_Guid, other.m_Guid) &&
			       string.Equals(m_FileId, other.m_FileId) &&
			       string.Equals(m_LocalPath, other.m_LocalPath);
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
			int hash = 0;

			unchecked
			{
				hash = (hash * 7) + (string.IsNullOrEmpty(m_Guid) ? 0 : m_Guid.GetHashCode());
				hash = (hash * 7) + (string.IsNullOrEmpty(m_FileId) ? 0 : m_FileId.GetHashCode());
				hash = (hash * 7) + (string.IsNullOrEmpty(m_LocalPath) ? 0 : m_LocalPath.GetHashCode());
			}

			return hash;
		}

		public void SetPathRelativeTo(string dir)
		{
			m_LocalPath = m_LocalPath.Replace(dir, "");
		}

		public static bool IsValid(AssetIdentifier id)
		{
			return !string.IsNullOrEmpty(id == null ? null : id.m_Guid);
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
		public List<string> sourceDirectory = new List<string>();
		public string destinationDirectory = null;
		public List<AssetIdentifierTuple> map = new List<AssetIdentifierTuple>();
	}

	class pb_RemapGuids : EditorWindow
	{
		TextAsset m_RemapFile = null;

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Convert to Package Manager")]
		static void MenuInitRemapGuidEditor()
		{
			GetWindow<pb_RemapGuids>(true, "Package Manager Conversion Utility", true);
		}

		void OnGUI()
		{
			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			SerializationMode serializationMode = EditorSettings.serializationMode;

			GUI.enabled = serializationMode == SerializationMode.ForceText;

			if (GUILayout.Button("Convert to Package Manager"))
				DoIt();

			if (GUILayout.Button("Show me guids and fileids"))
			{
				GUID guid = new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));

				ObjectIdentifier[] ids =
					BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(guid, EditorUserBuildSettings.activeBuildTarget);

				StringBuilder sb = new StringBuilder();

				sb.AppendLine(Selection.activeObject.name);
				sb.AppendLine(Selection.activeObject.GetInstanceID().ToString());
				sb.AppendLine(AssetDatabase.GetAssetPath(Selection.activeObject));
				sb.AppendLine(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
				sb.AppendLine("----");
				sb.AppendLine("GetGUIDAndLocalIdentifierInFile");
				foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)))
				{
					GUID g;
					long file;
					if (AssetDatabase.GetGUIDAndLocalIdentifierInFile(o.GetInstanceID(), out g, out file))
						sb.AppendLine("  " + o.name + "\n    " + o.GetInstanceID() + "\n    " + g + "\n    " + file);
				}

				sb.AppendLine("----");
				sb.AppendLine("LookupInstanceIDFromPathAndFileID");

				foreach (var id in ids)
				{
					int inst = AssetDatabase.LookupInstanceIDFromPathAndFileID(AssetDatabase.GetAssetPath(Selection.activeObject),
						(int) id.localIdentifierInFile);

					UnityEngine.Object o = EditorUtility.InstanceIDToObject(inst);

					sb.AppendLine("  " + (o != null ? o.name : "null") + "\n    " + inst + "\n    " + id.guid + "\n    " + id.localIdentifierInFile);
				}
				Debug.Log(sb.ToString());
			}

			GUI.enabled = true;
		}

		void DoIt()
		{
			GuidRemapObject remapObject = new GuidRemapObject();
			JsonUtility.FromJsonOverwrite(m_RemapFile.text, remapObject);

			string[] extensionsToScanForGuidRemap = new string[]
			{
				"*.meta",
				"*.asset",
				"*.mat",
				"*.unity",
			};

			string assetStoreProBuilderDirectory = pb_FileUtil.FindAssetStoreProBuilderInstall();

			if (string.IsNullOrEmpty(assetStoreProBuilderDirectory))
			{
				// todo pop up modal dialog asking user to point to ProBuilder directory (and validate before proceeding)
				Debug.LogWarning("Couldn't find an Asset Store install of ProBuilder. Aborting conversion process.");
				return;
			}

			foreach (string extension in extensionsToScanForGuidRemap)
			{
				foreach (string str in Directory.GetFiles("Assets", extension, SearchOption.AllDirectories))
					DoAssetIdentifierRemap(str, remapObject.map);
			}
		}

		static void DoAssetIdentifierRemap(string path, IEnumerable<AssetIdentifierTuple> map)
		{
			var sr = new StreamReader(path);
			var sw = new StreamWriter(path + ".remap", false);

			System.Collections.Generic.List<pb_Tuple<string, string>> replace = new System.Collections.Generic.List<pb_Tuple<string, string>>();

			// order is important - {fileId, guid} in asset files needs to be applied first
			IEnumerable<AssetIdentifierTuple> assetIdentifierTuples = map as AssetIdentifierTuple[] ?? map.ToArray();

			foreach (var kvp in assetIdentifierTuples)
			{
				replace.Add(new pb_Tuple<string, string>(
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.source.fileId, kvp.source.guid),
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.destination.fileId, kvp.destination.guid)));
			}

			HashSet<string> used = new HashSet<string>();

			foreach (var kvp in assetIdentifierTuples)
			{
				// AssetIdentifier list will contain duplicate guids (assets can contain sub-assets, separated by fileId)
				// when swapping meta file guids we don't need multiple entries
				if(used.Add(kvp.source.guid))
					replace.Add(new pb_Tuple<string, string>(
						string.Format("guid: {0}", kvp.source.guid),
						string.Format("guid: {0}", kvp.destination.guid)));
			}

			int modified = 0;

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();

				foreach (var kvp in replace)
				{
					if (line.Contains(kvp.Item1))
					{
						modified++;
						line = line.Replace(kvp.Item1, kvp.Item2);
						break;
					}
				}

				sw.WriteLine(line);
			}

			sr.Close();
			sw.Close();

			if (modified > 0)
			{
				File.Delete(path);
				File.Move(path + ".remap", path);
			}
		}
	}
}