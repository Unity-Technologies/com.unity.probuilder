using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    static class PreviewActionManager
    {
        [UserSetting("Mesh Editing", "Auto Update Action Preview", "Automatically update the action preview, without delay. This operation is costly and can cause lag when working with large selections.")]
        static Pref<bool> s_AutoUpdatePreview = new Pref<bool>("editor.autoUpdatePreview", false, SettingsScope.Project);
        internal static bool delayedPreview => !s_AutoUpdatePreview.value;

        static MenuAction s_CurrentAction;
        internal static MenuAction currentAction => s_CurrentAction;

        static bool s_HasPreview;
        internal static bool hasPreview => s_HasPreview;

        static bool s_SelectionChangedByAction;
        internal static bool selectionChangedByAction
        {
            set => s_SelectionChangedByAction = value;
        }

        static Overlay s_Overlay;

        internal static void SetPreviewUpdate(bool value)
        {
            if(s_AutoUpdatePreview.value == value)
                return;

            s_AutoUpdatePreview.value = value;

            SceneView.RemoveOverlayFromActiveView(s_Overlay);
            s_Overlay = null;
            SceneView.AddOverlayToActiveView(s_Overlay = new MenuActionSettingsOverlay());
        }

        internal static void DoAction(MenuAction action, bool preview)
        {
            if (s_CurrentAction != null)
            {
                Validate();
                Clear();
            }

            s_CurrentAction = action;
            s_HasPreview = preview;
            s_SelectionChangedByAction = false;

            if (preview)
            {
                UndoUtility.StartPreview();
                s_CurrentAction.PerformAction();
            }

            // Changing selection/tool/context should apply the preview and exit the current action
            Selection.selectionChanged += ObjectSelectionChanged;
            ToolManager.activeContextChanged += Validate;
            ToolManager.activeToolChanged += Validate;
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged += SelectModeChanged;

            SceneView.AddOverlayToActiveView(s_Overlay = new MenuActionSettingsOverlay());
        }

        static void Clear()
        {
            SceneView.RemoveOverlayFromActiveView(s_Overlay);
            s_Overlay = null;
            s_CurrentAction = null;
            s_SelectionChangedByAction = false;

            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged -= SelectModeChanged;
            ToolManager.activeContextChanged -= Validate;
            ToolManager.activeToolChanged -= Validate;
            Selection.selectionChanged -= ObjectSelectionChanged;
        }

        internal static void Validate()
        {
            if (s_HasPreview)
            {
                Clear();
                UndoUtility.ExitAndValidatePreview();
            }
            else
            {
                s_CurrentAction.PerformAction();
                Clear();
            }
        }

        internal static void Cancel()
        {
            Clear();
            if(s_HasPreview)
                UndoUtility.UndoPreview();
        }

        static void ObjectSelectionChanged()
        {
            if (!s_SelectionChangedByAction)
                Validate();

            s_SelectionChangedByAction = false;
        }

        static void SelectModeChanged(SelectMode _) => Validate();

        static void OnSelectionUpdated(IEnumerable<ProBuilderMesh> _)
        {
            if (!s_SelectionChangedByAction)
               Validate();
            s_SelectionChangedByAction = false;
        }

        internal static void UpdatePreview()
        {
            //Undo action might be triggering a refresh of the mesh and of the selection, so we need to temporarily unregister to these events
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            Selection.selectionChanged -= ObjectSelectionChanged;
            UndoUtility.StartPreview();
            s_CurrentAction.PerformAction();
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            Selection.selectionChanged += ObjectSelectionChanged;
        }
    }

    class MenuActionSettingsOverlay : Overlay
    {
        public MenuActionSettingsOverlay()
        {
            displayName = PreviewActionManager.currentAction.menuTitle;
            displayed = true;
        }

        public override VisualElement CreatePanelContent()
        {
            rootVisualElement.tooltip = PreviewActionManager.currentAction.tooltip.summary;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.minWidth = root.style.maxWidth = 300;

            var lastLine = new VisualElement();
            lastLine.style.flexDirection = FlexDirection.Row;
            var okButton = new Button(PreviewActionManager.Validate);
            okButton.text = "Validate";
            okButton.style.flexGrow = 1;
            var cancelButton = new Button(PreviewActionManager.Cancel);
            cancelButton.text = "Cancel";
            cancelButton.style.flexGrow = 1;
            lastLine.Add(okButton);
            lastLine.Add(cancelButton);

            var settingsElement = PreviewActionManager.currentAction.CreateSettingsContent();
            root.Add(settingsElement);
            if (PreviewActionManager.hasPreview)
            {
                var toggle = new Toggle("Live Preview");
                toggle.SetValueWithoutNotify(!PreviewActionManager.delayedPreview);
                toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    PreviewActionManager.SetPreviewUpdate(evt.newValue);
                });
                root.Add(toggle);
            }

            root.Add(lastLine);
            return root;
        }
    }

    public class MenuActionSettings : EditorAction
    {
        static bool s_CanTriggerNewAction = true;

        public MenuActionSettings(MenuAction action, bool hasPreview = false)
        {
            if (!s_CanTriggerNewAction)
            {
                Finish(EditorActionResult.Canceled);
                return;
            }

            s_CanTriggerNewAction = false;
            PreviewActionManager.DoAction(action, hasPreview);

            // Ensure we are not calling the action again in the same frame
            // Needed because Menu Items are called once per selected object while PB actions are acting on the entire selection at once
            EditorApplication.delayCall += () => { s_CanTriggerNewAction = true; };

            Finish(EditorActionResult.Success);
        }
    }
}
