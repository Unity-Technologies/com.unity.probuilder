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
                yield return "Select Mode Toolbar";
            }
        }
    }

    [EditorToolbarElement("Select Mode Toolbar", typeof(SceneView))]
    class SelectionSettingsToolbar : OverlayToolbar
    {
        List<EditorToolbarToggle> m_SelectModeToggles;
        SelectionSettingsToolbar()
        {
            name = "Select Mode Toolbar";

            m_SelectModeToggles = new List<EditorToolbarToggle>();

            m_SelectModeToggles.Add(CreateSelectModeToggle(SelectMode.Object));
            m_SelectModeToggles.Add(CreateSelectModeToggle(SelectMode.Vertex));
            m_SelectModeToggles.Add(CreateSelectModeToggle(SelectMode.Edge));
            m_SelectModeToggles.Add(CreateSelectModeToggle(SelectMode.Face));

            SetupChildrenAsButtonStrip();
        }

        EditorToolbarToggle CreateSelectModeToggle(SelectMode mode)
        {
            var toggleContent = UI.EditorGUIUtility.GetSelectModeToggleContent(mode);
            var toggle = new SelectModeToggle(mode, toggleContent.text, toggleContent.image as Texture2D);
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
