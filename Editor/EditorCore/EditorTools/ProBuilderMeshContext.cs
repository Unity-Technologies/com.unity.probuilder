using System;
using UnityEditor.EditorTools;

namespace UnityEditor.ProBuilder
{
    public class ProBuilderMeshContext : EditorToolContext
    {
        ProBuilderMeshContext()
        {
        }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(ProBuilderMeshMoveTool);
                case Tool.Rotate:
                    return typeof(ProBuilderMeshRotateTool);
                case Tool.Scale:
                    return typeof(ProBuilderMeshScaleTool);
                default:
                    return null;
            }
        }
    }
}
