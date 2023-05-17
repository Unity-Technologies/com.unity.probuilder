using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
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

        static readonly HashSet<string> k_ContextMenuBlacklist = new HashSet<string>()
        {
            "Smoothing",
            "Material Editor",
            "UV Editor",
            "Vertex Colors"
        };

        public override void PopulateMenu(DropdownMenu menu)
        {
            menu.AppendHeaderAction(IconUtility.GetIcon("Modes/Mode_Vertex"),
                x => { ProBuilderEditor.selectMode = SelectMode.Vertex; },
                x => ProBuilderEditor.selectMode == SelectMode.Vertex
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(IconUtility.GetIcon("Modes/Mode_Edge"),
                x => { ProBuilderEditor.selectMode = SelectMode.Edge; },
                x => ProBuilderEditor.selectMode == SelectMode.Edge
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(IconUtility.GetIcon("Modes/Mode_Face"),
                x => { ProBuilderEditor.selectMode = SelectMode.Face; },
                x => ProBuilderEditor.selectMode == SelectMode.Face
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);

            var actions = EditorToolbarLoader.GetActions();
            var group = ToolbarGroup.Tool;

            // grouping and filtering is bespoke for demo reasons
            foreach (var action in actions)
            {
                if (k_ContextMenuBlacklist.Contains(action.menuTitle))
                    continue;

                if (action.group == ToolbarGroup.Entity)
                    continue;

                if (action.group != group)
                {
                    menu.AppendSeparator();
                    group = action.group;
                }

                if(action.group != ToolbarGroup.Geometry && action.group != ToolbarGroup.Tool)
                    menu.AppendAction($"{action.group}/{action.menuTitle}", x => action.PerformAction(), GetStatus(action));
                else
                    menu.AppendAction(action.menuTitle, x => action.PerformAction(), GetStatus(action));
            }
        }

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
