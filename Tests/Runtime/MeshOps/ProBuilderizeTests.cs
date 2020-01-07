using System.IO;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    class ProBuilderizeTests : TemporaryAssetTest
    {
        [Test]
        public static void ImportCube_MatchesDefaultCube()
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

            try
            {
#pragma warning disable 612
                var dup = new GameObject().AddComponent<ProBuilderMesh>();
                var importer = new MeshImporter(dup);
                importer.Import(pb.gameObject);
#pragma warning restore 612
                dup.ToMesh();
                dup.Refresh();
                TestUtility.AssertAreEqual(pb.mesh, dup.mesh, message: pb.name);
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(pb.gameObject);
            }
        }

        [Test]
        public static void ImportMeshWithQuads_MatchesWindingOrder()
        {
            var srcPath = TestUtility.temporarySavedAssetsDirectory + "maya-cube-quads.fbx";

            // do this song and dance because AssetDatabase.LoadAssetAtPath doesn't seem to work with models in the
            // Package directories
            File.Copy(TestUtility.templatesDirectory + "MeshImporter/maya-cube-quads.fbx", srcPath);
            AssetDatabase.Refresh();
            var source = AssetDatabase.LoadMainAssetAtPath(srcPath);
            var meshImporter = (ModelImporter)AssetImporter.GetAtPath(srcPath);
            meshImporter.globalScale = 100f;
            meshImporter.isReadable = true;
            meshImporter.SaveAndReimport();

            Assert.IsNotNull(source);

            var instance = (GameObject)UObject.Instantiate(source);
            var mf = instance.gameObject.GetComponent<MeshFilter>();
            var mr = instance.gameObject.GetComponent<MeshRenderer>();
            var result = new GameObject().AddComponent<ProBuilderMesh>();
            var importer = new MeshImporter(mf.sharedMesh, mr.sharedMaterials, result);

            Assert.DoesNotThrow(() =>
            {
                importer.Import(new MeshImportSettings()
                {
                    quads = true,
                    smoothing = false,
                    smoothingAngle = 1f
                });
            });

            result.Rebuild();

            // Assert.IsNotNull doesn't work with  UnityObject magic
            Assert.IsFalse(result.mesh == null);

#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(result.mesh, "imported-cube-triangles");
#endif

            TestUtility.AssertMeshesAreEqual(TestUtility.GetAssetTemplate<Mesh>("imported-cube-triangles"), result.mesh);

            UObject.DestroyImmediate(result);

            UObject.DestroyImmediate(instance);
            meshImporter.keepQuads = true;
            meshImporter.SaveAndReimport();
            instance = (GameObject)UObject.Instantiate(source);

            var quadMesh = instance.GetComponent<MeshFilter>().sharedMesh;
            Assert.AreEqual(MeshTopology.Quads, quadMesh.GetTopology(0));

            result = new GameObject().AddComponent<ProBuilderMesh>();
            importer = new MeshImporter(quadMesh, null, result);

            Assert.DoesNotThrow(() =>
            {
                importer.Import(new MeshImportSettings()
                {
                    quads = true,
                    smoothing = false,
                    smoothingAngle = 1f
                });
            });

            result.Rebuild();

#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(result.mesh, "imported-cube-quads");
#endif

            TestUtility.AssertMeshesAreEqual(TestUtility.GetAssetTemplate<Mesh>("imported-cube-quads"), result.mesh);
            UObject.DestroyImmediate(result);
            AssetDatabase.DeleteAsset(TestUtility.temporarySavedAssetsDirectory + "maya-cube-quads.fbx");
        }
    }
}
