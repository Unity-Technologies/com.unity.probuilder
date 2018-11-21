using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectFaceLoop : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_Loop_Face", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        private static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Face Loop",
                "Selects a loop of connected faces.\n\n<b>Shortcut</b>: Shift + Double Click on Face."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        public override ActionResult DoAction()
        {
            var selection = MeshSelection.topInternal;

            UndoUtility.RecordSelection("Select Face Loop");

            foreach (ProBuilderMesh pb in selection)
            {
                HashSet<Face> loop = ElementSelection.GetFaceLoop(pb, pb.selectedFacesInternal);
                pb.SetSelectedFaces(loop);
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Select Face Loop");
        }
    }
}
