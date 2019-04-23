using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    sealed class HandleGUI : IDisposable
    {
        bool m_SrgbWrite;

        public HandleGUI()
        {
            Handles.BeginGUI();
#if !UNITY_2018_2_OR_NEWER
            m_SrgbWrite = GL.sRGBWrite;
            GL.sRGBWrite = false;
#endif
        }

        public void Dispose()
        {
#if !UNITY_2018_2_OR_NEWER
            GL.sRGBWrite = m_SrgbWrite;
#endif
            Handles.EndGUI();
        }
    }
}
