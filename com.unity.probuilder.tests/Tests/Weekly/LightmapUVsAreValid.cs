using System.Linq;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.SceneManagement;

namespace UnityEditor.ProBuilder.Tests.Slow
{
    class LightmapUVsAreValid
    {
        const string k_LightmapUnitTestsDir = "Assets/LightmappingUnitTests";
        const string k_LightmapUnitTestsScene = "Assets/LightmappingUnitTests/LightmappingUnitTestScene.unity";

        static void Setup()
        {
            if (!Directory.Exists(k_LightmapUnitTestsDir))
                Directory.CreateDirectory(k_LightmapUnitTestsDir);

            AssetDatabase.Refresh();

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene, k_LightmapUnitTestsScene, false);
        }

        static void Cleanup()
        {
            Directory.Delete(k_LightmapUnitTestsDir, true);
            File.Delete(k_LightmapUnitTestsDir + ".meta");
            AssetDatabase.Refresh();
        }

        static bool s_FinishedBaking;

        static void LightmappingStarted()
        {
            s_FinishedBaking = false;
        }

        static void LightmappingCompleted()
        {
            s_FinishedBaking = true;
        }

        [UnityTest]
        public IEnumerator DefaultUnwrapParamsDoNotOverlap()
        {
            var lightmapMode = Lightmapping.giWorkflowMode;

            try
            {
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

                Lightmapping.started += LightmappingStarted;

                if (Lightmapping.completed == null)
                    Lightmapping.completed = LightmappingCompleted;
                else
                    Lightmapping.completed += LightmappingCompleted;

                Setup();

                float x = -10f;

                using (var shapes = new TestUtility.BuiltInPrimitives())
                {
                    SceneModeUtility.SetStaticFlags(shapes.Select(it => it.gameObject).ToArray(), (int)StaticEditorFlags.LightmapStatic, true);

                    foreach (ProBuilderMesh mesh in shapes)
                    {
                        mesh.transform.position = new Vector3(x, 0f, 0f);
                        x += mesh.mesh.bounds.size.x + .5f;
                        mesh.Optimize(true);
                    }

                    Lightmapping.BakeAsync();

                    while (!s_FinishedBaking)
                        yield return null;

                    LogAssert.NoUnexpectedReceived();
                }
            }
            finally
            {
                Lightmapping.started -= LightmappingStarted;
                Lightmapping.completed -= LightmappingCompleted;
                Lightmapping.giWorkflowMode = lightmapMode;
                Cleanup();
            }
        }
    }
}
