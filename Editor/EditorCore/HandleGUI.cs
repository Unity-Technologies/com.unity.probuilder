using System;

namespace UnityEditor.ProBuilder
{
    sealed class HandleGUI : IDisposable
    {
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
