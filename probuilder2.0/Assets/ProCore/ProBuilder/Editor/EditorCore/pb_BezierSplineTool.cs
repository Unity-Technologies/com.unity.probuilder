using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	[CustomEditor(typeof(pb_BezierShape))]
	public class pb_BezierSplineTool : Editor
	{
		static GUIContent[] m_TangentModeIcons = new GUIContent[]
		{
			new GUIContent("Free"),
			new GUIContent("Aligned"),
			new GUIContent("Mirrored")
		};

		private static Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);
		Color bezierPositionHandleColor = new Color(.01f, .8f, .99f, 1f);
		Color bezierTangentHandleColor = new Color(.6f, .6f, .6f, .8f);

		BezierHandle m_currentHandle = new BezierHandle(-1, false);
		pb_BezierTangentMode m_TangentMode = pb_BezierTangentMode.Mirrored;

		pb_BezierShape m_Target = null;

		pb_Object m_CurrentObject
		{
			get
			{
				if(m_Target.mesh == null)
				{
					m_Target.mesh = m_Target.gameObject.AddComponent<pb_Object>();
					pb_EditorUtility.InitObject(m_Target.mesh);

				}

				return m_Target.mesh;
			}
		}

		struct BezierHandle
		{
			public int index;
			public bool isTangent;
			public pb_BezierTangentDirection tangent;

			public BezierHandle(int index, bool isTangent, pb_BezierTangentDirection tangent = pb_BezierTangentDirection.In)
			{
				this.index = index;
				this.isTangent = isTangent;
				this.tangent = tangent;
			}

			public static implicit operator int(BezierHandle handle)
			{
				return handle.index;
			}

			public static explicit operator BezierHandle(int index)
			{
				return new BezierHandle(index, false);
			}

			public static implicit operator pb_BezierTangentDirection(BezierHandle handle)
			{
				return handle.tangent;
			}
			
			public void SetIndex(int index)
			{
				this.index = index;
				this.isTangent = false;
			}

			public void SetIndexAndTangent(int index, pb_BezierTangentDirection dir)
			{
				this.index = index;
				this.isTangent = true;
				this.tangent = dir;
			}
		}

		List<pb_BezierPoint> m_Points { get { return m_Target.m_Points; } set { m_Target.m_Points = value; } }
		bool m_CloseLoop { get { return m_Target.m_CloseLoop; } set { m_Target.m_CloseLoop = value; } }
		float m_Radius { get { return m_Target.m_Radius; } set { m_Target.m_Radius = value; } }
		int m_Rows { get { return m_Target.m_Rows; } set { m_Target.m_Rows = value; } }
		int m_Columns { get { return m_Target.m_Columns; } set { m_Target.m_Columns = value; } }

		private GUIStyle _commandStyle = null;
		public GUIStyle commandStyle
		{
			get
			{
				if(_commandStyle == null)
				{
					_commandStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command"));
					_commandStyle.alignment = TextAnchor.MiddleCenter;
				}

				return _commandStyle;
			}
		}

		void OnEnable()
		{
			m_Target = target as pb_BezierShape;
		}

		void OnDisable()
		{
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			bool handleIsValid = (m_currentHandle > -1 && m_currentHandle < m_Points.Count);

			pb_BezierPoint inspectorPoint = handleIsValid ? 
				m_Points[m_currentHandle] :
				new pb_BezierPoint(Vector3.zero, -Vector3.forward, Vector3.forward);

			inspectorPoint.position = EditorGUILayout.Vector3Field("Position", inspectorPoint.position);
			inspectorPoint.tangentIn = EditorGUILayout.Vector3Field("Tangent In", inspectorPoint.tangentIn);
			inspectorPoint.tangentOut = EditorGUILayout.Vector3Field("Tangent Out", inspectorPoint.tangentOut);

			if(handleIsValid)
				m_Points[m_currentHandle] = inspectorPoint;

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
					m_Target.Init();
				}

				m_currentHandle = (BezierHandle) (m_Points.Count - 1);

				SceneView.RepaintAll();
			}

			m_TangentMode = (pb_BezierTangentMode) GUILayout.Toolbar((int)m_TangentMode, m_TangentModeIcons, commandStyle);
			m_CloseLoop = EditorGUILayout.Toggle("Close Loop", m_CloseLoop);
			m_Radius = Mathf.Max(.001f, EditorGUILayout.FloatField("Radius", m_Radius));
			m_Rows = pb_Math.Clamp(EditorGUILayout.IntField("Rows", m_Rows), 3, 512);
			m_Columns = pb_Math.Clamp(EditorGUILayout.IntField("Columns", m_Columns), 3, 512);

			if(EditorGUI.EndChangeCheck())
				UpdateMesh(true);
		}

		void UpdateMesh(bool vertexCountChanged)
		{
			m_Target.Refresh();

			if(!vertexCountChanged)
				pb_Editor.instance.Internal_UpdateSelectionFast();
			else
				pb_Editor.Refresh();
		}

		void OnSceneGUI()
		{
			int c = m_Points.Count;

			Matrix4x4 handleMatrix = Handles.matrix;
			Handles.matrix = m_Target.transform.localToWorldMatrix;

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

				// Handles.BeginGUI();
				// Handles.Label(m_Points[index].position, ("index: " + index));
				// Handles.EndGUI();

				pb_BezierPoint point = m_Points[index];

				if(m_currentHandle == index)
				{
					if(!m_currentHandle.isTangent)
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
					}
					else
					{
						Handles.color = bezierTangentHandleColor;

						if(m_currentHandle.tangent == pb_BezierTangentDirection.In && (m_CloseLoop || index > 0))
						{
							EditorGUI.BeginChangeCheck();
							point.tangentIn = Handles.PositionHandle(point.tangentIn, Quaternion.identity);
							if(EditorGUI.EndChangeCheck())
								point.EnforceTangentMode(pb_BezierTangentDirection.In, m_TangentMode);
							Handles.color = Color.blue;
							Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);
						}
							
						if(m_currentHandle.tangent == pb_BezierTangentDirection.Out && (m_CloseLoop || index < c - 1))
						{
							EditorGUI.BeginChangeCheck();
							point.tangentOut = Handles.PositionHandle(point.tangentOut, Quaternion.identity);
							if(EditorGUI.EndChangeCheck())
								point.EnforceTangentMode(pb_BezierTangentDirection.Out, m_TangentMode);
							Handles.color = Color.red;
							Handles.DrawLine(m_Points[index].position, m_Points[index].tangentOut);
						}
					}

					m_Points[index] = point;
				}

				// buttons
				{
					float size = HandleUtility.GetHandleSize(m_Points[index].position) * .05f;

					Handles.color = bezierPositionHandleColor;

					if (Handles.Button(m_Points[index].position, Quaternion.identity, size, size, Handles.DotCap))
					{
						m_currentHandle = (BezierHandle) index;
						Repaint();
					}

					Handles.color = bezierTangentHandleColor;

					if(m_CloseLoop || index > 0)
					{
						size = HandleUtility.GetHandleSize(m_Points[index].tangentIn) * .05f;

						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);

						if (Handles.Button(m_Points[index].tangentIn, Quaternion.identity, size, size, Handles.DotCap))
						{
							m_currentHandle.SetIndexAndTangent(index, pb_BezierTangentDirection.In);
							Repaint();
						}
					}

					if(m_CloseLoop || index < c - 1)
					{
						size = HandleUtility.GetHandleSize(m_Points[index].tangentOut) * .05f;

						Handles.DrawLine(m_Points[index].position, m_Points[index].tangentOut);

						if (Handles.Button(m_Points[index].tangentOut, Quaternion.identity, size, size, Handles.DotCap))
						{
							m_currentHandle.SetIndexAndTangent(index, pb_BezierTangentDirection.Out);
							Repaint();
						}
					}

					Handles.color = Color.white;
				}
			}

			Handles.matrix = handleMatrix;

			if( EditorGUI.EndChangeCheck() )
				UpdateMesh(false);
		}
	}
}
