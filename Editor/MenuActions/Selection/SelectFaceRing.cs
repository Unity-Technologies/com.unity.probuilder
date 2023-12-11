using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectFaceRing : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Toolbar/Selection_Ring_Face";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 2; }
        }

        protected internal override bool hasFileMenuEntry
        {
            get { return false; }
        }

        private static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Face Ring",
                "Selects a ring of connected faces.\n\n<b>Shortcut</b>: Control + Double Click on Face."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            var selection = MeshSelection.topInternal;

            UndoUtility.RecordSelection("Select Face Ring");

            foreach (ProBuilderMesh pb in selection)
            {
                HashSet<Face> loop = ElementSelection.GetFaceLoop(pb, pb.selectedFacesInternal, true);
                pb.SetSelectedFaces(loop);
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Select Face Ring");
        }

        public static ActionResult MenuRingAndLoopFaces(IEnumerable<ProBuilderMesh> selection)
        {
            UndoUtility.RecordSelection(selection.ToArray(), "Select Face Ring and Loop");

            foreach (ProBuilderMesh pb in selection)
            {
                HashSet<Face> loop = ElementSelection.GetFaceRingAndLoop(pb, pb.selectedFacesInternal);
                pb.SetSelectedFaces(loop);
            }

            ProBuilderEditor.Refresh();
            return new ActionResult(ActionResult.Status.Success, "Select Face Ring and Loop");
        }
    }
}
