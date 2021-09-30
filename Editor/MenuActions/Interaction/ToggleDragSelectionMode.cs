using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragSelectionMode : MenuAction
    {
        SelectionModifierBehavior modifier
        {
            get { return ProBuilderEditor.selectionModifierBehavior; }
            set { ProBuilderEditor.selectionModifierBehavior = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Settings; }
        }

        public override Texture2D icon
        {
            get
            {
                if (modifier == SelectionModifierBehavior.Add)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftAdd", IconSkin.Pro);
                else if (modifier == SelectionModifierBehavior.Subtract)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftSubtract", IconSkin.Pro);
                else
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
            }
        }

        public override TooltipContent tooltip
        {
            get { return _tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Set Drag Selection Mode",
                @"When drag selecting elements, does the shift key

- [Add] Always add to the selection
- [Subtract] Always subtract from the selection
- [Difference] Invert the selection by the selected faces (Default)
");

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override string menuTitle
        {
            get { return string.Format("Shift: {0}", modifier); }
        }

        protected override ActionResult PerformActionImplementation()
        {
            int mode = (int)modifier;
            int len = System.Enum.GetValues(typeof(SelectionModifierBehavior)).Length;
            modifier = (SelectionModifierBehavior)((mode + 1) % len);
            return new ActionResult(ActionResult.Status.Success, "Set Shift Drag Mode\n" + modifier);
        }
    }

    class DragSelectionModeDropdown : EditorToolbarDropdown
    {
        readonly GUIContent m_Add;
        readonly GUIContent m_Subtract;
        readonly GUIContent m_Difference;
        readonly ToggleDragSelectionMode m_MenuAction;

        public DragSelectionModeDropdown()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
            name = m_MenuAction.tooltip.title;
            tooltip = m_MenuAction.tooltip.summary;

            m_Add = EditorGUIUtility.TrTextContent("Add");
            m_Subtract = EditorGUIUtility.TrTextContent("Subtract");
            m_Difference = EditorGUIUtility.TrTextContent("Difference");

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            clicked += OpenContextMenu;

            OnDropdownOptionChange();
        }

        void OpenContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(m_Add, ProBuilderEditor.selectionModifierBehavior == SelectionModifierBehavior.Add,
                () => SetDragSelectionModeIfNeeded(SelectionModifierBehavior.Add));

            menu.AddItem(m_Subtract, ProBuilderEditor.selectionModifierBehavior == SelectionModifierBehavior.Subtract,
                () => SetDragSelectionModeIfNeeded(SelectionModifierBehavior.Subtract));

            menu.AddItem(m_Difference, ProBuilderEditor.selectionModifierBehavior == SelectionModifierBehavior.Difference,
                () => SetDragSelectionModeIfNeeded(SelectionModifierBehavior.Difference));

            menu.DropDown(worldBound);
        }

        void SetDragSelectionModeIfNeeded(SelectionModifierBehavior mode)
        {
            if (ProBuilderEditor.selectionModifierBehavior != mode)
            {
                ProBuilderEditor.selectionModifierBehavior = mode;
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
