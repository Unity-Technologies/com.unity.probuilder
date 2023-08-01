#if UNITY_2023_2_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.Actions;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.UIElements;

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

        if (m_Owner.HasPreview)
        {
            var previewButton = new Button(() => m_Owner.UpdatePreview());
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
    MenuAction m_Action;

    bool m_Preview;
    internal bool HasPreview => m_Preview;

    bool m_UndoNeeded = true;

    public MenuActionSettings(MenuAction action, bool hasPreview = true)
    {
        m_Action = action;
        m_Preview = hasPreview;
        m_UndoNeeded = m_Preview;

        if(m_Preview)
            m_Action.PerformAction();
        // Creating the overlay based on the action to fill the settings
        m_Overlay = new MenuActionSettingsOverlay(this, action);
        SceneView.AddOverlayToActiveView(m_Overlay);
        m_Overlay.displayed = true;

        // Pressing esc should escape the current action without applying, however, pressing ESC in PB is
        // returning from vert/edge/face mode to object mode, so when select mode is changed, we cancel the
        // action to mimic the behavior of EditorAction.
        ProBuilderEditor.selectModeChanged += (_) => Finish(EditorActionResult.Canceled);

        // Undo should undo the preview and leaving the action
        Undo.undoRedoPerformed += UndoRedoPerformed;

        // Delay call to ensure that if a MenuActionSettings is currently active, starting this one will not call
        // OnMenuActionPerformed when the other is deactivated
        EditorApplication.delayCall += () => MenuAction.onPerformAction += OnMenuActionPerformed;
    }

    protected override void OnFinish(EditorActionResult result)
    {
        MenuAction.onPerformAction -= OnMenuActionPerformed;
        Undo.undoRedoPerformed -= UndoRedoPerformed;
        SceneView.RemoveOverlayFromActiveView(m_Overlay);

        if (!HasPreview && result == EditorActionResult.Success)
            m_Action.PerformAction();
        else if (m_UndoNeeded && result == EditorActionResult.Canceled)
            EditorApplication.delayCall += Undo.PerformUndo;
    }

    internal void UpdatePreview()
    {
        if (HasPreview)
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.PerformUndo();
            m_Action.PerformAction();
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }
    }

    void UndoRedoPerformed()
    {
        m_UndoNeeded = false;
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
                if (evt.keyCode == KeyCode.Escape)
                {
                    Finish(EditorActionResult.Canceled);
                    evt.Use();
                }
                return;
        }
        m_Action.DoSceneGUI(sceneView);
    }
}
#endif
