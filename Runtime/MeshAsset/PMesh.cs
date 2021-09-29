using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    public class PMesh : ScriptableObject
    {
        [SerializeField]
        ushort m_Version;
        
        [NonSerialized]
        ushort m_CompileVersion;

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

        public int vertexCount => m_Positions.Length;

        public ushort version => m_Version;
        // todo remove
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
                    Upload();
                return m_UnityMesh;
            }
        }

        new void SetDirty()
        {
            unchecked { m_Version++; }
            meshWasModified?.Invoke(this);
        }

        public IList<Vector3> positions
        {
            get => m_Positions;
            set { m_Positions = value.ToArray(); SetDirty(); }
        }

        public IList<Face> faces
        {
            get => m_Faces;
            set { m_Faces = value.ToArray(); SetDirty(); }
        }

        public void Upload()
        {
            if (m_UnityMesh == null)
            {
                m_UnityMesh = new Mesh();
                AssetDatabase.AddObjectToAsset(m_UnityMesh, this);
            }

            m_CompileVersion = m_Version;
            PMeshCompiler.Compile(this, m_UnityMesh);
            meshWasCompiled?.Invoke(this);
        }
    }
}