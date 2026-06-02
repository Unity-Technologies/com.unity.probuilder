using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEditor.ProBuilder;
using NUnit.Framework;
using System.Threading;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

class ExportObj : TemporaryAssetTest
{
    static readonly ObjOptions.Handedness[] k_Handedness = new[] { ObjOptions.Handedness.Right, ObjOptions.Handedness.Left };
    static readonly bool[] k_CopyTextures = new[] { true, false };
    static readonly bool[] k_ApplyTransforms = new[] { true, false };
    static readonly bool[] k_VertexColors = new[] { true, false };
    static readonly bool[] k_TextureOffsetScale = new[] { true, false };

    [Test]
    public static void SerializedValues_AreCultureInvariant()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var model = new Model("Cube", cube.GetComponent<MeshFilter>().sharedMesh, cube.GetComponent<MeshRenderer>().sharedMaterial);
        var current = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

            string obj, mtl;
            List<string> textures;

            if (ObjExporter.Export("Cube Test", new Model[] { model }, out obj, out mtl, out textures))
            {
                Assert.IsFalse(obj.Any(x => x.Equals(',')));
                Assert.IsFalse(mtl.Any(x => x.Equals(',')));
            }
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = current;
            UnityEngine.Object.DestroyImmediate(cube);
        }
    }

    [Test]
    public static void ExportSingleCube_CreatesUnityReadableMeshFile(
        [ValueSource("k_Handedness")] ObjOptions.Handedness handedness,
        [ValueSource("k_CopyTextures")] bool copyTextures,
        [ValueSource("k_ApplyTransforms")] bool applyTransforms,
        [ValueSource("k_VertexColors")] bool vertexColors,
        [ValueSource("k_TextureOffsetScale")] bool textureOffsetScale
    )
    {
        var cube = ShapeFactory.Instantiate<Cube>();

        string obj;
        string mtl;
        List<string> textures;

        var res = ObjExporter.Export("Single cube",
            new Model[] { new Model("Single Cube Mesh", cube, true) },
            out obj,
            out mtl,
            out textures);

        Assume.That(res, Is.True);
        Assume.That(string.IsNullOrEmpty(obj), Is.False);
        Assume.That(string.IsNullOrEmpty(mtl), Is.False);

        string exportedPath = TestUtility.temporarySavedAssetsDirectory + "SingleCube.obj";

        File.WriteAllText(exportedPath, obj);
        AssetDatabase.ImportAsset(exportedPath);

        var imported = AssetDatabase.LoadAssetAtPath(exportedPath, typeof(Mesh)) as Mesh;

        Assume.That(imported, Is.Not.Null);
        Assert.That(imported.vertexCount, Is.GreaterThan(0));

        Object.DestroyImmediate(cube.gameObject);
    }

    [Test]
    public static void ExportSingleCube_CreatesTextureFile()
    {
        var cube = ShapeFactory.Instantiate<Cube>();

        string exportPath = TestUtility.temporarySavedAssetsDirectory + "SingleCube.obj";
        string exportedPath = UnityEditor.ProBuilder.Actions.ExportObj.DoExport(
            exportPath,
            new Model[] { new Model("Single Cube Mesh", cube.mesh, UnityEngine.ProBuilder.BuiltinMaterials.defaultMaterial) },
            new ObjOptions()
            {
                copyTextures = true
            });

        Assume.That(string.IsNullOrEmpty(exportedPath), Is.False);

        AssetDatabase.ImportAsset(exportedPath);

        var imported = AssetDatabase.LoadAssetAtPath(exportedPath, typeof(GameObject)) as GameObject;
        Assume.That(imported, Is.Not.Null);
        var go = GameObject.Instantiate(imported);
        var meshRenderer = go.GetComponentInChildren<MeshRenderer>();
        Assert.That(meshRenderer.sharedMaterials.Length, Is.GreaterThan(0));
        Assert.That(meshRenderer.sharedMaterials[0].mainTexture, Is.Not.Null);
        var mainTex = meshRenderer.sharedMaterials[0].mainTexture;
        Assert.That(AssetDatabase.GetAssetPath(mainTex), Is.EqualTo(TestUtility.temporarySavedAssetsDirectory+mainTex.name+".png"));

        Object.DestroyImmediate(cube.gameObject);
    }

    [Test]
    public static void ExportMultipleMeshes_CreatesModelWithTwoGroups()
    {
        var cubeA = ShapeFactory.Instantiate<Cube>();
        var cubeB = ShapeFactory.Instantiate<Cube>();
        var cube1 = new Model("Cube A", cubeA);
        var cube2 = new Model("Cube B", cubeB);
        string exportedPath = TestUtility.temporarySavedAssetsDirectory + "ObjGroup.obj";

        UnityEditor.ProBuilder.Actions.ExportObj.DoExport(exportedPath, new Model[] { cube1, cube2 }, new ObjOptions()
        {
            copyTextures = false,
            applyTransforms = true,
            vertexColors = false
        });

        AssetDatabase.ImportAsset(exportedPath);

        var imported = AssetDatabase.LoadAssetAtPath(exportedPath, typeof(Mesh)) as Mesh;

        Assume.That(imported, Is.Not.Null);

        var all = AssetDatabase.LoadAllAssetRepresentationsAtPath(exportedPath);

        Assert.That(all.Count(x => x is Mesh), Is.EqualTo(2));

        Object.DestroyImmediate(cubeA.gameObject);
        Object.DestroyImmediate(cubeB.gameObject);
    }
}
