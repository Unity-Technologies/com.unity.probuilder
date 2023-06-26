using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
#if UNITY_2020_2_OR_NEWER

    class PositionToolContext : EditorToolContext
    {
        PositionToolContext() { }


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

#if UNITY_2023_2_OR_NEWER
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

            // grouping and filtering is bespoke for demo reasons
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

                menu.AppendAction(title, _ => action.PerformAction(), GetStatus(action));
            }
        }
#endif

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
