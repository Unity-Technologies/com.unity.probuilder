using UnityEngine;
using System.Collections;

namespace UnityEngine.ProBuilder
{
	sealed class MeshHandle
	{
		Transform m_Transform;
		Mesh m_Mesh;

		public Mesh mesh
		{
			get { return m_Mesh; }
		}

		public MeshHandle(Transform transform, Mesh mesh)
		{
			m_Transform = transform;
			m_Mesh = mesh;
		}

		public void DrawMeshNow(int submeshIndex)
		{
			Graphics.DrawMeshNow(m_Mesh, m_Transform.localToWorldMatrix, submeshIndex);
		}
	}
}
