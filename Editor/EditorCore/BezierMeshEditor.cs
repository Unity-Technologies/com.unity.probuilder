using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /*
     * to have undo
     *
     * - instead of calling extrude mesh in overlay, set it to dirty
     * here, check if mesh is set to dirty, if dirty extrude
     */
    [CustomEditor(typeof(BezierMesh))]
    sealed class BezierSplineChanged : Editor
    {
        private void OnEnable()
        {
            BezierMesh.BezierMeshModified += RefreshEditor;
        }

        private void OnDisable()
        {
            BezierMesh.BezierMeshModified -= RefreshEditor;
        }

        private void RefreshEditor()
        {
            ProBuilderEditor.Refresh();
        }
    }
}
