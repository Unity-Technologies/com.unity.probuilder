using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectSmoothingGroup : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Toolbar/Selection_SelectBySmoothingGroup";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Smoothing Group",
                "Selects all faces matching the selected smoothing groups."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override MenuActionState optionsMenuState
        {
            get
            {
                if (enabled && ProBuilderEditor.selectMode == SelectMode.Face)
                    return MenuActionState.VisibleAndEnabled;

                return MenuActionState.Visible;
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            UndoUtility.RecordSelection("Select Faces with Smoothing Group");

            HashSet<int> selectedSmoothGroups = new HashSet<int>(MeshSelection.topInternal.SelectMany(x => x.selectedFacesInternal.Select(y => y.smoothingGroup)));

            List<GameObject> newSelection = new List<GameObject>();

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                IEnumerable<Face> matches = pb.facesInternal.Where(x => selectedSmoothGroups.Contains(x.smoothingGroup));

                if (matches.Count() > 0)
                {
                    newSelection.Add(pb.gameObject);
                    pb.SetSelectedFaces(matches);
                }
            }

            Selection.objects = newSelection.ToArray();

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Select Faces with Smoothing Group");
        }
    }
}
