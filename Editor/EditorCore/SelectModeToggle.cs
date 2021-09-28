using System;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    public class SelectModeToggle: EditorToolbarToggle
    {
        readonly SelectMode m_TargetMode;

        public SelectModeToggle(SelectMode targetMode, string name, Texture2D icon)
        {
            m_TargetMode = targetMode;
            this.name = name;
            this.icon = icon;
            tooltip = L10n.Tr(name);

            this.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue)
                    ProBuilderEditor.selectMode = m_TargetMode;

                if (m_TargetMode == ProBuilderEditor.selectMode)
                    SetValueWithoutNotify(true);
            });

            UpdateState();
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            ProBuilderToolManager.selectModeChanged += UpdateState;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            ProBuilderToolManager.selectModeChanged -= UpdateState;
        }

        void UpdateState()
        {
            SetValueWithoutNotify(IsActiveSelectMode());
        }

        bool IsActiveSelectMode()
        {
            return ProBuilderToolManager.selectMode == m_TargetMode;
        }
    }
}
