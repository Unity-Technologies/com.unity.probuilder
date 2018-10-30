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
	abstract class MeshAndElementGroupPair
	{
		ProBuilderMesh m_Mesh;
		List<ElementGroup> m_ElementGroups;

		public ProBuilderMesh mesh
		{
			get { return m_Mesh; }
		}

		public List<ElementGroup> elementGroups
		{
			get { return m_ElementGroups; }
		}

		public MeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot, bool collectCoincidentIndices)
		{
			m_Mesh = mesh;
			m_ElementGroups = ElementGroup.GetElementGroups(mesh, pivot, collectCoincidentIndices);
		}
	}

	class ElementGroup
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
			set
			{
				m_PreApplyPositionsMatrix = value;
				m_PostApplyPositionsMatrix = m_PreApplyPositionsMatrix.inverse;
			}
		}

		public Matrix4x4 inverseMatrix
		{
			get { return m_PostApplyPositionsMatrix; }
		}

		public static List<ElementGroup> GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, bool collectCoincident)
		{
			var groups = new List<ElementGroup>();
			var trs = mesh.transform.localToWorldMatrix;

			switch (pivot)
			{
				case PivotPoint.ModelBoundingBoxCenter:
				{
					var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
					var post = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), mesh.transform.rotation, Vector3.one);

					groups.Add(new ElementGroup()
					{
						m_Indices = collectCoincident
							? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
							: new List<int>(mesh.selectedIndexesInternal),
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

						List<int> indices;

						if (collectCoincident)
						{
							indices = new List<int>();
							mesh.GetCoincidentVertices(list, indices);
						}
						else
						{
							indices = list.SelectMany(x => x.distinctIndexesInternal).ToList();
						}

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
						m_Indices = collectCoincident
							? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
							: new List<int>(mesh.selectedIndexesInternal),
						m_PostApplyPositionsMatrix = Matrix4x4.Translate(MeshSelection.GetHandlePosition()),
						m_PreApplyPositionsMatrix = post.inverse
					});

					break;
				}
			}

			return groups;
		}

		static List<List<Face>> GetFaceSelectionGroups(ProBuilderMesh mesh)
		{
			var wings = WingedEdge.GetWingedEdges(mesh, mesh.selectedFacesInternal, true);
			var filter = new HashSet<Face>();
			var groups = new List<List<Face>>();

			foreach (var wing in wings)
			{
				var group = new List<Face>() { };
				CollectAdjacentFaces(wing, filter, group);
				if(group.Count > 0)
					groups.Add(group);
			}

			return groups;
		}

		static void CollectAdjacentFaces(WingedEdge wing, HashSet<Face> filter, List<Face> group)
		{
			if (!filter.Add(wing.face))
				return;

			group.Add(wing.face);

			var enumerator = new WingedEdgeEnumerator(wing);

			while (enumerator.MoveNext())
			{
				var opposite = enumerator.Current.opposite;
				if (opposite == null)
					continue;
				CollectAdjacentFaces(opposite, filter, group);
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
		List<MeshAndElementGroupPair> m_MeshAndElementGroupPairs = new List<MeshAndElementGroupPair>();
		bool m_IsEditing;

		float m_ProgridsSnapValue = .25f;
		bool m_SnapAxisConstraint = true;
		bool m_ProgridsSnapEnabled;
		static bool s_Initialized;
		static FieldInfo s_VertexDragging;
		static MethodInfo s_FindNearestVertex;
		static object[] s_FindNearestVertexArguments = new object[] { null, null, null };

		protected IEnumerable<MeshAndElementGroupPair> meshAndElementGroupPairs
		{
			get { return m_MeshAndElementGroupPairs; }
		}

		protected static bool vertexDragging
		{
			get
			{
				Init();
				return s_VertexDragging != null && (bool) s_VertexDragging.GetValue(null);
			}
		}

		protected bool isEditing
		{
			get { return m_IsEditing; }
		}

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

		protected float progridsSnapValue
		{
			get { return m_ProgridsSnapValue; }
		}

		protected bool snapAxisConstraint
		{
			get { return m_SnapAxisConstraint; }
		}

		protected bool progridsSnapEnabled
		{
			get { return m_ProgridsSnapEnabled; }
		}

		protected bool relativeSnapEnabled
		{
			get { return currentEvent.control; }
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

		protected abstract MeshAndElementGroupPair GetMeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot);

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

			DoTool(m_HandlePosition, m_HandleRotation);
		}

		protected abstract void DoTool(Vector3 handlePosition, Quaternion handleRotation);

		protected virtual void OnToolEngaged() { }

		protected virtual void OnToolDisengaged() { }

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

			m_ProgridsSnapEnabled = ProGridsInterface.SnapEnabled();
			m_ProgridsSnapValue = ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

			foreach (var mesh in selection)
			{
				mesh.ToMesh();
				mesh.Refresh();
			}

			m_MeshAndElementGroupPairs.Clear();

			foreach (var mesh in MeshSelection.topInternal)
				m_MeshAndElementGroupPairs.Add(GetMeshAndElementGroupPair(mesh, pivotPoint));

			OnToolEngaged();
		}

		protected void FinishEdit()
		{
			if (!m_IsEditing)
				return;

			Lightmapping.PopGIWorkflowMode();

			OnToolDisengaged();

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
