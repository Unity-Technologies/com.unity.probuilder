using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder.Actions
{
    /// <summary>
    /// Menu items for stripping ProBuilder scripts from GameObjects.
    /// </summary>
    /// @TODO MOVE TO ACTIONS
    internal sealed class StripProBuilderScripts : Editor
    {
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip All ProBuilder Scripts in Scene")]
        public static void StripAllScenes()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts in the scene.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
                return;

            ProBuilderMesh[] all = (ProBuilderMesh[])Resources.FindObjectsOfTypeAll(typeof(ProBuilderMesh));

            Strip(all);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection %#s", true, 0)]
        public static bool VerifyStripSelection()
        {
            return InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Length > 0;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection %#s")]
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
                        "Working over " + all[i].GetInstanceID() + ".",
                        ((float)i / all.Length)))
                    break;

                DoStrip(all[i]);
            }

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "Successfully stripped out all ProBuilder components.", "Okay");

            ProBuilderEditor.Refresh();
            MeshSelection.OnObjectSelectionChanged();
            AssetDatabase.Refresh();
        }

        public static void DoStrip(ProBuilderMesh pb)
        {
            try
            {
                GameObject go = pb.gameObject;

                if (go.TryGetComponent<Renderer>(out var ren))
                    EditorUtility.SetSelectionRenderState(ren, EditorSelectedRenderState.Highlight | EditorSelectedRenderState.Wireframe);

                if (EditorUtility.IsPrefabAsset(go))
                    return;

                EditorUtility.SynchronizeWithMeshFilter(pb);

                if (pb.mesh == null)
                {
                    DestroyProBuilderMeshAndDependencies(go, pb, false);
                    return;
                }

                // if meshes are assets and the mesh cache is valid don't duplicate the mesh to an instance.
                if (Experimental.meshesAreAssets && EditorMeshUtility.GetCachedMesh(pb, out _, out _))
                {
                    DestroyProBuilderMeshAndDependencies(go, pb, true);
                }
                else
                {
                    Mesh instance = Instantiate(pb.mesh);
                    var path = $"{EditorUtility.GetActiveSceneAssetsPath()}/{pb.mesh.name}.asset";
                    AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(path));
                    DestroyProBuilderMeshAndDependencies(go, pb);

                    go.GetComponent<MeshFilter>().sharedMesh = instance;
                    if (go.TryGetComponent(out MeshCollider meshCollider))
                        meshCollider.sharedMesh = instance;
                }
            }
            catch {}
        }

        internal static void DestroyProBuilderMeshAndDependencies(
            GameObject go,
            ProBuilderMesh pb,
            bool preserveMeshAssets = false,
            bool useUndoDestroy = false)
        {
            if(useUndoDestroy)
                Undo.RecordObject(pb, "Removing ProBuilderMesh during scripts striping");

            if (go.TryGetComponent(out PolyShape polyShape))
            {
                if(useUndoDestroy)
                    Undo.DestroyObjectImmediate(polyShape);
                else
                    DestroyImmediate(polyShape);
            }

            if (go.TryGetComponent(out BezierShape bezierShape))
            {
                if(useUndoDestroy)
                    Undo.DestroyObjectImmediate(bezierShape);
                else
                    DestroyImmediate(bezierShape);
            }

            if (go.TryGetComponent(out ProBuilderShape shape))
            {
                if(useUndoDestroy)
                    Undo.DestroyObjectImmediate(shape);
                else
                    DestroyImmediate(shape);
            }

            pb.preserveMeshAssetOnDestroy = preserveMeshAssets;
            if(useUndoDestroy)
                Undo.DestroyObjectImmediate(pb);
            else
                DestroyImmediate(pb);

            if(go.TryGetComponent(out Entity entity))
            {
                if(useUndoDestroy)
                    Undo.DestroyObjectImmediate(entity);
                else
                    DestroyImmediate(entity);
            }
        }
    }
}
