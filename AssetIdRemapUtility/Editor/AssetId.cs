using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
    [Serializable]
    sealed class AssetIdentifierTuple
    {
        public AssetId source;
        public AssetId destination;

        public AssetIdentifierTuple()
        {
            source = null;
            destination = null;
        }

        public AssetIdentifierTuple(AssetId src, AssetId dest)
        {
            source = src ?? new AssetId();
            destination = dest ?? new AssetId();
        }

        public bool AssetEquals(AssetIdentifierTuple other)
        {
            return AssetId.IsValid(source) == AssetId.IsValid(other.source) &&
                source.AssetEquals(other.source) &&
                AssetId.IsValid(destination) == AssetId.IsValid(other.destination) &&
                destination.AssetEquals(other.destination);
        }
    }

    [Serializable]
    class StringTuple
    {
        public string key;
        public string value;

        public StringTuple(string k, string v)
        {
            key = k;
            value = v;
        }
    }

    enum Origin
    {
        Source,
        Destination
    }

    [Serializable]
    class AssetIdRemapObject
    {
        public List<string> sourceDirectory = new List<string>();
        public string destinationDirectory = null;
        public List<AssetIdentifierTuple> map = new List<AssetIdentifierTuple>();

        public AssetIdentifierTuple this[int i]
        {
            get { return map[i]; }
            set { map[i] = value; }
        }

        public void Clear(Origin origin)
        {
            switch (origin)
            {
                case Origin.Source:
                    sourceDirectory.Clear();
                    for (int i = 0, c = map.Count; i < c; i++)
                        map[i].source.Clear();
                    break;

                case Origin.Destination:
                    destinationDirectory = "";
                    for (int i = 0, c = map.Count; i < c; i++)
                        map[i].destination.Clear();
                    break;
            }

            map = map.Where(x => AssetId.IsValid(x.source) || AssetId.IsValid(x.destination)).ToList();
        }

        public void Delete(IEnumerable<AssetIdentifierTuple> entries)
        {
            map.RemoveAll(entries.Contains);
        }

        public void Merge(IEnumerable<AssetIdentifierTuple> entries)
        {
            var types = entries.SelectMany(x => new[] { x.source.assetType, x.destination.assetType });

            if (types.Where(x => !string.IsNullOrEmpty(x)).Distinct().Count() > 1)
            {
                Debug.LogError("Attempting to map entries of multiple types! This is not allowed.");
                return;
            }

            var arr = entries as AssetIdentifierTuple[] ?? entries.ToArray();
            var src = arr.Where(x => AssetId.IsValid(x.source) && !AssetId.IsValid(x.destination)).ToArray();
            var dst = arr.Where(x => AssetId.IsValid(x.destination)).ToArray();

            if (dst.Length != 1)
            {
                Debug.LogError("Merging AssetId entries requires only one valid destination entry be selected.");
                return;
            }

            var d = dst.First().destination;

            foreach (var s in src)
                map.Add(new AssetIdentifierTuple(new AssetId(s.source), new AssetId(d)));

            map.RemoveAll(src.Contains);
            map.RemoveAll(x => dst.Contains(x) && !AssetId.IsValid(x.source));
        }
    }

    [Serializable]
    class AssetId : IEquatable<AssetId>
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
        #pragma warning disable 414
        bool m_IsEditorScript = false;
        #pragma warning restore 414

        public AssetId()
        {
            Clear();
        }

        public AssetId(AssetId other)
        {
            m_Guid = other.m_Guid;
            m_FileId = other.m_FileId;
            m_LocalPath = other.m_LocalPath;
            m_Name = other.m_Name;
            m_Type = other.m_Type;
        }

        public AssetId(UObject obj, string file, string guid, string localPath = null)
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
            m_Type = GetUObjectTypeString(obj);
        }

        static string GetUObjectTypeString(UObject obj)
        {
            MonoScript ms = obj as MonoScript;

            if (ms != null)
                return string.Format("{0}{1}{2}", obj.GetType().ToString(), k_MonoScriptTypeSplit[0], ms.GetClass());

            return obj.GetType().ToString();
        }

        public bool Equals(AssetId other)
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
            var id = obj as AssetId;
            if (id != null) return Equals(id);
            return false;
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

        public void Clear()
        {
            m_Guid = "";
            m_FileId = "";
            m_LocalPath = "";
            m_Name = "";
            m_Type = "";
            m_InternalType = AssetType.Unknown;
            m_MonoScriptClass = null;
            m_IsEditorScript = false;
        }

        public void SetPathRelativeTo(string dir)
        {
            m_LocalPath = m_LocalPath.Replace(dir, "");
        }

        public static bool IsValid(AssetId id)
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
                        if (m_LocalPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                            m_IsEditorScript = m_LocalPath.StartsWith("Editor/") || m_LocalPath.Contains("/Editor/") || PackageImporter.IsEditorPluginEnabled(guid);
                        else
                            m_IsEditorScript = m_LocalPath.StartsWith("Editor/") || m_LocalPath.Contains("/Editor/");
                    }
                    catch
                    {
                        m_MonoScriptClass = "null";
//                      pb_Log.Debug("Failed parsing type from monoscript \"" + m_Name + "\" (" + m_Type + ")");
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
            typeString = classType.Substring(last + 1, (classType.Length - last) - 1);

            return true;
        }

        public bool AssetEquals(AssetId other)
        {
            if (!assetType.Equals(other.assetType))
                return false;

            return localPath.Equals(other.localPath);
        }

        /// <summary>
        /// Does the object this id reference exist in the project?
        /// </summary>
        /// <returns></returns>
        public bool ExistsInProject()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(m_Guid);
            if (string.IsNullOrEmpty(assetPath))
                return false;
            var assetObj = AssetDatabase.LoadAssetAtPath<UObject>(assetPath);
            if (assetObj == null)
                return false;
            return m_Type.Equals(GetUObjectTypeString(assetObj));
        }
    }
}
