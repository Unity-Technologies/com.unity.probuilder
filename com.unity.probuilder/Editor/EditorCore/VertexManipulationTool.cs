using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

#if DEBUG_HANDLES
using UnityEngine.Rendering;
#endif

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
			var trs = mesh.transform.localToWorldMatrix;

			switch (pivot)
			{
				case PivotPoint.ModelBoundingBoxCenter:
				{
					var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
					var post = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), mesh.transform.rotation, Vector3.one);

					groups.Add(new ElementGroup()
					{
						m_Indices = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal),
						m_PostApplyPositionsMatrix = post,
						m_PreApplyPositionsMatrix = post.inverse
					});

					break;
				}

				case PivotPoint.IndividualOrigins:
				{
					if (ProBuilderEditor.selectMode != SelectMode.Face)
						goto case PivotPoint.ModelBoundingBoxCenter;

					foreach (var list in GetFaceSelectionGroups(mesh))
					{
						var bounds = Math.GetBounds(mesh.positionsInternal, list);
						var ntb = Math.NormalTangentBitangent(mesh, list[0]);
						var rot = mesh.transform.rotation * Quaternion.LookRotation(ntb.normal, ntb.bitangent);
						var post = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), rot, Vector3.one);

						var indices = new List<int>();
						mesh.GetCoincidentVertices(list, indices);

						groups.Add(new ElementGroup()
						{
							m_Indices = indices,
							m_PostApplyPositionsMatrix = post,
							m_PreApplyPositionsMatrix = post.inverse
						});
					}
					break;
				}

				default:
				{
					var post = Matrix4x4.Translate(MeshSelection.GetHandlePosition());

					groups.Add(new ElementGroup()
					{
						m_Indices = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal),
						m_PostApplyPositionsMatrix = Matrix4x4.Translate(MeshSelection.GetHandlePosition()),
						m_PreApplyPositionsMatrix = post.inverse
					});

					break;
				}
			}
		}

		internal static List<List<Face>> GetFaceSelectionGroups(ProBuilderMesh mesh)
		{
			var wings = WingedEdge.GetWingedEdges(mesh, mesh.selectedFacesInternal, true);
			var filter = new HashSet<Face>();
			var groups = new List<List<Face>>();

			foreach (var wing in wings)
			{
				if (!filter.Add(wing.face))
					continue;

				var group = new List<Face>() { wing.face };
				CollectAdjacentFaces(wing, filter, group);
				groups.Add(group);
			}

			return groups;
		}

		static void CollectAdjacentFaces(WingedEdge wing, HashSet<Face> filter, List<Face> group)
		{
			var enumerator = new WingedEdgeEnumerator(wing);

			while (enumerator.MoveNext())
			{
				var cur = enumerator.Current.opposite;

				if (cur  == null)
					continue;

				var face = cur.face;

				if (!filter.Add(face))
					continue;

				group.Add(face);
				CollectAdjacentFaces(enumerator.Current, filter, group);
			}
		}
	}

	abstract class VertexManipulationTool
	{
		internal static Pref<bool> s_ExtrudeEdgesAsGroup = new Pref<bool>("editor.extrudeEdgesAsGroup", true);
		internal static Pref<ExtrudeMethod> s_ExtrudeMethod = new Pref<ExtrudeMethod>("editor.extrudeMethod", ExtrudeMethod.FaceNormal);

		Vector3 m_HandlePosition;
		Quaternion m_HandleRotation;

		Vector3 m_HandlePositionOrigin;
		Quaternion m_HandleRotationOrigin;
		protected Quaternion handleRotationOriginInverse { get; private set; }

		protected Event currentEvent { get; private set; }

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
			currentEvent = evt;

			if (evt.type == EventType.MouseUp || evt.type == EventType.Ignore)
				FinishEdit();

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
				if (evt.type == EventType.Repaint)
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

			if (currentEvent.shift)
				Extrude();

			m_IsEditing = true;

			m_HandlePositionOrigin = m_HandlePosition;
			m_HandleRotationOrigin = m_HandleRotation;
			handleRotationOriginInverse = Quaternion.Inverse(m_HandleRotation);

			ProBuilderEditor.instance.OnBeginVertexMovement();

			m_Selection.Clear();

			foreach (var mesh in MeshSelection.topInternal)
				m_Selection.Add(new MeshAndElementSelection(mesh, pivotPoint));
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

		static void Extrude()
		{
			int ef = 0;

			var selection = MeshSelection.topInternal;
			var selectMode = ProBuilderEditor.selectMode;

			UndoUtility.RecordSelection("Extrude Vertices");

			foreach (var mesh in selection)
			{
				switch (selectMode)
				{
					case SelectMode.Edge:
						if (mesh.selectedFaceCount > 0)
							goto default;

						Edge[] newEdges = mesh.Extrude(mesh.selectedEdges,
							0.0001f,
							s_ExtrudeEdgesAsGroup,
							ProBuilderEditor.s_AllowNonManifoldActions);

						if (newEdges != null)
						{
							ef += newEdges.Length;
							mesh.SetSelectedEdges(newEdges);
						}
						break;

					default:
						int len = mesh.selectedFacesInternal.Length;

						if (len > 0)
						{
							mesh.Extrude(mesh.selectedFacesInternal, s_ExtrudeMethod, 0.0001f);
							mesh.SetSelectedFaces(mesh.selectedFacesInternal);
							ef += len;
						}

						break;
				}

				mesh.ToMesh();
				mesh.Refresh();
			}

			if (ef > 0)
			{
				EditorUtility.ShowNotification("Extrude");
				ProBuilderEditor.Refresh();
			}
		}
	}
}
