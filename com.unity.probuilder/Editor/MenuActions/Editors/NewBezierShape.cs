using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class NewBezierShape : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/NewBezierSpline", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Bezier Shape"; } }
		public override int toolbarPriority { get { return 1; } }
		private bool m_ExperimentalFeaturesEnabled = false;

		public NewBezierShape()
		{
			m_ExperimentalFeaturesEnabled = PreferencesInternal.GetBool(PreferenceKeys.pbEnableExperimental);
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"New Bezier Shape",
			"Creates a new shape that is built by extruding along a bezier spline."
		);

		public override bool hidden
		{
			get { return !m_ExperimentalFeaturesEnabled; }
		}

		public override bool enabled
		{
			get { return true; }
		}

		public override ActionResult DoAction()
		{
			GameObject go = new GameObject();
			var bezier = go.AddComponent<BezierShape>();
			go.GetComponent<MeshRenderer>().sharedMaterial = PreferencesInternal.GetMaterial(PreferenceKeys.pbDefaultMaterial);
			bezier.Init();
			bezier.Refresh();
			EditorUtility.InitObject(bezier.GetComponent<ProBuilderMesh>());
			MeshSelection.SetSelection(go);
			UndoUtility.RegisterCreatedObjectUndo(go, "Create Bezier Shape");
			bezier.isEditing = true;

			return new ActionResult(ActionResult.Status.Success, "Create Bezier Shape");
		}
	}
}
