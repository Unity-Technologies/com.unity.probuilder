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
                yield return "Select Mode Toolbar";
            }
        }
    }

    [EditorToolbarElement("Select Mode Toolbar", typeof(SceneView))]
    class SelectModeToolbar : OverlayToolbar
    {
        SelectModeToolbar()
        {
            name = "Select Mode Toolbar";

            CreateSelectModeToggle(SelectMode.Object, "Object Selection", "Modes/Mode_Object");
            CreateSelectModeToggle(SelectMode.Vertex, "Vertex Selection", "Modes/Mode_Vertex");
            CreateSelectModeToggle(SelectMode.Edge, "Edge Selection" , "Modes/Mode_Edge");
            CreateSelectModeToggle(SelectMode.Face, "Face Selection", "Modes/Mode_Face");

            SetupChildrenAsButtonStrip();
        }

        void CreateSelectModeToggle(SelectMode mode, string text, string iconName)
        {
            var icon = IconUtility.GetIcon(iconName);
            var toggle = new SelectModeToggle(mode, text, icon);
            Add(toggle);
        }
    }

    class ProbuilderToolSettings : Editor, ICreateToolbar
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
