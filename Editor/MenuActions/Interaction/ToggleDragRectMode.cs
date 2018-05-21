using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ToggleDragRectMode : MenuAction
	{
		RectSelectMode mode
		{
			get
			{
				return (RectSelectMode) PreferencesInternal.GetInt(PreferenceKeys.pbRectSelectMode,
					(int) RectSelectMode.Partial);
			}

			set
			{
				PreferencesInternal.SetInt(PreferenceKeys.pbRectSelectMode, (int) value);
			}
		}

		public override ToolbarGroup group { get { return ToolbarGroup.Selection; } }

		public override Texture2D icon
		{
			get
			{
				return mode == RectSelectMode.Complete
					? IconUtility.GetIcon("Toolbar/Selection_Rect_Complete")
					: IconUtility.GetIcon("Toolbar/Selection_Rect_Intersect", IconSkin.Pro);
			}
		}
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Set Drag Rect Mode",
			"Sets whether or not a mesh element (edge or face) needs to be completely encompassed by a drag to be selected.\n\nThe default value is Intersect, meaning if any part of the elemnent is touched by the drag rectangle it will be selected."
		);

		public override string menuTitle { get { return mode == RectSelectMode.Complete ? "Rect: Complete" : "Rect: Intersect"; } }

		public override ActionResult DoAction()
		{
			mode = InternalUtility.NextEnumValue(mode);
			ProBuilderEditor.instance.LoadPrefs();
			return new ActionResult(ActionResult.Status.Success,
				"Set Drag Select\n" + (mode == RectSelectMode.Complete ? "Complete" : "Intersect"));
		}

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
			       ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
			       ProBuilderEditor.instance.selectionMode != SelectMode.Vertex;
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
			       	ProBuilderEditor.instance.selectionMode == SelectMode.Vertex;
		}
	}
}
