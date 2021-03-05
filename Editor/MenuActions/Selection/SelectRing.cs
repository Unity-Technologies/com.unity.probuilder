using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    // Menu item entry
    sealed class SelectRing : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 2; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Ring",
                "",
                keyCommandAlt, 'R'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get
            {
                if (ProBuilderEditor.selectMode == SelectMode.Edge)
                    return EditorToolbarLoader.GetInstance<SelectEdgeRing>().enabled;
                else if (ProBuilderEditor.selectMode == SelectMode.Face)
                    return EditorToolbarLoader.GetInstance<SelectFaceRing>().enabled;
                else
                    return false;
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (ProBuilderEditor.selectMode == SelectMode.Edge)
                return EditorToolbarLoader.GetInstance<SelectEdgeRing>().PerformAction();
            else if (ProBuilderEditor.selectMode == SelectMode.Face)
                return EditorToolbarLoader.GetInstance<SelectFaceRing>().PerformAction();
            return ActionResult.NoSelection;
        }
    }
}
