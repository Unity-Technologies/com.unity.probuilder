using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
    /// <summary>
    /// Menu items for stripping ProBuilder scripts from GameObjects.
    /// </summary>
    /// @TODO MOVE TO ACTIONS
    sealed class StripProBuilderScripts : Editor
    {
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip All ProBuilder Scripts in Scene")]
        public static void StripAllScenes()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts in the scene.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
                return;

            ProBuilderMesh[] all = (ProBuilderMesh[])Resources.FindObjectsOfTypeAll(typeof(ProBuilderMesh));

            Strip(all);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection", true, 0)]
        public static bool VerifyStripSelection()
        {
            return InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Length > 0;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection")]
        public static void StripAllSelected()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts on the selected objects.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
                return;

            foreach (Transform t in Selection.transforms)
            {
                foreach (ProBuilderMesh pb in t.GetComponentsInChildren<ProBuilderMesh>(true))
                    DoStrip(pb);
            }
            MeshSelection.OnObjectSelectionChanged();
        }

        public static void Strip(ProBuilderMesh[] all)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                        "Stripping ProBuilder Scripts",
                        "Working over " + all[i].id + ".",
                        ((float)i / all.Length)))
                    break;

                DoStrip(all[i]);
            }

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "Successfully stripped out all ProBuilder components.", "Okay");

            ProBuilderEditor.Refresh();
            MeshSelection.OnObjectSelectionChanged();
        }

        public static void DoStrip(ProBuilderMesh pb)
        {
            try
            {
                GameObject go = pb.gameObject;

                Renderer ren = go.GetComponent<Renderer>();

                if (ren != null)
                    EditorUtility.SetSelectionRenderState(ren, EditorSelectedRenderState.Highlight | EditorSelectedRenderState.Wireframe);

                if (EditorUtility.IsPrefabAsset(go))
                    return;

                EditorUtility.SynchronizeWithMeshFilter(pb);


                if (go.TryGetComponent(out PolyShape polyShape))
                    DestroyImmediate(polyShape);

                if (go.TryGetComponent(out BezierShape bezierShape))
                    DestroyImmediate(bezierShape);

                if (pb.mesh == null)
                {
                    DestroyImmediate(pb);

                    if (go.TryGetComponent(out Entity entity))
                        DestroyImmediate(entity);

                    return;
                }

                string cachedMeshPath;
                Mesh cachedMesh;

                // if meshes are assets and the mesh cache is valid don't duplicate the mesh to an instance.
                if (Experimental.meshesAreAssets && EditorMeshUtility.GetCachedMesh(pb, out cachedMeshPath, out cachedMesh))
                {
                    pb.preserveMeshAssetOnDestroy = true;
                    DestroyImmediate(pb);
                    if (go.TryGetComponent(out Entity entity))
                        DestroyImmediate(entity);
                }
                else
                {
                    Mesh m = UnityEngine.ProBuilder.MeshUtility.DeepCopy(pb.mesh);

                    DestroyImmediate(pb);

                    if (go.TryGetComponent(out Entity entity))
                        DestroyImmediate(entity);

                    go.GetComponent<MeshFilter>().sharedMesh = m;
                    if (go.TryGetComponent(out MeshCollider meshCollider))
                        meshCollider.sharedMesh = m;
                }
            }
            catch {}
        }
    }
}
