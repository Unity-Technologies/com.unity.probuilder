using System;
using UnityEditor.EditorTools;

namespace UnityEditor.ProBuilder
{
    public class UVTextureContext : EditorToolContext
    {
        UVTextureContext()
        {
        }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(UVTextureMoveTool);
                case Tool.Rotate:
                    return typeof(UVTextureRotateTool);
                case Tool.Scale:
                    return typeof(UVTextureScaleTool);
                default:
                    return null;
            }
        }
    }
}
