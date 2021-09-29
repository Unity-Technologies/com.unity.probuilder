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
                if(m_Version != m_CompileVersion)
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
        public IList<Vector2> textures0 { get => m_Textures0; set { m_Textures0 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures1 { get => m_Textures1; set { m_Textures1 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures2 { get => m_Textures2; set { m_Textures2 = value?.ToArray(); SetDirty(); } }
        public IList<Vector2> textures3 { get => m_Textures3; set { m_Textures3 = value?.ToArray(); SetDirty(); } }
        public IList<Color> colors { get => m_Colors; set { m_Colors = value?.ToArray(); SetDirty(); } }
        public IList<Face> faces { get => m_Faces; set { m_Faces = value?.ToArray(); SetDirty(); } }

        // todo generated attributes shouldn't be settable
        public IList<Vector3> normals { get => m_Normals; set { m_Normals = value?.ToArray(); SetDirty(); } }
        public IList<Vector4> tangents { get => m_Tangents; set { m_Tangents = value?.ToArray(); SetDirty(); } }

        public void Compile()
        {
            if (m_UnityMesh == null)
            {
                m_UnityMesh = new Mesh();
                m_UnityMesh.name = name;
#if UNITY_EDITOR
                if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                    AssetDatabase.AddObjectToAsset(m_UnityMesh, this);
#endif
            }

            m_CompileVersion = m_Version;
            PMeshCompiler.Compile(this, m_UnityMesh);
            meshWasCompiled?.Invoke(this);
        }
    }
}