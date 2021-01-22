#if UNITY_2020_2_OR_NEWER
#define TOOL_CONTEXTS_ENABLED
#endif

using System;
using System.Diagnostics;
using UnityEditor.EditorTools;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
using System.Collections.Generic;
#endif

namespace UnityEditor.ProBuilder
{
    // Handles forwarding the Unity tool to ProBuilder editor
    class ProBuilderToolManager : IDisposable
    {
        // When tool contexts are fully implemented there should be no need for `SelectMode`
        static Pref<SelectMode> s_SelectMode = new Pref<SelectMode>("editor.selectMode", SelectMode.Object);
        static Pref<SelectMode> s_LastMeshSelectMode = new Pref<SelectMode>("editor.lastMeshSelectMode", SelectMode.Object);

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

        bool m_IsDisposed = false;

#if !TOOL_CONTEXTS_ENABLED
        const int k_BuiltinToolCount = (int) Tool.Custom + 1;
        Type[] m_DefaultTools;
        EditorTool[] m_VertexTools;
        EditorTool[] m_TextureTools;
        static readonly Dictionary<Type, Tool> k_ToolTypeMap = new Dictionary<Type, Tool>()
        {
            { typeof(ProbuilderMoveTool), Tool.Move },
            { typeof(ProbuilderRotateTool), Tool.Rotate },
            { typeof(ProbuilderScaleTool), Tool.Scale },
            { typeof(TextureMoveTool), Tool.Move },
            { typeof(TextureRotateTool), Tool.Rotate },
            { typeof(TextureScaleTool), Tool.Scale }
        };
#endif

        public ProBuilderToolManager()
        {
#if !TOOL_CONTEXTS_ENABLED
            EditorApplication.update += ForwardBuiltinToolCheck;

            m_DefaultTools = new Type[k_BuiltinToolCount];
            m_VertexTools = new EditorTool[k_BuiltinToolCount];
            m_TextureTools = new EditorTool[k_BuiltinToolCount];

            m_DefaultTools[(int) Tool.Move] = typeof(MoveTool);
            m_DefaultTools[(int) Tool.Rotate] = typeof(RotateTool);
            m_DefaultTools[(int) Tool.Scale] = typeof(ScaleTool);

            m_VertexTools[(int) Tool.Move] = ScriptableObject.CreateInstance<ProbuilderMoveTool>();
            m_VertexTools[(int) Tool.Rotate] = ScriptableObject.CreateInstance<ProbuilderRotateTool>();
            m_VertexTools[(int) Tool.Scale] = ScriptableObject.CreateInstance<ProbuilderScaleTool>();

            m_TextureTools[(int) Tool.Move] = ScriptableObject.CreateInstance<TextureMoveTool>();
            m_TextureTools[(int) Tool.Rotate] = ScriptableObject.CreateInstance<TextureRotateTool>();
            m_TextureTools[(int) Tool.Scale] = ScriptableObject.CreateInstance<TextureScaleTool>();

            for (int i = (int) Tool.Move; i <= (int) Tool.Scale; i++)
            {
                m_VertexTools[i].hideFlags = HideFlags.HideAndDontSave;
                m_TextureTools[i].hideFlags = HideFlags.HideAndDontSave;
            }
#endif
            SetSelectMode(s_SelectMode);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
                return;
            m_IsDisposed = true;
            GC.SuppressFinalize(this);

            SetSelectMode(SelectMode.Object);

#if !TOOL_CONTEXTS_ENABLED
            EditorApplication.update -= ForwardBuiltinToolCheck;

            for(int i = 0, c = m_VertexTools.Length; i < c; i++)
                if (m_VertexTools[i] != null)
                    UObject.DestroyImmediate(m_VertexTools[i]);
            for(int i = 0, c = m_TextureTools.Length; i < c; i++)
                if (m_TextureTools[i] != null)
                    UObject.DestroyImmediate(m_TextureTools[i]);
#endif
        }

        public void SetSelectMode(SelectMode mode)
        {
            if (mode == selectMode)
                return;

            selectMode = mode;

#if TOOL_CONTEXTS_ENABLED
            if (selectMode.IsPositionMode() && ToolManager.activeContextType != typeof(PositionToolContext))
                ToolManager.SetActiveContext<PositionToolContext>();
            else if (selectMode.IsTextureMode() && ToolManager.activeContextType != typeof(TextureToolContext))
                ToolManager.SetActiveContext<TextureToolContext>();
            else if (!selectMode.IsMeshElementMode())
                ToolManager.SetActiveContext<GameObjectToolContext>();
#else
            var tool = activeTool;

            if (tool == Tool.None)
                ToolManager.SetActiveTool<MoveTool>();
            else  if(mode.IsPositionMode() && m_VertexTools[(int)tool] != null)
                ToolManager.SetActiveTool(m_VertexTools[(int)tool]);
            else if(mode.IsTextureMode() && m_TextureTools[(int)tool] != null)
                ToolManager.SetActiveTool(m_TextureTools[(int)tool]);
            else if (mode == SelectMode.Object && GetBuiltinToolType(ToolManager.activeToolType, out Type builtin))
                ToolManager.SetActiveTool(builtin);
#endif
        }

        public void ResetToLastSelectMode()
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

        public static Tool activeTool
        {
            get
            {
#if TOOL_CONTEXTS_ENABLED
                return Tools.current;
#else
                if (ToolManager.activeToolType != null && k_ToolTypeMap.TryGetValue(ToolManager.activeToolType, out Tool tool))
                    return tool;
                return Tools.current;
#endif
            }
        }

        // Can't do this in `activeToolChanged` because it is forbidden by ToolManager to prevent recursion
        internal void ForwardBuiltinToolCheck()
        {
#if !TOOL_CONTEXTS_ENABLED
            if(selectMode.IsMeshElementMode() && GetProBuilderToolType(ToolManager.activeToolType, out EditorTool tool))
                ToolManager.SetActiveTool(tool);
#endif
        }

#if !TOOL_CONTEXTS_ENABLED
        bool GetBuiltinToolType(Type type, out Type builtin)
        {
            for (int i = 0; i < k_BuiltinToolCount; i++)
            {
                if (m_VertexTools[i]?.GetType() == type || m_TextureTools[i]?.GetType() == type)
                {
                    builtin = m_DefaultTools[i];
                    return true;
                }
            }

            builtin = null;
            return false;
        }

        bool GetProBuilderToolType(Type type, out EditorTool tool)
        {
            for (int i = 0; i < k_BuiltinToolCount; i++)
            {
                if (m_DefaultTools[i] == type)
                {
                    if(selectMode.IsPositionMode())
                        tool = m_VertexTools[i];
                    else if (selectMode.IsTextureMode())
                        tool = m_TextureTools[i];
                    else
                        continue;

                    return true;
                }
            }

            tool = null;
            return false;
        }
#endif
    }
}
