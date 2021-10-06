using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
#if UNITY_2020_2_OR_NEWER

    abstract class PositionToolContext : EditorToolContext
    {
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

    [EditorToolContext("Vertex"), Icon(k_IconPath)]
    class VertexToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Vertex.png";

        public override void OnActivated()
        {
            ProBuilderEditor.selectMode = SelectMode.Vertex;
        }
    }

    [EditorToolContext("Edge"), Icon(k_IconPath)]
    class EdgeToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Edge.png";

        public override void OnActivated()
        {
            ProBuilderEditor.selectMode = SelectMode.Edge;
        }
    }

    [EditorToolContext("Face"), Icon(k_IconPath)]
    class FaceToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Face.png";

        public override void OnActivated()
        {
            ProBuilderEditor.selectMode = SelectMode.Face;
        }
    }

    class ProBuilderToolSettings : Editor, ICreateToolbar
    {
        public IEnumerable<string> toolbarElements
        {
            get
            {
                yield return "Tool Settings/Pivot Mode";
                yield return "ProBuilder Tool Settings/Pivot Rotation";
                yield return "ProBuilder Tool Settings/Selection";
            }
        }
    }

    [EditorToolbarElement("ProBuilder Tool Settings/Selection", typeof(SceneView))]
    class SelectionSettingsToolbar : OverlayToolbar
    {
        SelectionSettingsToolbar()
        {
            name = "Selection Settings Toolbar";

            Add(new DragRectModeDropdown());
            Add(new DragSelectionModeDropdown());
            Add(new SelectBackFacesToggle());

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
