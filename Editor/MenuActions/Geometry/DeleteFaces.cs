using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	class DeleteFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Delete", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Delete Faces",
			@"Delete all selected faces.",
			keyCommandDelete
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					editLevel == EditLevel.Geometry &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.selectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuDeleteFace(selection);
		}
	}
}
