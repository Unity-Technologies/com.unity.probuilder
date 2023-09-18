using System;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder.Actions;
using System.Collections.Generic;
using UnityEditor.Actions;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderToolManager
    {
        static Pref<SelectMode> s_SelectMode = new Pref<SelectMode>(nameof(s_SelectMode), SelectMode.Face);

        public static Tool activeTool => Tools.current;

        public static SelectMode selectMode
        {
            get => SelectMode.Face;
            set => s_SelectMode.SetValue(value);
        }
    }

    [Icon("Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Face.png")]
    [EditorToolContext("ProBuilder")]
    class PositionToolContext : EditorToolContext
    {
        ProBuilderEditor m_Editor;

        protected override Type GetEditorToolType(Tool tool)
        {
            return tool switch
            {
                Tool.Move => typeof(ProbuilderMoveTool),
                Tool.Rotate => typeof(ProbuilderRotateTool),
                Tool.Scale => typeof(ProbuilderScaleTool),
                _ => null
            };
        }

        static DropdownMenuAction.Status GetStatus(MenuAction action)
        {
            if(action.hidden)
                return DropdownMenuAction.Status.Hidden;
            if (action.enabled)
                return DropdownMenuAction.Status.Normal;
            return DropdownMenuAction.Status.Disabled;
        }

        static readonly HashSet<Type> k_ContextMenuBlacklist = new HashSet<Type>()
        {
            typeof(Actions.OpenSmoothingEditor),
            typeof(Actions.OpenMaterialEditor),
            typeof(Actions.OpenUVEditor),
            typeof(Actions.OpenVertexColorEditor),

            typeof(Actions.ToggleSelectBackFaces),
            typeof(Actions.ToggleHandleOrientation),
            typeof(Actions.ToggleDragSelectionMode),
            typeof(Actions.ToggleDragRectMode),
            typeof(Actions.ToggleXRay)
        };

        public override void PopulateMenu(DropdownMenu menu)
        {
            //Headers area is for ProBuilder modes
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.VertexIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Vertex; },
                x => ProBuilderEditor.selectMode == SelectMode.Vertex
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.EdgeIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Edge; },
                x => ProBuilderEditor.selectMode == SelectMode.Edge
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.FaceIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Face; },
                x => ProBuilderEditor.selectMode == SelectMode.Face
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);

            var actions = EditorToolbarLoader.GetActions();
            var group = ToolbarGroup.Tool;

            foreach (var action in actions)
            {
                if (k_ContextMenuBlacklist.Contains(action.GetType()))
                    continue;

                if (action.group == ToolbarGroup.Entity || action.group == ToolbarGroup.Object)
                    continue;

                if (action.group != group)
                {
                    menu.AppendSeparator();
                    group = action.group;
                }

                var title = action.menuTitle;
                if (action.group != ToolbarGroup.Geometry && action.group != ToolbarGroup.Tool)
                    title = $"{action.group}/{action.menuTitle}";

                if (action.hasOptions)
                {
                    if(HasPreview(action))
                        menu.AppendAction(title, _ => EditorAction.Start(new MenuActionSettingsWithPreview(action)), GetStatus(action), action.icon);
                    else
                        menu.AppendAction(title, _ => EditorAction.Start(new MenuActionSettings(action)), GetStatus(action), action.icon);

                }
                else
                    menu.AppendAction(title, _ => action.PerformAction(), GetStatus(action), action.icon);
            }

            Transform[] trs = Selection.transforms;
            if (trs.GetComponents<MeshFilter>().Length > trs.GetComponents<ProBuilderMesh>().Length)
                ContextMenuUtility.AddMenuItemsForType(menu, typeof(MeshFilter), targets, "Mesh Filter");
        }

        static bool HasPreview(MenuAction action)
        {
            return !(action is DetachFaces || action is DuplicateFaces);
        }

        public override void OnActivated()
        {
            m_Editor = new ProBuilderEditor();
        }

        public override void OnWillBeDeactivated()
        {
            m_Editor.Dispose();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView view))
                return;
            m_Editor.OnSceneGUI(view);
        }

    }

    class TextureToolContext : EditorToolContext
    {
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
