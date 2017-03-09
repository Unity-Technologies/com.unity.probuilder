using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	[CustomEditor(typeof(pb_PolyShape))]
	public class pb_PolyShapeTool : Editor
	{
		[SerializeField] private Material m_LineMaterial;
		private Mesh m_LineMesh = null;
		private Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

		private pb_PolyShape polygon { get { return target as pb_PolyShape; } }

		void OnEnable()
		{
			pb_Editor.AddOnEditLevelChangedListener(OnEditLevelChange);
			m_LineMesh = new Mesh();
			Undo.undoRedoPerformed += UndoRedoPerformed;
			DrawPolyLine(polygon.points);
			EditorApplication.update += Update;
		}

		void OnDisable()
		{
			pb_Editor.RemoveOnEditLevelChangedListener(OnEditLevelChange);
			GameObject.DestroyImmediate(m_LineMesh);
			EditorApplication.update -= Update;
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			polygon.isEditing = EditorGUILayout.Toggle("Edit Mode", polygon.isEditing);

			if(EditorGUI.EndChangeCheck())
			{
				if(pb_Editor.instance != null)
					pb_Editor.instance.SetEditLevel(EditLevel.Plugin);

				if(polygon.isEditing)
					Tools.current = Tool.None;
			}
		}

		void Update()
		{
			m_LineMaterial.SetFloat("_EditorTime", (float) EditorApplication.timeSinceStartup);
		}

		void UpdateMesh(bool vertexCountChanged = true)
		{
			polygon.Refresh();

			DrawPolyLine(polygon.points);

			if(pb_Editor.instance != null)
			{
				if(!vertexCountChanged)
					pb_Editor.instance.Internal_UpdateSelectionFast();
				else
					pb_Editor.Refresh();
			}
		}

		void OnSceneGUI()
		{
			if(polygon == null || !polygon.isEditing || Tools.current != Tool.None)
			{
				if(polygon.isEditing)
					polygon.isEditing = false;

				return;
			}

			m_LineMaterial.SetPass(0);
			Graphics.DrawMeshNow(m_LineMesh, polygon.transform.localToWorldMatrix, 0);

			DoExistingPointsGUI();

			Event evt = Event.current;

			if( pb_Handle_Utility.SceneViewInUse(evt) )
				return;

			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);

			if(evt.type == EventType.MouseDown)
			{	
				DoPointPlacement( HandleUtility.GUIPointToWorldRay(evt.mousePosition) );
				SceneView.RepaintAll();
			}
		}

		void DoPointPlacement(Ray ray)
		{
			float hitDistance = Mathf.Infinity;
			m_Plane.SetNormalAndPosition(polygon.transform.up, polygon.transform.position);

			if( m_Plane.Raycast(ray, out hitDistance) )
			{
				pbUndo.RecordObject(polygon, "Add Polygon Shape Point");
				polygon.points.Add(polygon.transform.InverseTransformPoint(ray.GetPoint(hitDistance)));
				UpdateMesh();
			}
		}

		void DoExistingPointsGUI()
		{
			Transform trs = polygon.transform;
			int len = polygon.points.Count;
			Vector3 up = polygon.transform.up;
			Vector3 right = polygon.transform.right;
			Vector3 forward = polygon.transform.forward;

			for(int ii = 0; ii < len; ii++)
			{
				Vector3 point = trs.TransformPoint(polygon.points[ii]);

				float size = HandleUtility.GetHandleSize(point) * .05f;

				EditorGUI.BeginChangeCheck();

				point = Handles.Slider2D(point, up, right, forward, size, Handles.DotCap, Vector2.zero, true);

				if(EditorGUI.EndChangeCheck())
				{
					pbUndo.RecordObject(polygon, "Move Polygon Shape Point");
					polygon.points[ii] = trs.InverseTransformPoint(point);	
					UpdateMesh(true);
				}
			}
		}

		void DrawPolyLine(List<Vector3> points)
		{
			int vc = points.Count;// + 1;

			Vector3[] ver = new Vector3[vc];
			Vector2[] uvs = new Vector2[vc];
			int[] indices = new int[vc];

			for(int i = 0; i < vc; i++)
			{
				ver[i] = points[i%(points.Count)];
				uvs[i] = new Vector2( i / (float)(vc - 1f), 1f);
				indices[i] = i;
			}

			m_LineMesh.Clear();
			m_LineMesh.name = "Poly Shape Guide";
			m_LineMesh.vertices = ver;
			m_LineMesh.uv = uvs;
			m_LineMesh.SetIndices(indices, MeshTopology.LineStrip, 0);
		}

		void OnEditLevelChange(int editLevel)
		{
			if(polygon.isEditing && ((EditLevel)editLevel) != EditLevel.Plugin)
				polygon.isEditing = false;
		}

		void UndoRedoPerformed()
		{
			UpdateMesh(true);
		}
	}
}
