using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
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
