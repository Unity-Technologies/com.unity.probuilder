using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Base class from which any action that is represented in the ProBuilder toolbar inherits.
    /// </summary>
    /// <remarks>
    /// A MenuToolToggle is a special action that creates an <see cref="UnityEditor.EditorTools.EditorTool"/> instance and sets it as the active tool.
    /// </remarks>
    public abstract class MenuToolToggle : MenuAction
    {
        /// <summary>
        /// Holds a reference to the <see cref="UnityEditor.EditorTools.EditorTool"/> instance created by the action.
        /// </summary>
        protected EditorTool m_Tool;

        /// <summary>
        /// Gets a reference to the <see cref="UnityEditor.EditorTools.EditorTool"/> instance created by the action.
        /// </summary>
        public EditorTool Tool => m_Tool;

        protected override ActionResult PerformActionImplementation()
        {
            if(m_Tool == null)
                return ActionResult.NoSelection;

            if(ToolManager.IsActiveTool(m_Tool))
                ToolManager.RestorePreviousTool();
            else
                ToolManager.SetActiveTool(m_Tool);

            return ActionResult.Success;
        }
    }
}
