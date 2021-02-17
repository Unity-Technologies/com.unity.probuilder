using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;

[CustomEditor(typeof(PolySplineShape))]
public class PolySplineShapeEditor : Editor
{
    void UpdateProBuilderEditor()
    {
        ProBuilderEditor.Refresh(true);
        SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if(EditorGUI.EndChangeCheck())
        {
            UpdateProBuilderEditor();
        }

        foreach(var t in targets)
        {
            if(((PolySplineShape)t).isPBEditorDirty)
            {
                UpdateProBuilderEditor();
                ( (PolySplineShape) t ).isPBEditorDirty = false;
            }
        }
    }
}
