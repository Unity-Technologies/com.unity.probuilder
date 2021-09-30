using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragRectMode : MenuAction
    {
        RectSelectMode mode
        {
            get { return ProBuilderEditor.rectSelectMode; }
            set { ProBuilderEditor.rectSelectMode = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Settings; }
        }

        public override Texture2D icon
        {
            get
            {
                return mode == RectSelectMode.Complete
                    ? IconUtility.GetIcon("Toolbar/Selection_Rect_Complete")
                    : IconUtility.GetIcon("Toolbar/Selection_Rect_Intersect", IconSkin.Pro);
            }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Set Drag Rect Mode",
                "Sets whether or not a mesh element (edge or face) needs to be completely encompassed by a drag to be selected.\n\nThe default value is Intersect, meaning if any part of the elemnent is touched by the drag rectangle it will be selected."
            );

        public override string menuTitle { get { return mode == RectSelectMode.Complete ? "Rect: Complete" : "Rect: Intersect"; } }

        protected override ActionResult PerformActionImplementation()
        {
            mode = InternalUtility.NextEnumValue(mode);

            return new ActionResult(ActionResult.Status.Success,
                "Set Drag Select\n" + (mode == RectSelectMode.Complete ? "Complete" : "Intersect"));
        }

        public override bool enabled
        {
            get
            {
                return ProBuilderEditor.instance != null
                    && ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace);
            }
        }
    }

    class DragRectModeDropdown : EditorToolbarDropdown
    {
        readonly GUIContent m_Partial;
        readonly GUIContent m_Complete;
        readonly ToggleDragRectMode m_MenuAction;

        public DragRectModeDropdown()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
            name = m_MenuAction.tooltip.title;
            tooltip = m_MenuAction.tooltip.summary;

            m_Partial = EditorGUIUtility.TrTextContent("Intersect");
            m_Complete = EditorGUIUtility.TrTextContent("Complete");

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            clicked += OpenContextMenu;

            OnDropdownOptionChange();
        }

        void OpenContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(m_Partial, ProBuilderEditor.rectSelectMode == RectSelectMode.Partial,
                () => SetRectSelectModeIfNeeded(RectSelectMode.Partial));

            menu.AddItem(m_Complete, ProBuilderEditor.rectSelectMode == RectSelectMode.Complete,
                () => SetRectSelectModeIfNeeded(RectSelectMode.Complete));

            menu.DropDown(worldBound);
        }

        void SetRectSelectModeIfNeeded(RectSelectMode rectSelectMode)
        {
            if (ProBuilderEditor.rectSelectMode != rectSelectMode)
            {
                ProBuilderEditor.rectSelectMode = rectSelectMode;
                OnDropdownOptionChange();
            }
        }

        void OnDropdownOptionChange()
        {
            icon = m_MenuAction.icon;
        }

        void AttachedToPanel(AttachToPanelEvent evt)
        {
            MenuAction.afterActionPerformed += OnMenuActionPerformed;
        }

        void DetachedFromPanel(DetachFromPanelEvent evt)
        {
            MenuAction.afterActionPerformed -= OnMenuActionPerformed;
        }

        void OnMenuActionPerformed(MenuAction menuAction)
        {
            if (menuAction == m_MenuAction)
                OnDropdownOptionChange();
        }
    }
}
