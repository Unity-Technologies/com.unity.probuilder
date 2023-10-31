using System;
using System.Collections.Generic;
using UnityEditor.Actions;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    class MenuActionSettingsOverlay : Overlay
    {
        MenuActionSettings m_Owner;
        MenuAction m_CurrentAction;

        public MenuActionSettingsOverlay(MenuActionSettings owner, MenuAction action)
        {
            displayName = action.menuTitle;
            m_Owner = owner;
            m_CurrentAction = action;
        }

        public override VisualElement CreatePanelContent()
        {
            rootVisualElement.tooltip = m_CurrentAction.tooltip.summary;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.minWidth = root.style.maxWidth = 300;

            var lastLine = new VisualElement();
            lastLine.style.flexDirection = FlexDirection.Row;
            var okButton = new Button(() => m_Owner.Finish(EditorActionResult.Success));
            okButton.text = "Validate";
            okButton.style.flexGrow = 1;
            var cancelButton = new Button(() => m_Owner.Finish(EditorActionResult.Canceled));
            cancelButton.text = "Cancel";
            cancelButton.style.flexGrow = 1;
            lastLine.Add(okButton);
            lastLine.Add(cancelButton);

            var settingsElement = m_CurrentAction.CreateSettingsContent();
            root.Add(settingsElement);

            if (m_Owner is MenuActionSettingsWithPreview ownerWithPreview)
            {
                var previewButton = new Button(() => ownerWithPreview.UpdatePreview());
                previewButton.text = "Preview";
                previewButton.style.flexDirection = FlexDirection.Row;
                previewButton.style.flexGrow = 1;
                root.Add(previewButton);
            }

            root.Add(lastLine);

            return root;
        }
    }

    public class MenuActionSettings : EditorAction
    {
        MenuActionSettingsOverlay m_Overlay;
        protected MenuAction m_Action;

        public MenuActionSettings(MenuAction action)
        {
            m_Action = action;

            // Creating the overlay based on the action to fill the settings
            m_Overlay = new MenuActionSettingsOverlay(this, action);
            SceneView.AddOverlayToActiveView(m_Overlay);
            m_Overlay.displayed = true;

            // Pressing esc should escape the current action without applying, however, pressing ESC in PB is
            // returning from vert/edge/face mode to object mode, so when select mode is changed, we cancel the
            // action to mimic the behavior of EditorAction.
            ProBuilderEditor.selectModeChanged += (_) => Finish(EditorActionResult.Canceled);

            // Undo should undo the preview (if a preview exists) and leaving the action
            Undo.undoRedoEvent += UndoRedoEventCallback;

            // Delay call to ensure that if a MenuActionSettings is currently active, starting this one will not call
            // OnMenuActionPerformed when the other is deactivated
            EditorApplication.delayCall += () => MenuAction.onPerformAction += OnMenuActionPerformed;
        }

        protected override void OnFinish(EditorActionResult result)
        {
            Undo.undoRedoEvent -= UndoRedoEventCallback;
            MenuAction.onPerformAction -= OnMenuActionPerformed;
            SceneView.RemoveOverlayFromActiveView(m_Overlay);

            if (!(this is MenuActionSettingsWithPreview))
                m_Action.PerformAction();
        }

        protected virtual void UndoRedoEventCallback(in UndoRedoInfo info)
        {
            Finish(EditorActionResult.Canceled);
        }

        void OnMenuActionPerformed(MenuAction action)
        {
            if (action != m_Action)
                Finish(EditorActionResult.Canceled);
        }

        public override void OnSceneGUI(SceneView sceneView)
        {
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.KeyDown:
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        Finish(EditorActionResult.Success);
                        evt.Use();
                    }

                    return;
            }

            m_Action.DoSceneGUI(sceneView);
        }
    }

    public class MenuActionSettingsWithPreview : MenuActionSettings
    {
        bool m_UndoNeeded = true;

        public MenuActionSettingsWithPreview(MenuAction action) : base(action)
        {
            UndoUtility.StartPreview();
            // Triggering action Preview
            m_Action.PerformAction();

            // Changing selection should apply the preview and exit the current action
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
        }

        protected override void OnFinish(EditorActionResult result)
        {
            base.OnFinish(result);
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            if (m_UndoNeeded && result == EditorActionResult.Canceled)
                UndoUtility.EndPreview();
            else
                UndoUtility.ResetPreview();
        }

        internal void UpdatePreview()
        {
            //Undo action might be triggering a refresh of the mesh and of the selection, so we need to temporarily unregister to these events
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            Undo.undoRedoEvent -= UndoRedoEventCallback;
            UndoUtility.StartPreview();
            m_Action.PerformAction();
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            Undo.undoRedoEvent += UndoRedoEventCallback;
        }

        // Selection can be updated by the ProBuilder editor on a UndoRedo event, so we need to check if we are currently in this
        // situation to avoid exiting the action when we are actually undoing/redoing as we are not handling the 2 events the same way.
        void OnSelectionUpdated(IEnumerable<ProBuilderMesh> meshes)
        {
            if (!UndoUtility.IsPerformingUndoRedo)
                Finish(EditorActionResult.Success);
        }

        protected override void UndoRedoEventCallback(in UndoRedoInfo info)
        {
            m_UndoNeeded = false;
            Finish(EditorActionResult.Canceled);
        }
    }
}
