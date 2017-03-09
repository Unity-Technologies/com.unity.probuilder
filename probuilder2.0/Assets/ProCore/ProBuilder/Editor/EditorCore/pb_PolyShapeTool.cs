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
		[SerializeField] private pb_Renderable m_LineRenderable = null;
		private Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

		private pb_PolyShape polygon { get { return target as pb_PolyShape; } }

		void OnEnable()
		{
			pb_Editor.AddOnEditLevelChangedListener(OnEditLevelChange);
			m_LineRenderable = pb_Renderable.CreateInstance(new Mesh(), m_LineMaterial, polygon.transform);
			pb_MeshRenderer.Add(m_LineRenderable);
			Undo.undoRedoPerformed += UndoRedoPerformed;
			DrawPolyLine(polygon.points);
			EditorApplication.update += Update;
		}

		void OnDisable()
		{
			pb_Editor.RemoveOnEditLevelChangedListener(OnEditLevelChange);
			pb_MeshRenderer.Remove(m_LineRenderable);
			GameObject.DestroyImmediate(m_LineRenderable);
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
				{
					pb_MeshRenderer.Add(m_LineRenderable);
					Tools.current = Tool.None;
				}
				else
				{
					pb_MeshRenderer.Remove(m_LineRenderable);
				}
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
				{
					polygon.isEditing = false;
					pb_MeshRenderer.Remove(m_LineRenderable);
				}

				return;
			}

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

			Mesh m = m_LineRenderable.mesh;

			// Undo/redo destroys and re-inits pb_Renderable, which kills the mesh.
			if(m == null)
			{
				m = new Mesh();
				m_LineRenderable.mesh = m;
			}

			m.Clear();
			m.name = "Poly Shape Guide";
			m.vertices = ver;
			m.uv = uvs;
			m.SetIndices(indices, MeshTopology.LineStrip, 0);
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
