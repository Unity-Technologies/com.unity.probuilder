using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	[CustomEditor(typeof(pb_PolyShape))]
	public class pb_PolyShapeTool : Editor
	{
		[SerializeField] private bool m_isEditing = false;
		[SerializeField] private Plane m_Plane = new Plane(Vector3.up, 0f);
		[SerializeField] private Material m_LineMaterial;
		[SerializeField] private pb_Renderable m_LineRenderable = null;

		private pb_PolyShape polygon { get { return target as pb_PolyShape; } }

		void OnEnable()
		{
			pb_Editor.AddOnEditLevelChangedListener(OnEditLevelChange);
			m_LineRenderable = pb_Renderable.CreateInstance(new Mesh(), m_LineMaterial, polygon.transform);
			m_Plane = new Plane(Vector3.up, polygon.transform.position);
			EditorApplication.update += Update;
		}

		void OnDisable()
		{
			pb_Editor.RemoveOnEditLevelChangedListener(OnEditLevelChange);
			pb_MeshRenderer.Remove(m_LineRenderable);
			GameObject.DestroyImmediate(m_LineRenderable);
			EditorApplication.update -= Update;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			m_isEditing = EditorGUILayout.Toggle("Edit Mode", m_isEditing);

			if(EditorGUI.EndChangeCheck())
			{
				if(pb_Editor.instance != null)
					pb_Editor.instance.SetEditLevel(EditLevel.Plugin);

				if(m_isEditing)
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

		void OnSceneGUI()
		{
			if(polygon == null || !m_isEditing || Tools.current != Tool.None)
			{
				if(m_isEditing)
				{
					m_isEditing = false;
					pb_MeshRenderer.Remove(m_LineRenderable);
				}

				return;
			}

			DoExistingPointsGUI();

			DrawPolyLine(polygon.m_Points);

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

			if( m_Plane.Raycast(ray, out hitDistance) )
			{
				polygon.m_Points.Add(polygon.transform.InverseTransformPoint(ray.GetPoint(hitDistance)));
			}
		}

		void DoExistingPointsGUI()
		{
			Transform trs = polygon.transform;
			int len = polygon.m_Points.Count;

			for(int ii = 0; ii < len; ii++)
			{
				Vector3 point = trs.TransformPoint(polygon.m_Points[ii]);

				float size = HandleUtility.GetHandleSize(point) * .05f;

				if (Handles.Button(point, Quaternion.identity, size, size, Handles.DotCap))
				{

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
			m.Clear();
			m.vertices = ver;
			m.uv = uvs;
			m.SetIndices(indices, MeshTopology.LineStrip, 0);
		}

		void OnEditLevelChange(int editLevel)
		{
			if(m_isEditing && ((EditLevel)editLevel) != EditLevel.Plugin)
				m_isEditing = false;
		}
	}
}
