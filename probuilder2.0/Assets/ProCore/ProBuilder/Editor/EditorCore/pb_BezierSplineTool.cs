using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_BezierSplineTool : EditorWindow
	{
		Color bezierPositionHandleColor = new Color(.01f, .8f, .99f, 1f);
		Color bezierTangentHandleColor = new Color(.6f, .6f, .6f, .8f);

		List<pb_BezierPoint> m_Points = new List<pb_BezierPoint>();
		bool m_CloseLoop = false;
		float m_Radius = .5f;
		int m_Rows = 24;
		int m_Columns = 32;

		int m_currentIndex = -1;
		pb_BezierTangentMode m_TangentMode = pb_BezierTangentMode.Mirrored;
		pb_Object m_CurrentObject = null;

		public static void MenuOpenBezierSplineTool()
		{
			EditorWindow.GetWindow<pb_BezierSplineTool>(true, "Bezier Spline Editor", true);
		}

		void OnEnable()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			if(GUILayout.Button("Clear Points"))
			{
				m_Points.Clear();
			}

			if(GUILayout.Button("Add Point"))
			{
				if(m_Points.Count > 0)
				{
					m_Points.Add(new pb_BezierPoint(m_Points[m_Points.Count - 1].position,
						m_Points[m_Points.Count - 1].tangentIn,
						m_Points[m_Points.Count - 1].tangentOut));
				}
				else
				{
					Vector3 tan = new Vector3(0f, 0f, 2f);
					Vector3 p1 = new Vector3(3f, 0f, 0f);
					m_Points.Add(new pb_BezierPoint(Vector3.zero, -tan, tan));
					m_Points.Add(new pb_BezierPoint(p1, p1 + tan, p1 + -tan));
				}

				m_currentIndex = m_Points.Count - 1;

				SceneView.RepaintAll();
			}

			m_TangentMode = (pb_BezierTangentMode) EditorGUILayout.EnumPopup("Tangent Mode", m_TangentMode);
			m_CloseLoop = EditorGUILayout.Toggle("Close Loop", m_CloseLoop);
			m_Radius = Mathf.Max(.001f, EditorGUILayout.FloatField("Radius", m_Radius));
			m_Rows = pb_Math.Clamp(EditorGUILayout.IntField("Rows", m_Rows), 3, 512);
			m_Columns = pb_Math.Clamp(EditorGUILayout.IntField("Columns", m_Columns), 3, 512);

			if(EditorGUI.EndChangeCheck())
				UpdateMesh();

		}

		void UpdateMesh()
		{
			pb_Spline.Extrude(m_Points, m_Radius, m_Columns, m_Rows, m_CloseLoop, ref m_CurrentObject);
			pb_Editor.Refresh();
		}

		void OnSceneGUI(SceneView scn)
		{
			int c = m_Points.Count;

			EditorGUI.BeginChangeCheck();

			for(int index = 0; index < c; index++)
			{
				if(index < c -1 || m_CloseLoop)
				{
					Handles.DrawBezier(	m_Points[index].position,
										m_Points[(index+1)%c].position,
										m_Points[index].tangentOut,
										m_Points[(index+1)%c].tangentIn,
										Color.green,
										EditorGUIUtility.whiteTexture,
										1f);
				}

				Handles.BeginGUI();
				Handles.Label(m_Points[index].position, ("index: " + index));
				Handles.EndGUI();

				pb_BezierPoint point = m_Points[index];

				if(m_currentIndex == index)
				{
					Vector3 prev = point.position;
					prev = Handles.PositionHandle(prev, Quaternion.identity);
					if(!pb_Math.Approx3(prev, point.position))
					{
						Vector3 dir = prev - point.position;
						point.position = prev;
						point.tangentIn += dir;
						point.tangentOut += dir;
					}

					Handles.color = bezierTangentHandleColor;

					if(m_CloseLoop || index > 0)
					{
						EditorGUI.BeginChangeCheck();

						point.tangentIn = Handles.PositionHandle(point.tangentIn, Quaternion.identity);
						if(EditorGUI.EndChangeCheck())
							point.EnforceTangentMode(pb_BezierTangentDirection.In, m_TangentMode);
						Handles.color = Color.blue;
						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);
					}
						
					if(m_CloseLoop || index < c - 1)
					{
						EditorGUI.BeginChangeCheck();
						point.tangentOut = Handles.PositionHandle(point.tangentOut, Quaternion.identity);
						if(EditorGUI.EndChangeCheck())
							point.EnforceTangentMode(pb_BezierTangentDirection.Out, m_TangentMode);
						Handles.color = Color.red;
						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentOut);
					}

					m_Points[index] = point;
				}
				else
				{
					float size = HandleUtility.GetHandleSize(m_Points[index].position) * .05f;

					Handles.color = bezierPositionHandleColor;

					if (Handles.Button(m_Points[index].position, Quaternion.identity, size, size, Handles.DotCap))
						m_currentIndex = index;

					Handles.color = bezierTangentHandleColor;
					if(m_CloseLoop || index > 0)
						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);
					if(m_CloseLoop || index < c - 1)
						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentOut);
					Handles.color = Color.white;
				}
			}

			if( EditorGUI.EndChangeCheck() )
				UpdateMesh();
		}
	}
}
