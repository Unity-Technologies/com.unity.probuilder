using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class DeleteFaces : MenuAction
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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					editLevel == EditLevel.Geometry &&
					MeshSelection.TopInternal().Sum(x => x.selectedFaceCount) > 0;
			}
		}

		public override bool hidden
		{
			get
			{
				return editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted) && componentMode != ComponentMode.Face);
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuDeleteFace(MeshSelection.TopInternal());
		}
	}
}
