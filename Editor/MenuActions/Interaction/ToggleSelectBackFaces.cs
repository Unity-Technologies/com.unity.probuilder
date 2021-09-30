using System;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleSelectBackFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Settings; }
        }

        public override Texture2D icon
        {
            get { return ProBuilderEditor.backfaceSelectionEnabled ? m_Icons[1] : m_Icons[0]; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Set Hidden Element Selection",
                @"Setting Hidden Element Selection to <b>On</b> allows you to select faces that are either obscured by geometry or facing away from the scene camera (backfaces).

The default value is <b>On</b>.
");

        public override string menuTitle
        {
            get { return ProBuilderEditor.backfaceSelectionEnabled ? "Select Hidden: On" : "Select Hidden: Off"; }
        }

        public override SelectMode validSelectModes
        {
            get
            {
                return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace;
            }
        }

        Texture2D[] m_Icons;

        public ToggleSelectBackFaces()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On", IconSkin.Pro)
            };
        }

        protected override ActionResult PerformActionImplementation()
        {
            ProBuilderEditor.backfaceSelectionEnabled = !ProBuilderEditor.backfaceSelectionEnabled;
            return new ActionResult(ActionResult.Status.Success, "Set Hidden Element Selection\n" + (!ProBuilderEditor.backfaceSelectionEnabled ? "On" : "Off"));
        }
    }

    class SelectBackFacesToggle : EditorToolbarToggle
    {
        readonly ToggleSelectBackFaces m_MenuAction;

        public SelectBackFacesToggle(): base(IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On"),
            IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off"))
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
            name = m_MenuAction.tooltip.title;
            tooltip = m_MenuAction.tooltip.summary;

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            OnSelectBackFacesChange();
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            ProBuilderEditor.backfaceSelectionEnabled = newValue;
        }

        void OnSelectBackFacesChange()
        {
            value = ProBuilderEditor.backfaceSelectionEnabled;
        }

        void AttachedToPanel(AttachToPanelEvent evt)
        {
            MenuAction.afterActionPerformed += OnMenuActionPerformed;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
        }

        void DetachedFromPanel(DetachFromPanelEvent evt)
        {
            MenuAction.afterActionPerformed -= OnMenuActionPerformed;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
        }

        void OnMenuActionPerformed(MenuAction menuAction)
        {
            if (menuAction == m_MenuAction)
                OnSelectBackFacesChange();
        }

        void OnSelectModeChanged(SelectMode mode)
        {
            switch (mode)
            {
                case SelectMode.Object:
                    SetEnabled(false);
                    break;
                default:
                    SetEnabled(true);
                    break;
            }
        }
    }
}
