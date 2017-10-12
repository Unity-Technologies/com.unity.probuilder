using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SelectSmoothingGroup : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_SelectBySmoothingGroup"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select by Smooth",
			"Selects all faces matching the selected smoothing groups."
		);

		GUIContent gc_restrictToSelection = new GUIContent("Current Selection", "Optionally restrict the matches to only those faces on currently selected objects.");

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel != EditLevel.Top &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 0);
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry;
		}

		public override MenuActionState AltState()
		{
			if(	IsEnabled() &&
				pb_Editor.instance.editLevel == EditLevel.Geometry &&
				pb_Editor.instance.selectionMode == SelectMode.Face)
				return MenuActionState.VisibleAndEnabled;

			return MenuActionState.Visible;
		}

		public override void OnSettingsEnable()
		{
			pb_Editor.OnSelectionUpdate += OnElementSelectionChanged;
			OnElementSelectionChanged(selection);
		}

		public override void OnSettingsDisable()
		{
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChanged;
		}

		private string m_SelectedGroups = "";

		private void OnElementSelectionChanged(pb_Object[] selection)
		{
			if(selection != null)
				m_SelectedGroups = new HashSet<int>(selection.SelectMany(x => x.SelectedFaces.Select(y => y.smoothingGroup))).ToString(",");
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Select by Smoothing Group Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			bool restrictToSelection = pb_PreferencesInternal.GetBool("SelectSmoothingGroup::m_RestrictToSelection");
			restrictToSelection = EditorGUILayout.Toggle(gc_restrictToSelection, restrictToSelection);

			if( EditorGUI.EndChangeCheck() )
				pb_PreferencesInternal.SetBool("SelectSmoothingGroup::m_RestrictToSelection", restrictToSelection);

			GUILayout.Label("Currently Selected:", EditorStyles.boldLabel);

			GUILayout.Label(m_SelectedGroups);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Select Faces with Smoothing Groups"))
			{
				DoAction();
				SceneView.RepaintAll();
			}
		}

		public override pb_ActionResult DoAction()
		{
			pbUndo.RecordSelection(selection, "Select Faces with Smoothing Group");

			bool restrictToSelection = pb_PreferencesInternal.GetBool("SelectSmoothingGroup::m_RestrictToSelection");

			HashSet<int> selectedSmoothGroups = new HashSet<int>(selection.SelectMany(x => x.SelectedFaces.Select(y => y.smoothingGroup)));

			List<GameObject> newSelection = new List<GameObject>();

			foreach(pb_Object pb in restrictToSelection ? selection : Object.FindObjectsOfType<pb_Object>())
			{
				IEnumerable<pb_Face> matches = pb.faces.Where(x => selectedSmoothGroups.Contains(x.smoothingGroup));

				if(matches.Count() > 0)
				{
					newSelection.Add(pb.gameObject);
					pb.SetSelectedFaces(matches);
				}
			}

			Selection.objects = newSelection.ToArray();

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Smoothing Group");
		}
	}
}


