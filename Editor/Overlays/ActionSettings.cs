using System;
using UnityEditor;
using UnityEditor.Actions;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuActionSettingsOverlay : Overlay
{
    MenuAction m_CurrentAction;

    public void Set(MenuAction action)
    {
        m_CurrentAction = action;
    }

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Column;
        root.Add(m_CurrentAction.CreateSettingsContent());
        var lastLine = new VisualElement();
        lastLine.style.flexDirection = FlexDirection.Row;
        var okButton = new Button(OkPerformed);
        okButton.text = "Ok";
        okButton.style.flexGrow = 1;
        var cancelButton = new Button(CancelPerformed);
        cancelButton.text = "Cancel";
        cancelButton.style.flexGrow = 1;
        lastLine.Add(okButton);
        lastLine.Add(cancelButton);
        root.Add(lastLine);

        return root;
    }

    internal void OkPerformed()
    {
        m_CurrentAction.PerformAction();
        MenuActionSettings.End();
    }

    void CancelPerformed()
    {
        MenuActionSettings.End();
    }
}

public class MenuActionSettings
{
    static MenuActionSettingsOverlay s_Overlay;
    static MenuActionSettingsOverlay overlayInstance => s_Overlay??=new MenuActionSettingsOverlay();

    static MenuActionSettings s_Instance;
    static MenuActionSettings instance => s_Instance??=new MenuActionSettings();

    static bool isInUse = false;

    [InitializeOnLoadMethod]
    static void Initialize()
    {
        s_Instance = new MenuActionSettings();
    }

    MenuActionSettings()
    {
        SceneView.onGUIStarted += OnSceneGUI;
    }

    public static void Start(MenuAction action)
    {
        if(isInUse)
            SceneView.RemoveOverlayFromActiveView(overlayInstance);

        // Create the overlay when the action is created
        overlayInstance.Set(action);
        SceneView.AddOverlayToActiveView(overlayInstance);
        overlayInstance.RebuildContent();
        overlayInstance.displayed = true;

        isInUse = true;
    }

    public static void End()
    {
        SceneView.RemoveOverlayFromActiveView(overlayInstance);
        isInUse = false;
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        var evt = Event.current;
        switch (evt.type)
        {
            case EventType.KeyDown:
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    Debug.Log($"Action Finished [{EditorActionResult.Success}]");
                    overlayInstance.OkPerformed();
                    evt.Use();
                }
                if (evt.keyCode == KeyCode.Escape)
                {
                    Debug.Log($"Action Finished [{EditorActionResult.Canceled}]");
                    End();
                    evt.Use();
                }
                break;
        }
    }

}
