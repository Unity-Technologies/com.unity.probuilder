using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderToolManager
    {
        // When tool contexts are fully implemented there should be no need for `SelectMode`
        static Pref<SelectMode> s_SelectMode = new Pref<SelectMode>("editor.selectMode", SelectMode.Face);
        static Pref<SelectMode> s_LastMeshSelectMode = new Pref<SelectMode>("editor.lastMeshSelectMode", SelectMode.Face);

        public static SelectMode selectMode
        {
            get => s_SelectMode.value;

            private set
            {
                if (value.IsMeshElementMode())
                    s_LastMeshSelectMode.SetValue(value);
                s_SelectMode.SetValue(value);
                ProBuilderSettings.Save();
                if (selectModeChanged != null)
                    selectModeChanged();
            }
        }

        public static event Action selectModeChanged = () => {};

        [InitializeOnLoadMethod]
        static void ProBuilderToolManagerInit()
        {
            SetSelectMode(s_SelectMode);
        }

        public static void SetSelectMode(SelectMode mode)
        {
            if (mode == selectMode)
                return;

            selectMode = mode;
        }

        public static void ResetToLastSelectMode()
        {
            SetSelectMode(s_LastMeshSelectMode);
        }

        public static void NextMeshSelectMode()
        {
            if (s_SelectMode == SelectMode.Vertex)
                selectMode = SelectMode.Edge;
            else if (s_SelectMode == SelectMode.Edge)
                selectMode = SelectMode.Face;
            else if (s_SelectMode == SelectMode.Face)
                selectMode = SelectMode.Vertex;
            if (s_SelectMode == SelectMode.TextureVertex)
                selectMode = SelectMode.TextureEdge;
            else if (s_SelectMode == SelectMode.TextureEdge)
                selectMode = SelectMode.TextureFace;
            else if (s_SelectMode == SelectMode.TextureFace)
                selectMode = SelectMode.TextureVertex;
        }

        public static Tool activeTool => Tools.current;

    }
}
