using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    class MenuActionSettingsOverlay : Overlay
    {
        bool m_HasPreview = false;
        MenuAction m_CurrentAction;

        public MenuActionSettingsOverlay(MenuAction action, bool hasPreview)
        {
            m_CurrentAction = action;
            m_HasPreview = hasPreview;
            displayName = action.menuTitle;
            m_CurrentAction = action;

            if (hasPreview)
            {
                UndoUtility.StartPreview();
                m_CurrentAction.PerformAction();
            }

            SceneView.AddOverlayToActiveView(this);
            displayed = true;

            // Changing selection/tool/context should apply the preview and exit the current action
            Selection.selectionChanged += Validate;
            ToolManager.activeContextChanged += Validate;
            ToolManager.activeToolChanged += Validate;
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged += SelectModeChanged;
        }

        void Clear()
        {
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged -= SelectModeChanged;
            ToolManager.activeContextChanged -= Validate;
            ToolManager.activeToolChanged -= Validate;
            Selection.selectionChanged -= Validate;
            SceneView.RemoveOverlayFromActiveView(this);
            MenuActionSettings.s_ActionSettingsOverlay = null;
        }

        public override VisualElement CreatePanelContent()
        {
            rootVisualElement.tooltip = m_CurrentAction.tooltip.summary;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.minWidth = root.style.maxWidth = 300;

            var lastLine = new VisualElement();
            lastLine.style.flexDirection = FlexDirection.Row;
            var okButton = new Button(() => Validate());
            okButton.text = "Validate";
            okButton.style.flexGrow = 1;
            var cancelButton = new Button(() => Cancel());
            cancelButton.text = "Cancel";
            cancelButton.style.flexGrow = 1;
            lastLine.Add(okButton);
            lastLine.Add(cancelButton);

            var settingsElement = m_CurrentAction.CreateSettingsContent();
            root.Add(settingsElement);

            if (m_HasPreview)
            {
                var previewButton = new Button(UpdatePreview);
                previewButton.text = "Preview";
                previewButton.style.flexDirection = FlexDirection.Row;
                previewButton.style.flexGrow = 1;
                root.Add(previewButton);
            }

            root.Add(lastLine);

            return root;
        }

        internal void Validate()
        {
            Clear();
            if(m_HasPreview)
                UndoUtility.ExitAndValidatePreview();
            else
                m_CurrentAction.PerformAction();
        }

        void Cancel()
        {
            Clear();
            if(m_HasPreview)
                UndoUtility.UndoPreview();
        }

        void SelectModeChanged(SelectMode _) => Validate();

        void OnSelectionUpdated(IEnumerable<ProBuilderMesh> _) => Validate();

        void UpdatePreview()
        {
            //Undo action might be triggering a refresh of the mesh and of the selection, so we need to temporarily unregister to these events
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            UndoUtility.StartPreview();
            m_CurrentAction.PerformAction();
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
        }
    }

    public class MenuActionSettings : EditorAction
    {
        internal static MenuActionSettingsOverlay s_ActionSettingsOverlay;

        public MenuActionSettings(MenuAction action, bool hasPreview = false)
        {
            if (s_ActionSettingsOverlay != null)
                s_ActionSettingsOverlay.Validate();

            // Creating the overlay based on the action to fill the settings
            s_ActionSettingsOverlay = new MenuActionSettingsOverlay(action, hasPreview);
            Finish(EditorActionResult.Success);
        }
    }
}
