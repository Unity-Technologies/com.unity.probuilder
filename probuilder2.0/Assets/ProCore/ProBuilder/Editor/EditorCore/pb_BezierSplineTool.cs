using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_BezierSplineTool : EditorWindow
	{
		List<Vector3> points = new List<Vector3>();
		bool completeLoop = false;

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
				points.Add(Vector3.zero);

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
				points[i] = Handles.PositionHandle(points[i], Quaternion.identity);

				if(i < c -1 || completeLoop)
					Handles.DrawLine(points[i], points[(i+1)%c]);
			}
		}
	}
}
