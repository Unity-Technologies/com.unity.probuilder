using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PMeshFilter : MonoBehaviour
{
    [SerializeField]
    PMesh m_Mesh;

    public PMesh mesh
    {
        get => m_Mesh;
        set => m_Mesh = value;
    }

    public void SyncMeshFilter()
    {
        if (m_Mesh == null)
            return;
        GetComponent<MeshFilter>().sharedMesh = m_Mesh.unityMesh;
    }
}