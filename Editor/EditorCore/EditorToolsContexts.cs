#if UNITY_2020_2_OR_NEWER
using System;
using UnityEditor.EditorTools;

namespace UnityEditor.ProBuilder
{
    public class PositionToolContext : EditorToolContext
    {
        PositionToolContext()
        {
        }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(PositionMoveTool);
                case Tool.Rotate:
                    return typeof(PositionRotateTool);
                case Tool.Scale:
                    return typeof(PositionScaleTool);
                default:
                    return null;
            }
        }
    }

    public class TextureToolContext : EditorToolContext
    {
        TextureToolContext()
        {
        }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(TextureMoveTool);
                case Tool.Rotate:
                    return typeof(TextureRotateTool);
                case Tool.Scale:
                    return typeof(TextureScaleTool);
                default:
                    return null;
            }
        }
    }
}
#endif
