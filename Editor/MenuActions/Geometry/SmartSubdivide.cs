using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SmartSubdivide : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Smart Subdivide",
                "",
                keyCommandAlt, 'S'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face; }
        }

        public override bool enabled
        {
            get { return base.enabled && (MeshSelection.selectedEdgeCount > 0 || MeshSelection.selectedFaceCount > 0); }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Edge:
                    return EditorToolbarLoader.GetInstance<SubdivideEdges>().PerformAction();

                default:
                    return EditorToolbarLoader.GetInstance<SubdivideFaces>().PerformAction();
            }
        }
    }
}
