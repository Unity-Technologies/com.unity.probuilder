using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace UnityEditor.ProBuilder
{
	struct MeshAndElementSelection
	{
		ProBuilderMesh m_Mesh;
		Vector3[] m_Positions;
		List<ElementGroup> m_Selection;

		public ProBuilderMesh mesh
		{
			get { return m_Mesh; }
		}

		public Vector3[] positions
		{
			get { return m_Positions; }
		}

		public List<ElementGroup> selection
		{
			get { return m_Selection; }
		}

		public MeshAndElementSelection(ProBuilderMesh mesh, PivotPoint pivot)
		{
			m_Mesh = mesh;
			m_Positions = mesh.positions.ToArray();
			var l2w = m_Mesh.transform.localToWorldMatrix;
			for (int i = 0, c = m_Positions.Length; i < c; i++)
				m_Positions[i] = l2w.MultiplyPoint3x4(m_Positions[i]);
			var groups = new List<ElementGroup>();
			ElementGroup.GetElementGroups(mesh, pivot, groups);
			m_Selection = groups;
		}
	}

	struct ElementGroup
	{
		List<int> m_Indices;
		Matrix4x4 m_PreApplyPositionsMatrix;
		Matrix4x4 m_PostApplyPositionsMatrix;

		public List<int> indices
		{
			get { return m_Indices; }
		}

		/// <summary>
		/// An additional transform to be applied to world space vertices prior to handle apply.
		/// </summary>
		public Matrix4x4 matrix
		{
			get { return m_PreApplyPositionsMatrix; }
		}

		public Matrix4x4 inverseMatrix
		{
			get { return m_PostApplyPositionsMatrix; }
		}

		public static void GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, List<ElementGroup> groups)
		{
			Matrix4x4 postApplyPositionsMatrix, handleMatrix;
			var trs = mesh.transform.localToWorldMatrix;

			switch (pivot)
			{
				case PivotPoint.ModelBoundingBoxCenter:
				{
					var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
					postApplyPositionsMatrix = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), mesh.transform.rotation, Vector3.one);
					break;
				}

				case PivotPoint.IndividualOrigins:
				{
					var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
					var ntb = Math.NormalTangentBitangent(mesh, mesh.selectedFacesInternal[0]);
					var rot = mesh.transform.rotation * Quaternion.LookRotation(ntb.normal, ntb.bitangent);
					postApplyPositionsMatrix = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), rot, Vector3.one);
					break;
				}

				default:
				{
					postApplyPositionsMatrix = Matrix4x4.Translate(MeshSelection.GetHandlePosition());
					break;
				}
			}

			groups.Add(new ElementGroup()
			{
				m_Indices = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal),
				m_PostApplyPositionsMatrix = postApplyPositionsMatrix,
				m_PreApplyPositionsMatrix = postApplyPositionsMatrix.inverse
			});
		}
	}

	abstract class VertexManipulationTool
	{
		Vector3 m_HandlePosition;
		Quaternion m_HandleRotation;

		Vector3 m_HandlePositionOrigin;
		Quaternion m_HandleRotationOrigin;
		protected Quaternion handleRotationOriginInverse { get; private set; }

		protected PivotPoint pivotPoint { get; private set; }

		protected Vector3 handlePositionOrigin
		{
			get { return m_HandlePositionOrigin; }
		}

		protected Quaternion handleRotationOrigin
		{
			get { return m_HandleRotationOrigin; }
		}

		protected List<MeshAndElementSelection> m_Selection = new List<MeshAndElementSelection>();

		protected bool m_IsEditing;

		public void OnSceneGUI(Event evt)
		{
			if (evt.type == EventType.MouseUp || evt.type == EventType.Ignore)
				FinishEdit();

			if (evt.alt)
				return;

			if (Tools.pivotMode == PivotMode.Center)
				pivotPoint = PivotPoint.WorldBoundingBoxCenter;
			else if(ProBuilderEditor.handleOrientation == HandleOrientation.Normal)
				pivotPoint = PivotPoint.IndividualOrigins;
			else
				pivotPoint = PivotPoint.ModelBoundingBoxCenter;

			if (!m_IsEditing)
			{
				m_HandlePosition = MeshSelection.GetHandlePosition();
				m_HandleRotation = MeshSelection.GetHandleRotation(ProBuilderEditor.handleOrientation);
			}
			else
			{
#if DEBUG_HANDLES
				if (evt.type == EventType.Repaint)
#else
				if (evt.type == EventType.Repaint && pivotPoint != PivotPoint.WorldBoundingBoxCenter)
#endif
				{
					foreach (var key in m_Selection)
					{
						foreach (var group in key.selection)
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
							EditorMeshHandles.DrawGizmo(Vector3.zero, group.matrix.inverse);
						}
					}
				}
			}

			DoTool(m_HandlePosition, m_HandleRotation);
		}

		protected abstract void DoTool(Vector3 handlePosition, Quaternion handleRotation);

		protected void BeginEdit()
		{
			if (m_IsEditing)
				return;

			m_IsEditing = true;

			m_HandlePositionOrigin = m_HandlePosition;
			m_HandleRotationOrigin = m_HandleRotation;
			handleRotationOriginInverse = Quaternion.Inverse(m_HandleRotation);

			ProBuilderEditor.instance.OnBeginVertexMovement();

			m_Selection.Clear();

			foreach (var mesh in MeshSelection.topInternal)
			{
				m_Selection.Add(new MeshAndElementSelection(mesh, pivotPoint));
			}
		}

		protected void FinishEdit()
		{
			if (!m_IsEditing)
				return;

			ProBuilderEditor.instance.OnFinishVertexModification();

			m_IsEditing = false;
		}

		protected void Apply(Matrix4x4 delta)
		{
			foreach (var key in m_Selection)
			{
				var mesh = key.mesh;
				var worldToLocal = mesh.transform.worldToLocalMatrix;
				var origins = key.positions;
				var positions = mesh.positionsInternal;

				foreach (var group in key.selection)
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

	class MoveTool : VertexManipulationTool
	{
		Vector3 m_HandlePosition;
		Matrix4x4 m_Translation = Matrix4x4.identity;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_HandlePosition = handlePosition;

			EditorGUI.BeginChangeCheck();

			m_HandlePosition = Handles.PositionHandle(m_HandlePosition, handleRotation);

			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit();

				var delta = m_HandlePosition - handlePositionOrigin;

				switch (pivotPoint)
				{
					case PivotPoint.WorldBoundingBoxCenter:
						break;

					case PivotPoint.ModelBoundingBoxCenter:
						delta = handleRotationOriginInverse * delta;
						break;

					case PivotPoint.IndividualOrigins:
						delta = handleRotationOriginInverse * delta;
						break;
				}

				m_Translation.SetTRS(delta, Quaternion.identity, Vector3.one);

				Apply(m_Translation);
			}
		}
	}

	class RotateTool : VertexManipulationTool
	{
		Quaternion m_Rotation;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_Rotation = Quaternion.identity;

			EditorGUI.BeginChangeCheck();

			var hm = Handles.matrix;
			Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);
			m_Rotation = Handles.RotationHandle(m_Rotation, Vector3.zero);
			Handles.matrix = hm;

			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit();

				Apply(Matrix4x4.Rotate(m_Rotation));
			}
		}
	}

	class ScaleTool : VertexManipulationTool
	{
		Vector3 m_Scale;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_Scale = Vector3.one;

			EditorGUI.BeginChangeCheck();
			m_Scale = Handles.ScaleHandle(m_Scale, handlePosition, handleRotation, UnityEditor.HandleUtility.GetHandleSize(handlePosition));
			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit();
				Apply(Matrix4x4.Scale(m_Scale));
			}
		}
	}
}