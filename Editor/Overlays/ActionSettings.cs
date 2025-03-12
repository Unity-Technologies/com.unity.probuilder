using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    class PreviewActionManager : IDisposable
    {
        [UserSetting("Mesh Editing", "Auto Update Action Preview", "Automatically update the action preview, without delay. This operation is costly and can cause lag when working with large selections.")]
        static Pref<bool> s_AutoUpdatePreview = new Pref<bool>("editor.autoUpdatePreview", false, SettingsScope.Project);
        internal static bool delayedPreview => !s_AutoUpdatePreview.value;

        MenuAction m_CurrentAction;

        bool m_HasPreview;
        internal static bool hasPreview => s_Instance?.m_HasPreview ?? false;

        bool m_SelectionUpdateDisabled = false;
        internal static bool selectionUpdateDisabled
        {
            set
            {
                if (s_Instance != null)
                    s_Instance.m_SelectionUpdateDisabled = value;
            }
        }

        Overlay m_Overlay;

        static PreviewActionManager s_Instance;
        internal static string actionName => s_Instance?.m_CurrentAction?.menuTitle ?? "";
        internal static string actionTooltip => s_Instance?.m_CurrentAction?.tooltip.summary ?? "";

        void Init(MenuAction action, bool preview)
        {
            m_CurrentAction = action;
            m_HasPreview = preview;

            if (preview)
            {
                UndoUtility.StartPreview();
                m_CurrentAction.PerformAction();
            }

            // Changing selection/tool/context should apply the preview and exit the current action
            Selection.selectionChanged += ObjectSelectionChanged;
            ToolManager.activeContextChanged += Validate;
            ToolManager.activeToolChanged += Validate;
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged += SelectModeChanged;

            SceneView.AddOverlayToActiveView(m_Overlay = new MenuActionSettingsOverlay());
            m_Overlay.displayed = true;
        }

        public void Dispose()
        {
            s_Instance = null;
            SceneView.RemoveOverlayFromActiveView(m_Overlay);

            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged -= SelectModeChanged;
            ToolManager.activeContextChanged -= Validate;
            ToolManager.activeToolChanged -= Validate;
            Selection.selectionChanged -= ObjectSelectionChanged;
        }

        internal static bool IsCurrentAction(MenuAction action)
        {
            if(s_Instance == null || s_Instance.m_CurrentAction == null)
                return false;

            return s_Instance.m_CurrentAction.GetType() == action.GetType();
        }

        internal static void DoAction(MenuAction action, bool preview)
        {
            Validate();
            s_Instance = new PreviewActionManager();
            s_Instance.Init(action, preview);
        }

        internal static VisualElement GetContent()
        {
            return s_Instance?.m_CurrentAction?.CreateSettingsContent() ?? new VisualElement();
        }

        internal static void SetPreviewUpdate(bool value)
        {
            if(s_AutoUpdatePreview.value == value)
                return;

            s_AutoUpdatePreview.value = value;

            if (s_Instance == null)
                return;

            SceneView.RemoveOverlayFromActiveView(s_Instance.m_Overlay);
            SceneView.AddOverlayToActiveView(s_Instance.m_Overlay = new MenuActionSettingsOverlay());
            s_Instance.m_Overlay.displayed = true;
        }

        internal static void EndPreview()
        {
            s_Instance?.Dispose();
        }

        internal static void Validate()
        {
            s_Instance?.ValidateInternal();
        }

        void ValidateInternal()
        {
            Dispose();
            if (m_HasPreview)
                UndoUtility.ExitAndValidatePreview();
            else
                m_CurrentAction.PerformAction();
        }

        internal static void Cancel()
        {
            s_Instance?.CancelInternal();
        }

        void CancelInternal()
        {
            Dispose();
            if(m_HasPreview)
                UndoUtility.UndoPreview();
        }

        void ObjectSelectionChanged()
        {
            if (!m_SelectionUpdateDisabled)
                Validate();

            m_SelectionUpdateDisabled = false;
        }

        void SelectModeChanged(SelectMode _) => Validate();

        void OnSelectionUpdated(IEnumerable<ProBuilderMesh> _)
        {
            Validate();
        }

        internal static void UpdatePreview()
        {
            s_Instance?.UpdatePreviewInternal();
        }

        internal void UpdatePreviewInternal()
        {
            //Undo action might be triggering a refresh of the mesh and of the selection, so we need to temporarily unregister to these events
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            Selection.selectionChanged -= ObjectSelectionChanged;
            UndoUtility.StartPreview();
            m_CurrentAction.PerformAction();
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            Selection.selectionChanged += ObjectSelectionChanged;
        }
    }

    class MenuActionSettingsOverlay : Overlay
    {
        public MenuActionSettingsOverlay()
        {
            displayName = PreviewActionManager.actionName;
        }

        public override VisualElement CreatePanelContent()
        {
            rootVisualElement.tooltip = PreviewActionManager.actionTooltip;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.minWidth = root.style.maxWidth = 300;

            var lastLine = new VisualElement();
            lastLine.style.flexDirection = FlexDirection.Row;
            var okButton = new Button(PreviewActionManager.Validate);
            okButton.text = "Confirm";
            okButton.style.flexGrow = 1;
            var cancelButton = new Button(PreviewActionManager.Cancel);
            cancelButton.text = "Cancel";
            cancelButton.style.flexGrow = 1;
            lastLine.Add(okButton);
            lastLine.Add(cancelButton);

            var settingsElement = PreviewActionManager.GetContent();
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

    /// <summary>
    /// An EditorAction for displaying MenuAction settings overlay and action previewing.
    /// </summary>
    public class MenuActionSettings : EditorAction
    {
        static bool s_CanTriggerNewAction = true;
        
        /// <summary>
        /// MenuActionSettings constructor.
        /// </summary>
        /// <param name="action">The MenuAction for which to display a settings overlay.</param>
        /// <param name="hasPreview">Indicates if the action can be previewed.</param>
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

        internal static bool IsCurrentAction(MenuAction action)
        {
            return PreviewActionManager.IsCurrentAction(action);
        }
    }
}
