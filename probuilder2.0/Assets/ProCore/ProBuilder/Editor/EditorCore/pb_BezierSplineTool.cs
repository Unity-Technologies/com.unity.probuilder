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
		static GUIContent[] m_TangentModeIcons = new GUIContent[3];

		private const float HANDLE_SIZE = .05f;

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

			m_TangentModeIcons[0] = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Bezier_Free.png"), "Tangent Mode: Free");
			m_TangentModeIcons[1] = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Bezier_Aligned.png"), "Tangent Mode: Aligned");
			m_TangentModeIcons[2] = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Bezier_Mirrored.png"), "Tangent Mode: Mirrored");
		}

		void OnDisable()
		{
			Undo.undoRedoPerformed -= this.UndoRedoPerformed;
		}

		pb_BezierPoint DoBezierPointGUI(pb_BezierPoint point)
		{
			Vector3 pos = point.position, tin = point.tangentIn, tout = point.tangentOut;

			bool wasInWideMode = EditorGUIUtility.wideMode;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 3f;

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

			Vector3 euler = point.rotation.eulerAngles;
			euler = EditorGUILayout.Vector3Field("Rotation", euler);
			point.rotation = Quaternion.Euler(euler);

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

			GUILayout.BeginHorizontal();
			m_TangentMode = (pb_BezierTangentMode) GUILayout.Toolbar((int)m_TangentMode, m_TangentModeIcons, commandStyle);
			GUILayout.EndHorizontal();

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

				if(pb_Editor.instance != null)
				{
					if(!vertexCountChanged)
						pb_Editor.instance.Internal_UpdateSelectionFast();
					else
						pb_Editor.Refresh();
				}
			}
		}

		void OnSceneGUI()
		{
			Event e = Event.current;
			bool eventHasBeenUsed = false;

			if(m_IsMoving)
			{
				if(	e.type == EventType.Ignore ||
					e.type == EventType.MouseUp )
				{
					eventHasBeenUsed = true;
					OnFinishVertexModification();
				}
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

			int count = m_Points.Count;

			Matrix4x4 handleMatrix = Handles.matrix;
			Handles.matrix = m_Target.transform.localToWorldMatrix;

			EditorGUI.BeginChangeCheck();

			for(int index = 0; index < count; index++)
			{
				if(index < count -1 || m_CloseLoop)
				{
					Handles.DrawBezier(	m_Points[index].position,
										m_Points[(index+1)%count].position,
										m_Points[index].tangentOut,
										m_Points[(index+1)%count].tangentIn,
										Color.green,
										EditorGUIUtility.whiteTexture,
										1f);
				}

				// If the index is selected show the full transform gizmo, otherwise use free move handles
				if(m_currentHandle == index)
				{
					pb_BezierPoint point = m_Points[index];

					if(!m_currentHandle.isTangent)
					{
						Vector3 prev = point.position;
						prev = Handles.PositionHandle(prev, Quaternion.identity);
						if(!pb_Math.Approx3(prev, point.position))
						{
							if(!m_IsMoving)
							{
								if(e.shift)
								{
									pbUndo.RecordObject(m_Target, "Add Point");
									pb_BezierPoint dup = point;
									m_Points.Insert(m_currentHandle.index, dup);
								}

								OnBeginVertexModification();
							}

							Vector3 dir = prev - point.position;
							point.position = prev;
							point.tangentIn += dir;
							point.tangentOut += dir;
						}

						// rotation
						int prev_index = index > 0 ? index - 1 : (m_CloseLoop ? count - 1 : -1);
						int next_index = index < count - 1 ? index + 1: (m_CloseLoop ? 0 : -1);
						Vector3 rd = pb_BezierPoint.GetLookDirection(m_Points, index, prev_index, next_index);

						Quaternion look = Quaternion.LookRotation(rd);
						float size = HandleUtility.GetHandleSize(point.position);
						Matrix4x4 pm = Handles.matrix;
						Handles.matrix = pm * Matrix4x4.TRS(point.position, look, Vector3.one);
						point.rotation = Handles.Disc(point.rotation, Vector3.zero, Vector3.forward, size, false, 0f);
						Handles.matrix = pm;
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

						if(m_currentHandle.tangent == pb_BezierTangentDirection.Out && (m_CloseLoop || index < count - 1))
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
			}

			EventType eventType = e.type;

			if(!eventHasBeenUsed)
				eventHasBeenUsed = eventType == EventType.Used;

			for(int index = 0; index < count; index++)
			{
				Vector3 prev;
				pb_BezierPoint point = m_Points[index];

				// Position Handle
				float size = HandleUtility.GetHandleSize(point.position) * HANDLE_SIZE;
				Handles.color = bezierPositionHandleColor;

				if(m_currentHandle == index && !m_currentHandle.isTangent)
				{
					Handles.DotCap(0, point.position, Quaternion.identity, size);
				}
				else
				{
					prev = point.position;
					prev = Handles.FreeMoveHandle(prev, Quaternion.identity, size, Vector3.zero, Handles.DotCap);
					if(!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
					{
						eventHasBeenUsed = true;
						m_currentHandle = (BezierHandle) index;
						Repaint();
					}
					else if(!pb_Math.Approx3(prev, point.position))
					{
						if(!m_IsMoving)
							OnBeginVertexModification();

						point.SetPosition(prev);
					}
				}

				// Tangent handles
				Handles.color = bezierTangentHandleColor;

				// Tangent In Handle
				if(m_CloseLoop || index > 0)
				{
					size = HandleUtility.GetHandleSize(point.tangentIn) * HANDLE_SIZE;
					Handles.DrawLine(point.position, point.tangentIn);

					if(index == m_currentHandle && m_currentHandle.isTangent && m_currentHandle.tangent == pb_BezierTangentDirection.In)
					{
						Handles.DotCap(0, point.tangentIn, Quaternion.identity, size);
					}
					else
					{
						prev = point.tangentIn;
						prev = Handles.FreeMoveHandle(prev, Quaternion.identity, size, Vector3.zero, Handles.DotCap);

						if(!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
						{
							eventHasBeenUsed = true;
							m_currentHandle.SetIndexAndTangent(index, pb_BezierTangentDirection.In);
							Repaint();
						}
						else if(!pb_Math.Approx3(prev, point.tangentIn))
						{
							if(!m_IsMoving)
								OnBeginVertexModification();
							point.tangentIn = prev;
							point.EnforceTangentMode(pb_BezierTangentDirection.In, m_TangentMode);
						}
					}
				}

				// Tangent Out
				if(m_CloseLoop || index < count - 1)
				{
					size = HandleUtility.GetHandleSize(point.tangentOut) * HANDLE_SIZE;
					Handles.DrawLine(point.position, point.tangentOut);

					if(index == m_currentHandle && m_currentHandle.isTangent && m_currentHandle.tangent == pb_BezierTangentDirection.Out)
					{
						Handles.DotCap(0, point.tangentOut, Quaternion.identity, size);
					}
					else
					{
						prev = point.tangentOut;
						prev = Handles.FreeMoveHandle(prev, Quaternion.identity, size, Vector3.zero, Handles.DotCap);

						if(!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
						{
							eventHasBeenUsed = true;
							m_currentHandle.SetIndexAndTangent(index, pb_BezierTangentDirection.Out);
							Repaint();
						}
						else if(!pb_Math.Approx3(prev, point.tangentOut))
						{
							if(!m_IsMoving)
								OnBeginVertexModification();
							point.tangentOut = prev;
							point.EnforceTangentMode(pb_BezierTangentDirection.Out, m_TangentMode);
						}
					}
				}

				m_Points[index] = point;
			}

			Handles.matrix = handleMatrix;

			if( EditorGUI.EndChangeCheck() )
				UpdateMesh(false);
		}

		void UndoRedoPerformed()
		{
			UpdateMesh(true);

			if(m_Target && m_CurrentObject)
			{
				m_CurrentObject.ToMesh();
				m_CurrentObject.Refresh();
				m_CurrentObject.Optimize();
			}
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
			pb_Editor.Refresh();
		}
	}
}
