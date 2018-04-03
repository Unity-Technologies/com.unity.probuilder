using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SelectMaterial : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_SelectByMaterial", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select by Material",
			"Selects all faces matching the selected materials."
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

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Select Material Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			bool restrictToSelection = pb_PreferencesInternal.GetBool("pb_restrictSelectMaterialToCurrentSelection");
			restrictToSelection = EditorGUILayout.Toggle(gc_restrictToSelection, restrictToSelection);

			if( EditorGUI.EndChangeCheck() )
				pb_PreferencesInternal.SetBool("pb_restrictSelectMaterialToCurrentSelection", restrictToSelection);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Select Faces with Material"))
			{
				DoAction();
				SceneView.RepaintAll();
			}
		}

		public override pb_ActionResult DoAction()
		{
			pb_Undo.RecordSelection(selection, "Select Faces with Material");

			bool restrictToSelection = pb_PreferencesInternal.GetBool("pb_restrictSelectMaterialToCurrentSelection");

			HashSet<Material> sel = new HashSet<Material>(selection.SelectMany(x => x.SelectedFaces.Select(y => y.material).Where( z => z != null)));
			List<GameObject> newSelection = new List<GameObject>();

			foreach(pb_Object pb in restrictToSelection ? selection : Object.FindObjectsOfType<pb_Object>())
			{
				IEnumerable<pb_Face> matches = pb.faces.Where(x => sel.Contains(x.material));

				if(matches.Count() > 0)
				{
					newSelection.Add(pb.gameObject);
					pb.SetSelectedFaces(matches);
				}
			}

			Selection.objects = newSelection.ToArray();

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Material");
		}
	}
}


