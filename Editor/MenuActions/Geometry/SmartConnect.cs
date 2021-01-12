using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SmartConnect : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return null; } }
        public override TooltipContent tooltip { get { return _tooltip; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Smart Connect",
                "",
                keyCommandAlt, 'E'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        public override bool enabled
        {
            get
            {
                return base.enabled && (MeshSelection.selectedVertexCount > 1
                                        || MeshSelection.selectedEdgeCount > 1
                                        || MeshSelection.selectedFaceCount > 1);
            }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Vertex:
                    return EditorToolbarLoader.GetInstance<ConnectVertices>().PerformAction();

                default:
                    return EditorToolbarLoader.GetInstance<ConnectEdges>().PerformAction();
            }
        }
    }
}
