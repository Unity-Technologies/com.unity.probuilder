using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_BezierSplineTool : EditorWindow
	{
		List<pb_BezierPoint> points = new List<pb_BezierPoint>();
		bool m_CompleteLoop = false;

		int m_currentIndex = -1;

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
			if(GUILayout.Button("Add Point"))
			{
				if(points.Count > 0)
					points.Add(new pb_BezierPoint(points[points.Count - 1].position + Vector3.right, points[points.Count - 1].tangent));
				else
					points.Add(new pb_BezierPoint(Vector3.zero, Vector3.right));

				SceneView.RepaintAll();
			}

			if(GUILayout.Button("Extrude", GUILayout.MinHeight(32)))
			{
				pb_Spline.Extrude(points);
			}
		}

		void OnSceneGUI(SceneView scn)
		{
			int c = points.Count;

			for(int i = 0; i < c; i++)
			{
				// public static void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color color, Texture2D texture, float width);

				if(i < c -1 || m_CompleteLoop)
				{
					Handles.DrawBezier(	points[i].position,
										points[(i+1)%c].position,
										points[i].tangent,
										points[(i+1)%c].tangent,
										Color.green,
										EditorGUIUtility.whiteTexture,
										1f);
				}

				pb_BezierPoint point = points[i];

				if(m_currentIndex == i)
				{
					point.position = Handles.PositionHandle(point.position, Quaternion.identity);
					point.tangent = Handles.PositionHandle(point.tangent, Quaternion.identity);

					Handles.color = new Color(.3f, .3f, .3f, .8f);
					Handles.DrawLine(points[i].position, points[i].tangent);
					Handles.color = Color.white;

					points[i] = point;
				}
				else
				{
					float size = HandleUtility.GetHandleSize(points[i].position) * .05f;

					Handles.color = new Color(.01f, .8f, .99f, 1f);
					if (Handles.Button(points[i].position, Quaternion.identity, size, size, Handles.DotCap))
						m_currentIndex = i;

					Handles.color = new Color(.3f, .3f, .3f, .8f);
					Handles.DrawLine(points[i].position, points[i].tangent);
					Handles.color = Color.white;
				}
			}
		}
	}
}
