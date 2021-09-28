using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PMesh : ScriptableObject
{
    [SerializeField]
    Vector3[] m_Positions;
    
    [SerializeField]
    int[] m_Indices;

    [SerializeField]
    int m_Version;

    [SerializeField]
    Mesh m_UnityMesh;

    public int version => m_Version;
    public Mesh unityMesh => m_UnityMesh;
    public static event Action<Mesh> meshWasModified;
    
    new void SetDirty()
    {
        m_Version++;
        Upload();
        meshWasModified?.Invoke(m_UnityMesh);
    }

    public Vector3[] positions
    {
        get => m_Positions;
        
        set
        {
            m_Positions = value.ToArray();
            SetDirty();
        }
    }
    
    public int[] indices
    {
        get => m_Indices;

        set
        {
            m_Indices = value.ToArray();
            SetDirty();
        }
    }

    void Upload()
    {
        if (m_UnityMesh == null)
        {
            m_UnityMesh = new Mesh();
            AssetDatabase.AddObjectToAsset(m_UnityMesh, this);
        }
        
        m_UnityMesh.Clear();
        m_UnityMesh.SetVertices(positions);
        m_UnityMesh.subMeshCount = 1;
        m_UnityMesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }
}