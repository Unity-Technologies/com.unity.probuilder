using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class GrowSelection : MenuAction
	{
		Pref<bool> m_GrowSelectionWithAngle = new Pref<bool>("GrowSelection.useAngle", false);
		Pref<bool> m_GrowSelectionAngleIterative = new Pref<bool>("GrowSelection.iterativeGrow", true);
		Pref<float> m_GrowSelectionAngleValue = new Pref<float>("GrowSelection.angleValue", 15f);

		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Grow", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Grow Selection",
			@"Adds adjacent elements to the current selection, optionally testing to see if they are within a specified angle.

Grow by angle is enabled by Option + Clicking the <b>Grow Selection</b> button.",
			keyCommandAlt, 'G'
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MenuCommands.VerifyGrowSelection(MeshSelection.TopInternal());
			}
		}

		public override bool hidden
		{
			get { return editLevel != EditLevel.Geometry; }
		}

		protected override MenuActionState optionsMenuState
		{
			get
			{
				if (enabled &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Face)
					return MenuActionState.VisibleAndEnabled;

				return MenuActionState.Hidden;
			}
		}

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Grow Selection Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			m_GrowSelectionWithAngle.value = EditorGUILayout.Toggle("Restrict to Angle", m_GrowSelectionWithAngle);

			GUI.enabled = m_GrowSelectionWithAngle;

			m_GrowSelectionAngleValue.value = EditorGUILayout.FloatField("Max Angle", m_GrowSelectionAngleValue);

			GUI.enabled = m_GrowSelectionWithAngle;

			bool iterative = m_GrowSelectionWithAngle ? m_GrowSelectionAngleIterative : true;

			m_GrowSelectionAngleIterative.value = EditorGUILayout.Toggle("Iterative", iterative);

			GUI.enabled = true;

			if (EditorGUI.EndChangeCheck())
				Settings.Save();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Grow Selection"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			var selection = MeshSelection.TopInternal();
			var editor = ProBuilderEditor.instance;

			if(!ProBuilderEditor.instance || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Grow Selection");

			int grown = 0;
			bool angleGrow = m_GrowSelectionWithAngle;
			bool iterative = m_GrowSelectionAngleIterative;
			float growSelectionAngle = m_GrowSelectionAngleValue;

			if(!angleGrow && !iterative)
				iterative = true;

			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				int previousTriCount = pb.selectedVertexCount;

				switch( editor != null ? ProBuilderEditor.componentMode : (ComponentMode)0 )
				{
					case ComponentMode.Vertex:
						pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndexesInternal));
						break;

					case ComponentMode.Edge:
						pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndexesInternal));
						break;

					case ComponentMode.Face:

						Face[] selectedFaces = pb.GetSelectedFaces();

						HashSet<Face> sel;

						if(iterative)
						{
							sel = ElementSelection.GrowSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
							sel.UnionWith(selectedFaces);
						}
						else
						{
							sel = ElementSelection.FloodSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
						}

						pb.SetSelectedFaces( sel.ToArray() );

						break;
				}

				grown += pb.selectedVertexCount - previousTriCount;
			}

			ProBuilderEditor.Refresh();
			SceneView.RepaintAll();

			if(grown > 0)
				return new ActionResult(ActionResult.Status.Success, "Grow Selection");

			return new ActionResult(ActionResult.Status.Failure, "Nothing to Grow");
		}
	}
}
