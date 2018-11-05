using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	abstract class VertexTool : VertexManipulationTool
	{
		const bool k_CollectCoincidentVertices = true;
#if APPLY_POSITION_TO_SPACE_GIZMO
		Matrix4x4 m_CurrentDelta = Matrix4x4.identity;
#endif

		class MeshAndPositions : MeshAndElementGroupPair
		{
			Vector3[] m_Positions;

			public Vector3[] positions
			{
				get { return m_Positions; }
			}

			public MeshAndPositions(ProBuilderMesh mesh, PivotPoint pivot) : base(mesh, pivot, k_CollectCoincidentVertices)
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

		protected override void DoTool(Vector3 position, Quaternion rotation)
		{
			if ( isEditing && currentEvent.type == EventType.Repaint)
			{
				foreach (var key in meshAndElementGroupPairs)
				{
					foreach (var group in key.elementGroups)
					{
#if DEBUG_HANDLES
							using (var faceDrawer = new EditorMeshHandles.TriangleDrawingScope(Color.cyan, CompareFunction.Always))
							{
								foreach (var face in key.mesh.GetSelectedFaces())
								{
									var indices = face.indexesInternal;

									for (int i = 0, c = indices.Length; i < c; i += 3)
									{
										faceDrawer.Draw(
											group.matrix.MultiplyPoint3x4(key.positions[indices[i]]),
											group.matrix.MultiplyPoint3x4(key.positions[indices[i + 1]]),
											group.matrix.MultiplyPoint3x4(key.positions[indices[i + 2]])
										);
									}
								}
							}
#endif

#if APPLY_POSITION_TO_SPACE_GIZMO
						EditorMeshHandles.DrawGizmo(Vector3.zero, group.matrix.inverse * m_CurrentDelta);
#else
						EditorMeshHandles.DrawGizmo(Vector3.zero, group.matrix.inverse);
#endif
					}
				}
			}
		}

		protected void Apply(Matrix4x4 delta)
		{
#if APPLY_POSITION_TO_SPACE_GIZMO
			m_CurrentDelta.SetColumn(3, delta.GetColumn(3));
#endif

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
