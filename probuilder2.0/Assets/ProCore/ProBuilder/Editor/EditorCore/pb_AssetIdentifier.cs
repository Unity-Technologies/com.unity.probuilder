using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	[Serializable]
	class pb_AssetIdentifierTuple
	{
		public pb_AssetIdentifier source;
		public pb_AssetIdentifier destination;

		public pb_AssetIdentifierTuple()
		{
			source = null;
			destination = null;
		}

		public pb_AssetIdentifierTuple(pb_AssetIdentifier src, pb_AssetIdentifier dest)
		{
			source = src;
			destination = dest;
		}
	}

	[Serializable]
	class pb_NamespaceRemapObject : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public Dictionary<string, string> map = new Dictionary<string, string>();

		public bool TryGetValue(string key, out string value)
		{
			return map.TryGetValue(key, out value);
		}

		[Serializable]
		class Tuple
		{
			public string key;
			public string value;

			public Tuple(string k, string v)
			{
				key = k;
				value = v;
			}
		}

		// serialize as key value pair to make json easier to read
		[SerializeField]
		Tuple[] m_Map;

		public void OnBeforeSerialize()
		{
			m_Map = map.Select(x => new Tuple(x.Key, x.Value)).ToArray();
		}

		public void OnAfterDeserialize()
		{
			for (int i = 0, c = m_Map.Length; i < c; i++)
				map.Add(m_Map[i].key, m_Map[i].value);
		}
	}

	[Serializable]
	class pb_GuidRemapObject
	{
		public List<string> sourceDirectory = new List<string>();
		public string destinationDirectory = null;
		public pb_NamespaceRemapObject namespaceMap = null;
		public List<pb_AssetIdentifierTuple> map = new List<pb_AssetIdentifierTuple>();
	}

	[Serializable]
	class pb_AssetIdentifier : IEquatable<pb_AssetIdentifier>
	{
		const string k_MonoScriptTypeString = "UnityEditor.MonoScript";

		static readonly string[] k_MonoScriptTypeSplit = new string[1] {"::"};

		enum AssetType
		{
			Unknown = 0,
			Default = 1,
			MonoScript = 2,
			// add more as special cases require
		}

		/// <summary>
		/// A path relative to the root asset directory (ex, ProBuilder/About/Hello.cs).
		/// Stored per-asset because the path may change between upgrades. A single file name is stored at the tuple
		/// level.
		/// </summary>
		public string localPath
		{
			get { return m_LocalPath; }
		}

		public string name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Return the backing type of this asset. If the asset is a MonoScript, the associated mono class will be
		/// returned. To get the Unity asset type use assetType.
		/// </summary>
		public string type
		{
			get { return IsMonoScript() ? m_MonoScriptClass : m_Type; }
		}

		public string assetType
		{
			get { return IsMonoScript() ? k_MonoScriptTypeString : m_Type; }
		}

		/// <summary>
		/// File Ids associated with this asset.
		/// </summary>
		public string fileId
		{
			get { return m_FileId; }
		}

		/// <summary>
		/// Asset GUID.
		/// </summary>
		public string guid
		{
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

		// the remaining properties are only relevant to monoscript files
		AssetType m_InternalType = AssetType.Unknown;
		string m_MonoScriptClass = null;
		bool m_IsEditorScript = false;

		public pb_AssetIdentifier(UObject obj, string file, string guid, string localPath = null)
		{
			if (obj == null)
				throw new SystemException("Cannot initialize an AssetIdentifier with a null object");

			if (string.IsNullOrEmpty(guid))
				throw new SystemException("Cannot initialize an AssetIdentifier without a GUID");

			if (string.IsNullOrEmpty(file))
				throw new SystemException("Cannot initialize an AssetIdentifier without a FileId");

			m_FileId = file;
			m_Guid = guid;
			m_Name = obj.name;
			m_LocalPath = localPath;
			MonoScript ms = obj as MonoScript;
			if (ms != null)
				m_Type = string.Format("{0}{1}{2}", obj.GetType().ToString(), k_MonoScriptTypeSplit[0], ms.GetClass());
			else
				m_Type = obj.GetType().ToString();
		}

		public bool Equals(pb_AssetIdentifier other)
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
			return Equals((pb_AssetIdentifier) obj);
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

		public static bool IsValid(pb_AssetIdentifier id)
		{
			return !string.IsNullOrEmpty(id == null ? null : id.m_Guid);
		}

		public bool IsMonoScript()
		{
			if (m_InternalType == AssetType.Unknown)
			{
				if (m_Type.StartsWith(k_MonoScriptTypeString))
				{
					m_InternalType = AssetType.MonoScript;

					try
					{
						m_MonoScriptClass = m_Type.Split(k_MonoScriptTypeSplit, StringSplitOptions.RemoveEmptyEntries)[1];
						m_IsEditorScript = m_LocalPath.StartsWith("Editor/") || m_LocalPath.Contains("/Editor/");
					}
					catch
					{
						m_MonoScriptClass = "null";
						pb_Log.Warning("Failed parsing type from monoscript \"" + m_Name + "\" (" + m_Type + ")");
					}
				}
				else
				{
					m_InternalType = AssetType.Default;
				}
			}

			return m_InternalType == AssetType.MonoScript;
		}

		bool GetNamespaceAndType(string classType, out string namespaceString, out string typeString)
		{
			namespaceString = null;
			typeString = null;

			if (string.IsNullOrEmpty(classType))
				return false;

			int last = classType.LastIndexOf('.');

			if (last < 0)
			{
				typeString = classType;
				return true;
			}

			namespaceString = classType.Substring(0, last);
			typeString = classType.Substring(last, classType.Length - last);

			return true;
		}

		public bool AssetEquals(pb_AssetIdentifier other, pb_NamespaceRemapObject namespaceRemap = null)
		{
			if (!assetType.Equals(other.assetType))
				return false;

			if (IsMonoScript())
			{
				// would be better to compare assemblies, but that's not possible when going from src to dll
				// however this at least catches the case where a type exists in both a runtime and Editor dll
				if (m_IsEditorScript == other.m_IsEditorScript)
				{
					// ideally we'd do a scan and find the closest match based on local path, but for now it's a
					// relatively controlled environment and we can deal with duplicate names on an as-needed basis

					// left namespace, left type, etc
					string ln, rn, lt, rt;

					if (GetNamespaceAndType(m_MonoScriptClass, out ln, out lt) &&
					    GetNamespaceAndType(other.m_MonoScriptClass, out rn, out rt))
					{
						if (!string.IsNullOrEmpty(ln))
						{
							// remapped left namespace
							string lrn;

							// if left namespace existed check for a remap, otherwise compare and return
							if (namespaceRemap.TryGetValue(ln, out lrn))
							{
								if (lrn.Equals(rn) && lt.Equals(rt))
									return true;
							}
							else
							{
								return ln.Equals(rn) && lt.Equals(rt);
							}
						}
						else
						{
							// left didn't have a namespace to begin with, so check against name only
							return lt.Equals(rt);
						}
					}
				}
			}
			else
			{
				return localPath.Equals(other.localPath);
			}

			return false;
		}
	}
}