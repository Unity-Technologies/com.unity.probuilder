#if UNITY_2023_2_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.Actions;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuActionSettingsOverlay : Overlay
{
    MenuActionSettings m_Owner;
    MenuAction m_CurrentAction;

    public MenuActionSettingsOverlay(MenuActionSettings owner, MenuAction action)
    {
        displayName = action.menuTitle;
        m_Owner = owner;
        m_CurrentAction = action;
        maxSize = new Vector2(300, 300);
    }

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Column;
        root.Add(m_CurrentAction.CreateSettingsContent());
        var lastLine = new VisualElement();
        lastLine.style.flexDirection = FlexDirection.Row;
        var okButton = new Button(() => m_Owner.Finish(EditorActionResult.Success));
        okButton.text = "Ok";
        okButton.style.flexGrow = 1;
        var cancelButton = new Button(() => m_Owner.Finish(EditorActionResult.Canceled));
        cancelButton.text = "Cancel";
        cancelButton.style.flexGrow = 1;
        lastLine.Add(okButton);
        lastLine.Add(cancelButton);
        root.Add(lastLine);

        return root;
    }
}

public class MenuActionSettings : EditorAction
{
    MenuActionSettingsOverlay m_Overlay;
    MenuAction m_Action;

    public MenuActionSettings(MenuAction action)
    {
        m_Action = action;
        // Creating the overlay based on the action to fill the settings
        m_Overlay = new MenuActionSettingsOverlay(this, action);
        SceneView.AddOverlayToActiveView(m_Overlay);
        m_Overlay.displayed = true;
    }

    protected override void OnFinish(EditorActionResult result)
    {
        SceneView.RemoveOverlayFromActiveView(m_Overlay);
        if (result == EditorActionResult.Success)
            m_Action.PerformAction();
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
                break;
        }
    }
}
#endif
