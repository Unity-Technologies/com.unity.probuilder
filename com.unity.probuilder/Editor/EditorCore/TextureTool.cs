using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class TextureTool : VertexManipulationTool
	{
		class MeshAndTextures : MeshAndElementGroupPair
		{
			List<Vector4> m_Textures;

			public MeshAndTextures(ProBuilderMesh mesh, PivotPoint pivot)
				: base(mesh, pivot)
			{

			}
		}

		Matrix4x4 m_HandleMatrix;

		protected void PushMatrix()
		{
			m_HandleMatrix = Handles.matrix;
		}

		protected void PopMatrix()
		{
			m_HandleMatrix = Handles.matrix;
		}

		protected override MeshAndElementGroupPair GetMeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot)
		{
			return new MeshAndTextures(mesh, pivot);
		}

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation) { }
	}
}
