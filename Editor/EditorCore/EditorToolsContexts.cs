using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
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
                yield return "Selection Settings Toolbar";
            }
        }
    }

    [EditorToolbarElement("Selection Settings Toolbar", typeof(SceneView))]
    class SelectionSettingsToolbar : OverlayToolbar
    {
        List<EditorToolbarToggle> m_SelectModeToggles;
        SelectionSettingsToolbar()
        {
            name = "ProBuilder Selection Settings";

            m_SelectModeToggles = new List<EditorToolbarToggle>();

            m_SelectModeToggles.Add(AddSelectModeToggle(SelectMode.Object, "Object Selection", "Modes/Mode_Object"));
            m_SelectModeToggles.Add(AddSelectModeToggle(SelectMode.Vertex, "Vertex Selection", "Modes/Mode_Vertex"));
            m_SelectModeToggles.Add(AddSelectModeToggle(SelectMode.Edge, "Edge Selection", "Modes/Mode_Edge"));
            m_SelectModeToggles.Add(AddSelectModeToggle(SelectMode.Face, "Face Selection", "Modes/Mode_Face"));

            SetupChildrenAsButtonStrip();
        }

        EditorToolbarToggle AddSelectModeToggle(SelectMode mode, string name, string iconName)
        {
            var icon = IconUtility.GetIcon(iconName);
            var toggle = new SelectModeToggle(mode, name, IconUtility.GetIcon(iconName));
            Add(toggle);

            return toggle;
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
