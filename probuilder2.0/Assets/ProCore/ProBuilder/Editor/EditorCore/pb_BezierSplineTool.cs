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
		private static Vector3 Vector3_Forward = new Vector3(0f, 0f, 1f);
		private static Vector3 Vector3_Backward = new Vector3(0f, 0f, -1f);

		private static Color bezierPositionHandleColor = new Color(.01f, .8f, .99f, 1f);
		private static Color bezierTangentHandleColor = new Color(.6f, .6f, .6f, .8f);

		[SerializeField] BezierHandle m_currentHandle = new BezierHandle(-1, false);
		[SerializeField] pb_BezierTangentMode m_TangentMode = pb_BezierTangentMode.Mirrored;
		pb_BezierShape m_Target = null;
		bool m_IsMoving = false;

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

		[System.Serializable]
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

		bool m_CloseLoop
		{
			get { return m_Target.m_CloseLoop; }

			set {
				if(m_Target.m_CloseLoop != value)
					pbUndo.RecordObject(m_Target, "Set Bezier Shape Close Loop");
				m_Target.m_CloseLoop = value;
			}
		}

		float m_Radius
		{
			get { return m_Target.m_Radius; }

			set {
				if(m_Target.m_Radius != value)
					pbUndo.RecordObject(m_Target, "Set Bezier Shape Radius");
				m_Target.m_Radius = value;
			}
		}

		int m_Rows
		{
			get { return m_Target.m_Rows; }

			set {
				if(m_Target.m_Rows != value)
					pbUndo.RecordObject(m_Target, "Set Bezier Shape Rows");
				m_Target.m_Rows = value;
			}
		}

		int m_Columns
		{
			get { return m_Target.m_Columns; }

			set {
				if(m_Target.m_Columns != value)
					pbUndo.RecordObject(m_Target, "Set Bezier Shape Columns");
				m_Target.m_Columns = value;
			}
		}

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
			Undo.undoRedoPerformed += this.UndoRedoPerformed;
		}

		void OnDisable()
		{
		}

		pb_BezierPoint DoBezierPointGUI(pb_BezierPoint point)
		{
			Vector3 pos = point.position, tin = point.tangentIn, tout = point.tangentOut;

			bool wasInWideMode = EditorGUIUtility.wideMode;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = Screen.width / 3f;

			EditorGUI.BeginChangeCheck();
			pos = EditorGUILayout.Vector3Field("Position", pos);
			if( EditorGUI.EndChangeCheck() )
				point.SetPosition(pos);

			EditorGUI.BeginChangeCheck();
			tin = EditorGUILayout.Vector3Field("Tan. In", tin);
			if( EditorGUI.EndChangeCheck() )
				point.SetTangentIn(tin, m_TangentMode);
			Rect r = GUILayoutUtility.GetLastRect();
			r.x += EditorGUIUtility.labelWidth - 12;
			GUI.color = Color.blue;
			GUI.Label(r, "\u2022");
			GUI.color = Color.white;

			EditorGUI.BeginChangeCheck();
			tout = EditorGUILayout.Vector3Field("Tan. Out", tout);
			if( EditorGUI.EndChangeCheck() )
				point.SetTangentOut(tout, m_TangentMode);
			r = GUILayoutUtility.GetLastRect();
			r.x += EditorGUIUtility.labelWidth - 12;
			GUI.color = Color.red;
			GUI.Label(r, "\u2022");
			GUI.color = Color.white;

			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.wideMode = wasInWideMode;

			return point;
		}

		public override void OnInspectorGUI()
		{
			Event e = Event.current;

			if(m_IsMoving)
			{
				if(	e.type == EventType.Ignore ||
					e.type == EventType.MouseUp )
					OnFinishVertexModification();
			}

			EditorGUI.BeginChangeCheck();

			bool handleIsValid = (m_currentHandle > -1 && m_currentHandle < m_Points.Count);

			pb_BezierPoint inspectorPoint = handleIsValid ? 
				m_Points[m_currentHandle] :
				new pb_BezierPoint(Vector3_Zero, Vector3_Backward, Vector3_Forward);

			inspectorPoint = DoBezierPointGUI(inspectorPoint);

			if(handleIsValid && EditorGUI.EndChangeCheck())
			{
				if(!m_IsMoving)
					OnBeginVertexModification();

				m_Points[m_currentHandle] = inspectorPoint;
				UpdateMesh(false);
			}

			EditorGUI.BeginChangeCheck();

			if(GUILayout.Button("Clear Points"))
			{
				pbUndo.RecordObject(m_Target, "Clear Bezier Spline Points");
				m_Points.Clear();
			}

			if(GUILayout.Button("Add Point"))
			{
				pbUndo.RecordObject(m_Target, "Add Bezier Spline Point");

				if(m_Points.Count > 0)
				{
					m_Points.Add(new pb_BezierPoint(m_Points[m_Points.Count - 1].position,
						m_Points[m_Points.Count - 1].tangentIn,
						m_Points[m_Points.Count - 1].tangentOut));
					UpdateMesh(true);
				}
				else
				{
					m_Target.Init();
				}

				m_currentHandle = (BezierHandle) (m_Points.Count - 1);

				SceneView.RepaintAll();
			}

			m_TangentMode 	= (pb_BezierTangentMode) GUILayout.Toolbar((int)m_TangentMode, m_TangentModeIcons, commandStyle);

			m_CloseLoop = EditorGUILayout.Toggle("Close Loop", m_CloseLoop);
			m_Radius = Mathf.Max(.001f, EditorGUILayout.FloatField("Radius", m_Radius));
			m_Rows = pb_Math.Clamp(EditorGUILayout.IntField("Rows", m_Rows), 3, 512);
			m_Columns = pb_Math.Clamp(EditorGUILayout.IntField("Columns", m_Columns), 3, 512);

			if(EditorGUI.EndChangeCheck())
				UpdateMesh(true);
		}

		void UpdateMesh(bool vertexCountChanged)
		{
			if(m_Target != null)
			{
				m_Target.Refresh();

				if(!vertexCountChanged)
					pb_Editor.instance.Internal_UpdateSelectionFast();
				else
					pb_Editor.Refresh();
			}
		}

		void OnSceneGUI()
		{
			Event e = Event.current;

			if(m_IsMoving)
			{
				if(	e.type == EventType.Ignore ||
					e.type == EventType.MouseUp )
					OnFinishVertexModification();
			}

			if(e.type == EventType.KeyDown)
			{
				if(e.keyCode == KeyCode.Backspace && m_currentHandle > -1 && m_currentHandle < m_Points.Count)
				{
					pbUndo.RecordObject(m_Target, "Delete Bezier Point");
					m_Points.RemoveAt(m_currentHandle);
					UpdateMesh(true);
				}
			}

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
							if(!m_IsMoving)
								OnBeginVertexModification();

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
							{
								if(!m_IsMoving)
									OnBeginVertexModification();

								point.EnforceTangentMode(pb_BezierTangentDirection.In, m_TangentMode);
							}
							Handles.color = Color.blue;
							Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);
						}
							
						if(m_currentHandle.tangent == pb_BezierTangentDirection.Out && (m_CloseLoop || index < c - 1))
						{
							EditorGUI.BeginChangeCheck();
							point.tangentOut = Handles.PositionHandle(point.tangentOut, Quaternion.identity);
							if(EditorGUI.EndChangeCheck())
							{
								if(!m_IsMoving)
									OnBeginVertexModification();

								point.EnforceTangentMode(pb_BezierTangentDirection.Out, m_TangentMode);
							}
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
		
		void UndoRedoPerformed()
		{
			UpdateMesh(true);
		}

		void OnBeginVertexModification()
		{
			m_IsMoving = true;
			pbUndo.RecordObject(m_Target, "Modify Bezier Spline");
			pb_Lightmapping.PushGIWorkflowMode();
		}

		void OnFinishVertexModification()
		{
			m_IsMoving = false;
			pb_Lightmapping.PopGIWorkflowMode();
			m_CurrentObject.ToMesh();
			m_CurrentObject.Refresh();
			m_CurrentObject.Optimize();
		}
	}
}
