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
        }

        public void Dispose()
        {
            Handles.EndGUI();
        }
    }
}
