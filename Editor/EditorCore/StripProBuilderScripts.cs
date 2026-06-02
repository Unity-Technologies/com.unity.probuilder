using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;

namespace UnityEditor.ProBuilder.Actions
{
    /// <summary>
    /// Menu items for stripping ProBuilder scripts from GameObjects.
    /// </summary>
    sealed class StripProBuilderScripts : Editor
    {
        const string k_UndoMessage = "Strip ProBuilder Scripts";

        // return ProBuilderMesh components in loaded scenes only for the current stage
        static List<ProBuilderMesh> GetMeshesInActiveScenes()
        {
            var stage = StageNavigationManager.instance.currentStage;
            var c = stage.sceneCount;
            var scenes = new HashSet<Scene>();
            for (int i = 0; i < c; ++i)
                scenes.Add(stage.GetSceneAt(i));
            var filtered = new List<ProBuilderMesh>();
            foreach(var mesh in Resources.FindObjectsOfTypeAll<ProBuilderMesh>())
                if(scenes.Contains(mesh.gameObject.scene))
                    filtered.Add(mesh);
            return filtered;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip All ProBuilder Scripts in Scene")]
        public static void StripAllScenes()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts in the scene. You will no longer be able to edit these objects.\n\nContinue?", "Okay", "Cancel"))
                return;

            var all = GetMeshesInActiveScenes();
            for (int i = 0, c = all?.Count ?? 0; i < c; i++)
            {
                if (c > 32 && UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                        "Stripping ProBuilder Scripts",
                        "Working over " + all[i].GetObjectId() + ".",
                        ((float)i / all.Count)))
                    break;

                DoStrip(all[i], true);
            }

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "Successfully stripped out all ProBuilder components.", "Okay");

            ProBuilderEditor.Refresh();
            MeshSelection.OnObjectSelectionChanged();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection", true, 0)]
        public static bool VerifyStripSelection()
        {
            return InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Length > 0;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip ProBuilder Scripts in Selection")]
        public static void StripAllSelected()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts on the selected objects. You will no longer be able to edit these objects.\n\nContinue?", "Okay", "Cancel"))
                return;

            foreach (Transform t in Selection.transforms)
            {
                foreach (ProBuilderMesh pb in t.GetComponentsInChildren<ProBuilderMesh>(true))
                    DoStrip(pb);
            }
            MeshSelection.OnObjectSelectionChanged();
        }

        public static void DoStrip(ProBuilderMesh pb, bool undo = false)
        {
            GameObject go = pb.gameObject;

            if (go.TryGetComponent<Renderer>(out var ren))
                EditorUtility.SetSelectionRenderState(ren, EditorSelectedRenderState.Highlight | EditorSelectedRenderState.Wireframe);

            EditorUtility.SynchronizeWithMeshFilter(pb);

            if (pb.mesh == null)
            {
                DestroyProBuilderMeshAndDependencies(go, pb, false, undo);
                return;
            }

            // if meshes are assets and the mesh cache is valid don't duplicate the mesh to an instance.
            if (Experimental.meshesAreAssets && EditorMeshUtility.GetCachedMesh(pb, out _, out _))
            {
                DestroyProBuilderMeshAndDependencies(go, pb, true, undo);
            }
            else
            {
                Mesh instance = Instantiate(pb.mesh);
                var path = $"{EditorUtility.GetActiveSceneAssetsPath()}/{pb.mesh.name}.asset";
                AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(path));
                DestroyProBuilderMeshAndDependencies(go, pb, false, undo);

                var filter = go.GetComponent<MeshFilter>();

                if(undo)
                    Undo.RecordObject(filter, k_UndoMessage);

                filter.sharedMesh = instance;
                PrefabUtility.RecordPrefabInstancePropertyModifications(filter);

                if (go.TryGetComponent(out MeshCollider collider))
                {
                    if(undo)
                        Undo.RecordObject(collider, k_UndoMessage);
                    collider.sharedMesh = instance;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
                }
            }
        }

        static void Destroy(Object o, bool undo)
        {
            if(undo)
                Undo.DestroyObjectImmediate(o);
            else
                DestroyImmediate(o);
        }

        internal static void DestroyProBuilderMeshAndDependencies(
            GameObject go,
            ProBuilderMesh pb,
            bool preserveMeshAssets = false,
            bool useUndoDestroy = false)
        {
            if(useUndoDestroy)
                Undo.RecordObject(pb, k_UndoMessage);

            if (go.TryGetComponent(out PolyShape polyShape))
                Destroy(polyShape, useUndoDestroy);

            if (go.TryGetComponent(out BezierShape bezierShape))
                Destroy(bezierShape, useUndoDestroy);

            if (go.TryGetComponent(out ProBuilderShape shape))
                Destroy(shape, useUndoDestroy);

            pb.preserveMeshAssetOnDestroy = preserveMeshAssets;

            Destroy(pb, useUndoDestroy);

            if(go.TryGetComponent(out Entity entity))
                Destroy(entity, useUndoDestroy);

            PrefabUtility.RecordPrefabInstancePropertyModifications(go);
        }
    }
}
