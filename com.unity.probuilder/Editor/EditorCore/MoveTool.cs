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
		bool m_ApplyDeltaInWorldSpace;

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

		public bool applyDeltaInWorldSpace
		{
			get { return m_ApplyDeltaInWorldSpace; }
		}

		public MeshAndElementSelection(ProBuilderMesh mesh, PivotPoint pivot)
		{
			m_Mesh = mesh;
			m_Positions = mesh.positions.ToArray();
			var l2w = m_Mesh.transform.localToWorldMatrix;
			for (int i = 0, c = m_Positions.Length; i < c; i++)
				m_Positions[i] = l2w.MultiplyPoint3x4(m_Positions[i]);
			m_ApplyDeltaInWorldSpace = pivot == PivotPoint.WorldBoundingBoxCenter;
			var groups = new List<ElementGroup>();
			ElementGroup.GetElementGroups(mesh, pivot, groups);
			m_Selection = groups;
		}
	}

	struct ElementGroup
	{
		List<int> m_Indices;
		Matrix4x4 m_Matrix;
		Matrix4x4 m_InverseMatrix;

		public List<int> indices
		{
			get { return m_Indices; }
		}

		/// <summary>
		/// An additional transform to be applied to world space vertices prior to handle apply.
		/// </summary>
		public Matrix4x4 matrix
		{
			get { return m_Matrix; }
		}

		public Matrix4x4 inverseMatrix
		{
			get { return m_InverseMatrix; }
		}

		public static void GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, List<ElementGroup> groups)
		{
			Matrix4x4 inverse = Matrix4x4.identity;
			var trs = mesh.transform.localToWorldMatrix;

			switch (pivot)
			{
				case PivotPoint.ModelBoundingBoxCenter:
					var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
					inverse = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), mesh.transform.rotation, Vector3.one);
					break;

				case PivotPoint.IndividualOrigins:
					break;

				//			case PivotPoint.WorldBoundingBoxCenter:
				//			case PivotPoint.Custom:
				default:
					inverse = Matrix4x4.Translate(MeshSelection.GetHandlePosition());
					break;
			}

			groups.Add(new ElementGroup()
			{
				m_Indices = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal),
				m_InverseMatrix = inverse,
				m_Matrix = inverse.inverse,
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

			if (!m_IsEditing)
			{
				m_HandlePosition = MeshSelection.GetHandlePosition();
				m_HandleRotation = MeshSelection.GetHandleRotation(ProBuilderEditor.handleOrientation);
			}
			else
			{
				if (evt.type == EventType.Repaint && Tools.pivotMode == PivotMode.Pivot)
				{
					foreach (var key in m_Selection)
					{
#if DEBUG
						foreach (var group in key.selection)
						{
							using (var faceDrawer = new EditorMeshHandles.FaceDrawingScope(Color.cyan, CompareFunction.Always))
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

							var m = group.matrix.inverse;
							var p = m.MultiplyPoint3x4(Vector3.zero);

							var size = HandleUtility.GetHandleSize(p) * .5f;

							if (EditorMeshHandles.BeginDrawingLines(Color.green, CompareFunction.Always))
							{
								EditorMeshHandles.DrawLine(p, p + m.MultiplyVector(Vector3.up) * size);
								EditorMeshHandles.EndDrawingLines();
							}

							if (EditorMeshHandles.BeginDrawingLines(Color.red, CompareFunction.Always))
							{
								EditorMeshHandles.DrawLine(p, p + m.MultiplyVector(Vector3.right) * size);
								EditorMeshHandles.EndDrawingLines();
							}

							if (EditorMeshHandles.BeginDrawingLines(Color.blue, CompareFunction.Always))
							{
								EditorMeshHandles.DrawLine(p, p + m.MultiplyVector(Vector3.forward) * size);
								EditorMeshHandles.EndDrawingLines();
							}
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
				var pivot = Tools.pivotMode == PivotMode.Center
					? PivotPoint.WorldBoundingBoxCenter
					: PivotPoint.ModelBoundingBoxCenter;

				m_Selection.Add(new MeshAndElementSelection(mesh, pivot));
			}
		}

		protected void FinishEdit()
		{
			if (!m_IsEditing)
				return;

			ProBuilderEditor.instance.OnFinishVertexModification();

			m_IsEditing = false;
		}

		/// <summary>
		///
		/// </summary>
		protected void Apply(Matrix4x4 delta)
		{
			foreach (var selectionGroup in m_Selection)
			{
				var mesh = selectionGroup.mesh;
				var worldToLocal = mesh.transform.worldToLocalMatrix;
				var localToWorld = Matrix4x4.Rotate(mesh.transform.localRotation);
				var origins = selectionGroup.positions;
				var positions = mesh.positionsInternal;

				var transformed = selectionGroup.applyDeltaInWorldSpace
					? delta
					: localToWorld * (delta * Matrix4x4.Rotate(m_HandleRotationOrigin).inverse);

				foreach (var group in selectionGroup.selection)
				{
					foreach (var index in group.indices)
					{
						// todo Move matrix * origin to selection group c'tor
//						if (selectionGroup.applyDeltaInWorldSpace)
//						{
							// position = WorldToLocal * (InverseGroupMatrix * (delta * (GroupMatrix * WorldPosition)))
							positions[index] = worldToLocal.MultiplyPoint3x4(
								group.inverseMatrix.MultiplyPoint3x4(
									transformed.MultiplyPoint3x4(
										group.matrix.MultiplyPoint3x4(origins[index]))));
//						}
//						else
//						{
//							var p = group.matrix.MultiplyPoint3x4(origins[index]);
//							p = m_HandleRotationOrigin * p;
//							p = delta.MultiplyPoint3x4(p);
//							p = Quaternion.Inverse(m_HandleRotationOrigin) * p;
//							positions[index] = group.inverseMatrix.MultiplyPoint3x4(p);
//						}
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
		Vector3 delta = Vector3.zero;

		void DoTranslate(Vector3 deltaInWorldSpace)
		{
			foreach (var key in m_Selection)
			{
				var mesh = key.mesh;
				var meshRotation = mesh.transform.rotation;
				var worldToLocal = mesh.transform.worldToLocalMatrix;
				var origins = key.positions;
				var positions = mesh.positionsInternal;

				var delta = key.applyDeltaInWorldSpace
					? deltaInWorldSpace
					: (handleRotationOriginInverse * deltaInWorldSpace);

				var mat = Matrix4x4.Translate(delta);

				foreach (var group in key.selection)
				{
					foreach (var index in group.indices)
					{
						positions[index] = worldToLocal.MultiplyPoint3x4(
							group.inverseMatrix.MultiplyPoint3x4(
								mat.MultiplyPoint3x4(group.matrix.MultiplyPoint3x4(origins[index]))));
					}
				}

				mesh.mesh.vertices = positions;
				mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
				mesh.Refresh(RefreshMask.Normals);
			}

			ProBuilderEditor.UpdateMeshHandles(false);
		}

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

				delta = m_HandlePosition - handlePositionOrigin;

				DoTranslate(delta);
			}
		}
	}

	class RotateTool : VertexManipulationTool
	{
		Quaternion m_Rotation;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_Rotation = handleRotation;

			EditorGUI.BeginChangeCheck();
			m_Rotation = Handles.RotationHandle(m_Rotation, handlePosition);
			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit();

				Apply(Matrix4x4.Rotate(m_Rotation * handleRotationOriginInverse));
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