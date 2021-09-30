using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.ProBuilder
{
    public class PMesh : ScriptableObject
    {
        [Flags]
        enum DirtyFlags : UInt16
        {
            None = 0,
            Position = 1 << 0,
            Topology = 1 << 1,
            Texture0 = 1 << 2,
            Texture1 = 1 << 3,
            Texture2 = 1 << 4,
            Texture3 = 1 << 5,
            Normals = 1 << 6,
            Tangents = 1 << 7,
            Colors = 1 << 8,
            CompileRequired = Position | Topology,
            OptionalAttrib = 0xFF & ~CompileRequired,
            UploadRequired = 0xFF
        }

        [SerializeField]
        DirtyFlags m_DirtyFlags;
        
        // version is used to handle when external changes have been applied (ex, undo)
        [SerializeField]
        ushort m_Version;
        
        [SerializeField]
        Vector3[] m_Positions;

        [SerializeField]
        Vector2[] m_Textures0, m_Textures1, m_Textures2, m_Textures3;

        [SerializeField]
        Color[] m_Colors;

        [SerializeField]
        Face[] m_Faces;

        [SerializeField]
        Mesh m_UnityMesh;

        [NonSerialized]
        ushort m_CompileVersion;
        
        [NonSerialized]
        Vector3[] m_Normals;

        [NonSerialized]
        Vector4[] m_Tangents;

        public int vertexCount => m_Positions?.Length ?? 0;
        public int faceCount => m_Faces?.Length ?? 0;

        public ushort version => m_Version;
        // todo remove compiledVersion, it's only used for debugging in the inspector
        public ushort compiledVersion => m_CompileVersion;
        public static event Action<PMesh> meshWasModified;
        public static event Action<PMesh> meshWasCompiled;

        public PMesh()
        {
            m_CompileVersion = m_Version;
        }

        public Mesh unityMesh
        {
            get
            {
                Compile();
                return m_UnityMesh;
            }
        }

        new void SetDirty()
        {
            unchecked { m_Version++; }
            meshWasModified?.Invoke(this);
        }

        public IList<Vector3> positions { get => m_Positions; set { m_Positions = value?.ToArray(); SetDirty(); } }
        public IList<Face> faces { get => m_Faces; set { m_Faces = value?.ToArray(); SetDirty(); } }

        // todo optional attributes could be set without dirtying the mesh as long as the vertex count matches
        public IList<Vector2> textures0 { get => m_Textures0; set { m_Textures0 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures1 { get => m_Textures1; set { m_Textures1 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures2 { get => m_Textures2; set { m_Textures2 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures3 { get => m_Textures3; set { m_Textures3 = value?.ToArray(); SetDirty(); } }
        public IList<Color> colors { get => m_Colors; set { m_Colors = value?.ToArray(); SetDirty(); } }

        // todo generated attributes shouldn't be settable
        public IList<Vector3> normals { get => m_Normals; set { m_Normals = value?.ToArray(); SetDirty(); } }
        public IList<Vector4> tangents { get => m_Tangents; set { m_Tangents = value?.ToArray(); SetDirty(); } }

        public void Compile(bool forceRebuild = false)
        {
            if (m_UnityMesh == null)
            {
                m_UnityMesh = new Mesh();
                m_UnityMesh.name = name;
                unchecked { m_Version = (ushort)(m_CompileVersion + 1); }
#if UNITY_EDITOR
                if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                    AssetDatabase.AddObjectToAsset(m_UnityMesh, this);
#endif
            }

            if (m_Version == m_CompileVersion && !forceRebuild)
                return;
            
            // todo debug info
            // if(m_Version == m_CompileVersion)
            Debug.LogWarning($"Compile {m_Version} != {m_CompileVersion}");

            m_CompileVersion = m_Version;
            PMeshCompiler.Compile(this, m_UnityMesh);
            meshWasCompiled?.Invoke(this);
            
            AssetUtility.SetDirty(this);
            AssetUtility.SetDirty(m_UnityMesh);
        }

        public void Upload()
        {
            // todo debug info
            Debug.Log("Upload");

            if (m_UnityMesh == null)
                return;
            
            if(m_Normals != null && m_Normals.Length == vertexCount)
                m_UnityMesh.SetNormals(m_Normals);
            if(m_Colors != null && m_Colors.Length == vertexCount)
                m_UnityMesh.SetColors(m_Colors);
            if(m_Textures0 != null && m_Textures0.Length == vertexCount)
                m_UnityMesh.SetUVs(0, m_Textures0);
            if(m_Textures1 != null && m_Textures1.Length == vertexCount)
                m_UnityMesh.SetUVs(1, m_Textures1);
            if(m_Textures2 != null && m_Textures2.Length == vertexCount)
                m_UnityMesh.SetUVs(2, m_Textures2);
            if(m_Textures3 != null && m_Textures3.Length == vertexCount)
                m_UnityMesh.SetUVs(3, m_Textures3);
            if(m_Tangents != null && m_Tangents.Length == vertexCount)
                m_UnityMesh.SetTangents(m_Tangents); 
            
            AssetUtility.SetDirty(this);
            AssetUtility.SetDirty(m_UnityMesh);
        }
    }
}