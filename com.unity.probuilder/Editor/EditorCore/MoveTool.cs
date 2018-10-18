using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

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

	public List<int> indices
	{
		get { return m_Indices; }
	}

	/// <summary>
	/// The coordinate space in which vertices are transformed.
	/// </summary>
	public Matrix4x4 matrix
	{
		get { return m_Matrix; }
	}

	public Matrix4x4 inverseMatrix
	{
		get { return m_Matrix.inverse; }
	}

	public static void GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, List<ElementGroup> groups)
	{
		Matrix4x4 matrix = Matrix4x4.identity;

		switch (pivot)
		{
			case PivotPoint.ModelBoundingBoxCenter:
				var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
				matrix = Matrix4x4.TRS(-bounds.center, Quaternion.identity, Vector3.one);
				break;

			case PivotPoint.IndividualOrigins:
				break;

//			case PivotPoint.WorldBoundingBoxCenter:
//			case PivotPoint.Custom:
			default:
				matrix = mesh.transform.localToWorldMatrix;
				break;
		}

		groups.Add(new ElementGroup()
		{
			m_Indices = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal),
			m_Matrix = matrix,
		});
	}
}

abstract class VertexManipulationTool
{
	Vector3 m_HandlePosition;
	Quaternion m_HandleRotation;

	Vector3 m_HandlePositionOrigin;
	Quaternion m_HandleRotationOrigin;

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
	/// <param name="translation">The translation to apply in handle coordinates (usually this corresponds to world space).</param>
	protected void ApplyTranslation(Vector3 translation)
	{
		foreach (var selectionGroup in m_Selection)
		{
			var delta = selectionGroup.applyDeltaInWorldSpace
				? translation
				: Quaternion.Inverse(m_HandleRotationOrigin) * translation;

			var mesh = selectionGroup.mesh;
			var origins = selectionGroup.positions;
			var positions = mesh.positionsInternal;

			foreach (var group in selectionGroup.selection)
			{
				foreach (var index in group.indices)
				{
					// todo Move matrix * origin to selection group c'tor
					positions[index] = group.inverseMatrix.MultiplyPoint3x4(group.matrix.MultiplyPoint3x4(origins[index]) + delta);
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

    protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
    {
	    if (!m_IsEditing)
		    m_HandlePosition = handlePosition;

	    EditorGUI.BeginChangeCheck();

	    m_HandlePosition = Handles.PositionHandle(m_HandlePosition, handleRotation);

	    if (EditorGUI.EndChangeCheck())
	    {
		    if(!m_IsEditing)
			    BeginEdit();

		    delta = m_HandlePosition - handlePositionOrigin;

		    ApplyTranslation(delta);
	    }

	    Handles.BeginGUI();
	    GUILayout.Label(delta.ToString());
	    Handles.EndGUI();


//	    if (!m_IsMovingElements)
//			m_ElementHandlePosition = m_HandlePosition;
//
//		m_ElementHandleCachedPosition = m_ElementHandlePosition;
//
//		m_ElementHandlePosition = Handles.PositionHandle(m_ElementHandlePosition, m_HandleRotation);
//
//		if (m_CurrentEvent.alt)
//			return;
//
//		if (m_ElementHandlePosition != m_ElementHandleCachedPosition)
//		{
//			Vector3 diff = m_ElementHandlePosition - m_ElementHandleCachedPosition;
//
//			Vector3 mask = diff.ToMask(Math.handleEpsilon);
//
//			if (m_DoSnapToVertex)
//			{
//				Vector3 v;
//
//				if (FindNearestVertex(m_CurrentEvent.mousePosition, out v))
//					diff = Vector3.Scale(v - m_ElementHandleCachedPosition, mask);
//			}
//			else if (m_DoSnapToFace)
//			{
//				ProBuilderMesh obj = null;
//				RaycastHit hit;
//				Dictionary<ProBuilderMesh, HashSet<Face>> ignore = new Dictionary<ProBuilderMesh, HashSet<Face>>();
//				foreach (ProBuilderMesh pb in selection)
//					ignore.Add(pb, new HashSet<Face>(pb.selectedFacesInternal));
//
//				if (EditorHandleUtility.FaceRaycast(m_CurrentEvent.mousePosition, out obj, out hit, ignore))
//				{
//					if (mask.IntSum() == 1)
//					{
//						Ray r = new Ray(m_ElementHandleCachedPosition, -mask);
//						Plane plane = new Plane(obj.transform.TransformDirection(hit.normal).normalized,
//							obj.transform.TransformPoint(hit.point));
//
//						float forward, backward;
//						plane.Raycast(r, out forward);
//						plane.Raycast(r, out backward);
//						float planeHit = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
//						r.direction = -r.direction;
//						plane.Raycast(r, out forward);
//						plane.Raycast(r, out backward);
//						float rev = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
//						if (Mathf.Abs(rev) > Mathf.Abs(planeHit))
//							planeHit = rev;
//
//						if (Mathf.Abs(planeHit) > Mathf.Epsilon)
//							diff = mask * -planeHit;
//					}
//					else
//					{
//						diff = Vector3.Scale(obj.transform.TransformPoint(hit.point) - m_ElementHandleCachedPosition, mask.Abs());
//					}
//				}
//			}
//
//			if (!m_IsMovingElements)
//			{
//				m_IsMovingElements = true;
//				m_TranslateOrigin = m_ElementHandleCachedPosition;
//				m_RotateOrigin = m_HandleRotation.eulerAngles;
//				m_ScaleOrigin = m_HandleScale;
//
//				OnBeginVertexMovement();
//
//				if (Event.current.modifiers == EventModifiers.Shift)
//					ShiftExtrude();
//
//				ProGridsInterface.OnHandleMove(mask);
//			}
//
//			for (int i = 0; i < selection.Length; i++)
//			{
//				var mesh = selection[i];
//
//				mesh.TranslateVerticesInWorldSpace(mesh.selectedIndexesInternal,
//					diff,
//					m_SnapEnabled ? m_SnapValue : 0f,
//					m_SnapAxisConstraint);
//
//				mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
//				mesh.Refresh(RefreshMask.Normals);
//				mesh.mesh.RecalculateBounds();
//			}
//
//			UpdateMeshHandles();
//		}
    }
}
