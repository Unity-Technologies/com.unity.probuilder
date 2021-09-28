using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
#if UNITY_2020_2_OR_NEWER

    [EditorToolContext("ProBuilder"), Icon(k_IconPath)]
    class ProBuilderToolContext : EditorToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Object.png";

        ProBuilderToolContext() { }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(ProbuilderMoveTool);
                case Tool.Rotate:
                    return typeof(ProbuilderRotateTool);
                case Tool.Scale:
                    return typeof(ProbuilderScaleTool);
                default:
                    return null;
            }
        }
    }

    [CustomEditor(typeof(ProBuilderToolContext))]
    class ProBuilderToolContextEditor : Editor, ICreateToolbar
    {
        public IEnumerable<string> toolbarElements
        {
            get
            {
                yield return "Selection Settings Toolbar";
            }
        }
    }

    [EditorToolbarElement("Selection Settings Toolbar", typeof(SceneView))]
    class SelectionSettingsToolbar : OverlayToolbar
    {
        SelectionSettingsToolbar()
        {
            var object_Graphic_off = IconUtility.GetIcon("Modes/Mode_Object");
            var face_Graphic_off = IconUtility.GetIcon("Modes/Mode_Face");
            var vertex_Graphic_off = IconUtility.GetIcon("Modes/Mode_Vertex");
            var edge_Graphic_off = IconUtility.GetIcon("Modes/Mode_Edge");

            name = "ProBuilder Selection Settings";

            Add(new EditorToolbarToggle("Object Selection", object_Graphic_off, object_Graphic_off));
            Add(new EditorToolbarToggle("Vertex Selection", vertex_Graphic_off, vertex_Graphic_off));
            Add(new EditorToolbarToggle("Edge Selection", edge_Graphic_off, edge_Graphic_off));
            Add(new EditorToolbarToggle("Face Selection", face_Graphic_off, face_Graphic_off));

            SetupChildrenAsButtonStrip();
        }
    }

    class TextureToolContext : EditorToolContext
    {
        TextureToolContext() { }

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

#endif
}
