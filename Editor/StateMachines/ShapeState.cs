using System;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    [Serializable]
    abstract class ShapeState
    {
        public static ShapeState s_defaultState;
        public ShapeState m_nextState = null;
        public static DrawShapeTool tool;

        public static ShapeState StartStateMachine()
        {
            if(tool == null)
            {
                Debug.LogError("Cannot start FSM, no tool associated to the FSM");
                return null;
            }
            else if(s_defaultState == null)
            {
                Debug.LogError("Cannot start FSM, default state has not be set");
                return null;
            }

            s_defaultState.InitState();
            return s_defaultState;
        }

        protected virtual void InitState()
        {
        }

        public abstract ShapeState DoState(Event evt);

        protected virtual void EndState()
        {
        }

        protected virtual ShapeState NextState()
        {
            EndState();
            if(m_nextState == null)
                return ResetState();

            m_nextState.InitState();
            SceneView.RepaintAll();
            return m_nextState;
        }

        public static ShapeState ResetState()
        {
            if (tool.m_ProBuilderShape != null)
                UnityEngine.Object.DestroyImmediate(tool.m_ProBuilderShape.gameObject);

            s_defaultState.InitState();
            SceneView.RepaintAll();
            return s_defaultState;
        }
    }
}
