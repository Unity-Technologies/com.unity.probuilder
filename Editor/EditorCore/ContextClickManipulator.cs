using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    class ContextClickManipulator : MouseManipulator
    {
        readonly Action m_Action;
        // m_Active is necessary because we want context clicks to fire on mouse up, and CanStopManipulation only checks
        // that the mouse button matches the current activator. in our case, the modifier is rmb + alt, so "CanStop"
        // passes regardless of modifier state or "CanStart" result.
        bool m_Active;

        public ContextClickManipulator(Action action)
        {
            m_Action = action;
            m_Active = false;

            activators.Add(new ManipulatorActivationFilter() {
                button = MouseButton.RightMouse
            });

            activators.Add(new ManipulatorActivationFilter() {
                button = MouseButton.LeftMouse, modifiers = EventModifiers.Alt
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                return;
            m_Active = true;
            evt.StopPropagation();
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            if (CanStopManipulation(evt) && m_Active)
            {
                m_Action();
                evt.StopPropagation();
            }
        }
    }
}
