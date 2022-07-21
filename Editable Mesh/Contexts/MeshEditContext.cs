using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

// 1) Learn to create EditorToolContext
[EditorToolContext("Mesh Edit", typeof(MeshFilter))]
public class MeshEditContext : EditorToolContext
{
    public enum ElementEditMode
    {
        Vertex,
        Edge
    }
    
    public static ElementEditMode EditMode { get; set; }
    
    // 2) Learn to override built-in transform tools
    protected override Type GetEditorToolType(Tool tool)
    {
        switch (tool)
        {
            case Tool.Move:
                return typeof(MeshElementMoveTool);
            default:
                return null;
        }
    }
}
