using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	[CustomEditor(typeof(pb_PolyShape))]
	public class pb_PolyShapeTool : Editor
	{
		[SerializeField] private bool m_isEditing = false;

		[SerializeField] private Plane m_Plane = new Plane(Vector3.up, 0f);

		void OnEnable()
		{
			pb_Editor.AddOnEditLevelChangedListener(OnEditLevelChange);
		}

		void OnDisable()
		{
			pb_Editor.RemoveOnEditLevelChangedListener(OnEditLevelChange);
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
					Tools.current = Tool.None;
			}
		}

		void OnSceneGUI()
		{
			pb_PolyShape poly = target as pb_PolyShape;

			if(poly == null || !m_isEditing || Tools.current != Tool.None)
				return;

			Transform trs = poly.transform;

			foreach(Vector3 p in poly.m_Points)
			{
				Vector3 worldPoint = trs.TransformPoint(p);
				float size = HandleUtility.GetHandleSize(worldPoint) * .05f;

				if (Handles.Button(worldPoint, Quaternion.identity, size, size, Handles.DotCap))
				{

				}
			}
		}

		void OnEditLevelChange(int editLevel)
		{
			if(m_isEditing && ((EditLevel)editLevel) != EditLevel.Plugin)
				m_isEditing = false;
		}
	}
}
