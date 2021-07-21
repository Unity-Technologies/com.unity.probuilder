using System.Collections.Generic;
using System.Text;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Common troubleshooting actions for repairing ProBuilder meshes.
    /// </summary>
    /// @TODO MOVE THESE TO ACTIONS
    static class RepairActions
    {
        /// <summary>
        /// Menu interface for manually re-generating all ProBuilder geometry in scene.
        /// </summary>
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Repair/Rebuild All ProBuilder Objects", false, PreferenceKeys.menuRepair)]
        public static void MenuForceSceneRefresh()
        {
            StringBuilder sb = new StringBuilder();
            ProBuilderMesh[] all = Object.FindObjectsOfType<ProBuilderMesh>();

            for (int i = 0, l = all.Length; i < l; i++)
            {
                UnityEditor.EditorUtility.DisplayProgressBar(
                    "Refreshing ProBuilder Objects",
                    "Reshaping pb_Object " + all[i].id + ".",
                    ((float)i / all.Length));

                try
                {
                    all[i].ToMesh();
                    all[i].Refresh();
                    all[i].Optimize();
                }
                catch (System.Exception e)
                {
                    if (!ReProBuilderize(all[i]))
                        sb.AppendLine("Failed rebuilding: " + all[i].ToString() + "\n\t" + e.ToString());
                }
            }

            if (sb.Length > 0)
                Log.Error(sb.ToString());

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("Refresh ProBuilder Objects",
                "Successfully refreshed all ProBuilder objects in scene.",
                "Okay");
        }

        static bool ReProBuilderize(ProBuilderMesh pb)
        {
            try
            {
                GameObject go = pb.gameObject;
                pb.preserveMeshAssetOnDestroy = true;
                Undo.DestroyObjectImmediate(pb);

                // don't delete pb_Entity here because it won't
                // actually get removed till the next frame, and
                // probuilderize wants to add it if it's missing
                // (which it looks like it is from c# side but
                // is not)

                pb = Undo.AddComponent<ProBuilderMesh>(go);
                InternalMeshUtility.ResetPbObjectWithMeshFilter(pb, true);

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();

                return true;
            }
            catch
            {
                return false;
            }
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Repair/Rebuild Shared Indexes Cache", true, PreferenceKeys.menuRepair)]
        static bool VertifyRebuildMeshes()
        {
            return InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Length > 0;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Repair/Rebuild Shared Indexes Cache", false, PreferenceKeys.menuRepair)]
        public static void DoRebuildMeshes()
        {
            RebuildSharedIndexes(InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms));
        }

        /// <summary>
        /// Rebuild targets if they can't be refreshed.
        /// </summary>
        /// <param name="targets"></param>
        static void RebuildSharedIndexes(ProBuilderMesh[] targets)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < targets.Length; i++)
            {
                UnityEditor.EditorUtility.DisplayProgressBar(
                    "Refreshing ProBuilder Objects",
                    "Rebuilding mesh " + targets[i].id + ".",
                    ((float)i / targets.Length));

                ProBuilderMesh pb = targets[i];

                try
                {
                    pb.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(pb.positionsInternal);

                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();
                }
                catch (System.Exception e)
                {
                    sb.AppendLine("Failed rebuilding " + pb.name + " shared indexes cache.\n" + e.ToString());
                }
            }

            if (sb.Length > 0)
                Log.Error(sb.ToString());

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("Rebuild Shared Index Cache", "Successfully rebuilt " + targets.Length + " shared index caches", "Okay");
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Repair/Fix Meshes in Selection", false, PreferenceKeys.menuRepair)]
        public static void MenuRemoveDegenerateTriangles()
        {
            int count = 0;

            foreach (ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
            {
                int removedVertexCount;

                if(!MeshValidation.EnsureMeshIsValid(pb, out removedVertexCount))
                {
                    pb.Rebuild();
                    pb.Optimize();
                    count += removedVertexCount;
                }
            }

            Debug.Log("Removed " + count + " vertices \nbelonging to degenerate triangles.");
            EditorUtility.ShowNotification("Removed " + count + " vertices \nbelonging to degenerate triangles.");
        }
    }
}
