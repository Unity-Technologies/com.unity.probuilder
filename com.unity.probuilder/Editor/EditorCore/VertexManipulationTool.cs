using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;

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
		/// <value>
		/// Called when vertex modifications are complete.
		/// </value>
		public static event Action<ProBuilderMesh[]> afterMeshModification;

		/// <value>
		/// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
		/// </value>
		public static event Action<ProBuilderMesh[]> beforeMeshModification;

		internal static Pref<bool> s_ExtrudeEdgesAsGroup = new Pref<bool>("editor.extrudeEdgesAsGroup", true);
		internal static Pref<ExtrudeMethod> s_ExtrudeMethod = new Pref<ExtrudeMethod>("editor.extrudeMethod", ExtrudeMethod.FaceNormal);

		Vector3 m_HandlePosition;
		Quaternion m_HandleRotation;
		Vector3 m_HandlePositionOrigin;
		Quaternion m_HandleRotationOrigin;

		float m_SnapValue = .25f;
		bool m_SnapAxisConstraint = true;
		bool m_SnapEnabled;
		static bool s_Initialized;
		static FieldInfo s_VertexDragging;
		static MethodInfo s_FindNearestVertex;
		static object[] s_FindNearestVertexArguments = new object[] { null, null, null };

		protected static bool vertexDragging
		{
			get
			{
				Init();
				return s_VertexDragging != null && (bool) s_VertexDragging.GetValue(null);
			}
		}

		protected List<MeshAndElementSelection> m_Selection = new List<MeshAndElementSelection>();
		protected bool m_IsEditing;

		protected Event currentEvent { get; private set; }

		protected PivotPoint pivotPoint { get; private set; }

		protected Vector3 handlePositionOrigin
		{
			get { return m_HandlePositionOrigin; }
		}

		protected Quaternion handleRotationOriginInverse { get; private set; }

		protected Quaternion handleRotationOrigin
		{
			get { return m_HandleRotationOrigin; }
		}

		protected float snapValue
		{
			get { return m_SnapValue; }
		}

		protected bool snapAxisConstraint
		{
			get { return m_SnapAxisConstraint; }
		}

		protected bool snapEnabled
		{
			get { return m_SnapEnabled; }
		}

		static void Init()
		{
			if (s_Initialized)
				return;
			s_Initialized = true;
			s_VertexDragging = typeof(Tools).GetField("vertexDragging", BindingFlags.NonPublic | BindingFlags.Static);
			s_FindNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex",
				BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
		}

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

		protected void BeginEdit(string undoMessage)
		{
			if (m_IsEditing)
				return;

			// Disable iterative lightmapping
			Lightmapping.PushGIWorkflowMode();

			var selection = MeshSelection.topInternal.ToArray();

			UndoUtility.RegisterCompleteObjectUndo(selection, string.IsNullOrEmpty(undoMessage) ? "Modify Vertices" : undoMessage);

			if (beforeMeshModification != null)
				beforeMeshModification(selection);

			if (currentEvent.shift)
				Extrude();

			m_IsEditing = true;

			m_HandlePositionOrigin = m_HandlePosition;
			m_HandleRotationOrigin = m_HandleRotation;
			handleRotationOriginInverse = Quaternion.Inverse(m_HandleRotation);

			m_SnapEnabled = ProGridsInterface.SnapEnabled();
			m_SnapValue = ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

			foreach (var mesh in selection)
			{
				mesh.ToMesh();
				mesh.Refresh();
			}

			m_Selection.Clear();

			foreach (var mesh in MeshSelection.topInternal)
				m_Selection.Add(new MeshAndElementSelection(mesh, pivotPoint));
		}

		protected void FinishEdit()
		{
			if (!m_IsEditing)
				return;

			Lightmapping.PopGIWorkflowMode();

			var selection = MeshSelection.topInternal.ToArray();

			foreach (var mesh in selection)
			{
				mesh.ToMesh();
				mesh.Refresh();
				mesh.Optimize();
			}

			ProBuilderEditor.Refresh();

			if (afterMeshModification != null)
				afterMeshModification(selection);

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

		/// <summary>
		/// Find the nearest vertex among all visible objects.
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="vertex"></param>
		/// <returns></returns>
		protected static bool FindNearestVertex(Vector2 mousePosition, out Vector3 vertex)
		{
			s_FindNearestVertexArguments[0] = mousePosition;

			if (s_FindNearestVertex == null)
				s_FindNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex",
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			object result = s_FindNearestVertex.Invoke(null, s_FindNearestVertexArguments);
			vertex = (bool) result ? (Vector3) s_FindNearestVertexArguments[2] : Vector3.zero;
			return (bool) result;
		}
	}
}
