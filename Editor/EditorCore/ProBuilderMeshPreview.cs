using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomPreview(typeof(ProBuilderMesh))]
    sealed class ProBuilderMeshPreview : ObjectPreview
    {
        MeshPreview m_MeshPreview;
        Mesh[] m_GeneratedMeshAssets = new Mesh [1];

        public override void Initialize(Object[] targets)
        {
            base.Initialize(targets);

            foreach (var obj in targets)
            {
                if(!(obj is ProBuilderMesh probuilderMesh))
                    continue;

                var mesh = new Mesh();
                UnityEngine.ProBuilder.MeshUtility.Compile(probuilderMesh, mesh);
                m_MeshPreview = new MeshPreview(mesh);

                // The inspector only ever draws the target mesh, so there's no point in creating preview meshes for
                // the entire selection
                return;
            }

            m_MeshPreview = new MeshPreview(null);
        }

        public override void Cleanup()
        {
            m_MeshPreview.Dispose();
            foreach (var kvp in m_GeneratedMeshAssets)
                Object.DestroyImmediate(kvp);
            base.Cleanup();
        }

        public override void OnPreviewSettings() => m_MeshPreview.OnPreviewSettings();

        public override bool HasPreviewGUI()
        {
            if (target == null || !(target is ProBuilderMesh mesh) || mesh.gameObject == null)
                return false;
            return PrefabUtility.IsPartOfPrefabAsset(mesh.gameObject);
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background) => m_MeshPreview.OnPreviewGUI(rect, background);

        public override string GetInfoString() => MeshPreview.GetInfoString(m_GeneratedMeshAssets[0]);
    }
}
