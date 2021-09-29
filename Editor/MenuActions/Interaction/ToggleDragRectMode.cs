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
            get { return ToolbarGroup.Selection; }
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

    sealed class DragRectModeDropdown : EditorToolbarDropdown
    {
        readonly GUIContent m_Intersect;
        readonly GUIContent m_Complete;
        readonly ToggleDragRectMode m_MenuAction;

        public DragRectModeDropdown()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
            name = m_MenuAction.tooltip.title;

            m_Intersect = EditorGUIUtility.TrTextContent("Intersect");
            m_Complete = EditorGUIUtility.TrTextContent("Complete");

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            clicked += OpenContextMenu;

            OnDropdownOptionChange();
        }

        void OpenContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(m_Intersect, ProBuilderEditor.rectSelectMode == RectSelectMode.Partial, () =>
            {
                if (ProBuilderEditor.rectSelectMode != RectSelectMode.Partial) m_MenuAction.PerformAction();
            });
            menu.AddItem(m_Complete, ProBuilderEditor.rectSelectMode == RectSelectMode.Complete, () =>
            {
                if (ProBuilderEditor.rectSelectMode != RectSelectMode.Complete) m_MenuAction.PerformAction();
            });
            menu.DropDown(worldBound);
        }

        void OnDropdownOptionChange()
        {
            tooltip = m_MenuAction.tooltip.summary;
            icon = m_MenuAction.icon;

            //Ensuring constant size of the text area
            var textElement = this.Q<TextElement>(UnityEditor.Toolbars.EditorToolbar.elementLabelClassName);
            if (textElement != null)
                textElement.style.width = 40;
        }

        void AttachedToPanel(AttachToPanelEvent evt)
        {
            MenuAction.onPerformAction += OnMenuActionPerformed;
        }

        void DetachedFromPanel(DetachFromPanelEvent evt)
        {
            MenuAction.onPerformAction -= OnMenuActionPerformed;
        }

        void OnMenuActionPerformed(MenuAction menuAction)
        {
            if (menuAction == m_MenuAction)
                OnDropdownOptionChange();
        }
    }
}
