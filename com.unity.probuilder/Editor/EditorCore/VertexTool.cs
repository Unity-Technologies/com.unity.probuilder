using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	abstract class VertexTool : VertexManipulationTool
	{
		class MeshAndPositions : MeshAndElementGroupPair
		{
			Vector3[] m_Positions;

			public Vector3[] positions
			{
				get { return m_Positions; }
			}

			public MeshAndPositions(ProBuilderMesh mesh, PivotPoint pivot) : base(mesh, pivot)
			{
				m_Positions = mesh.positions.ToArray();

				var l2w = mesh.transform.localToWorldMatrix;

				for (int i = 0, c = m_Positions.Length; i < c; i++)
					m_Positions[i] = l2w.MultiplyPoint3x4(m_Positions[i]);
			}
		}

		protected override MeshAndElementGroupPair GetMeshAndElementGroupPair (ProBuilderMesh mesh, PivotPoint pivot)
		{
			return new MeshAndPositions(mesh, pivot);
		}

		protected void Apply(Matrix4x4 delta)
		{
			foreach (var key in meshAndElementGroupPairs)
			{
				if (!(key is MeshAndPositions))
					continue;

				var kvp = (MeshAndPositions)key;
				var mesh = kvp.mesh;
				var worldToLocal = mesh.transform.worldToLocalMatrix;
				var origins = kvp.positions;
				var positions = mesh.positionsInternal;

				foreach (var group in kvp.elementGroups)
				{
					foreach (var index in group.indices)
					{
						positions[index] = worldToLocal.MultiplyPoint3x4(
							group.inverseMatrix.MultiplyPoint3x4(
								delta.MultiplyPoint3x4(group.matrix.MultiplyPoint3x4(origins[index]))));
					}
				}

				mesh.mesh.vertices = positions;
				mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
				mesh.Refresh(RefreshMask.Normals);
			}

			ProBuilderEditor.UpdateMeshHandles(false);
		}
	}
}
