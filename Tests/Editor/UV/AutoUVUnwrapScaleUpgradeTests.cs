using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.SceneManagement;

class AutoUVUnwrapScaleUpgradeTests : TemporaryAssetTest
{
    const float k_AllowedFloatError = .00001f;

    static Scene s_Scene;
    // ValueSource doesn't work when the array is populated in setup, hence the index workaround
    static ProBuilderMesh[] s_Meshes = new ProBuilderMesh[4];
    static int[] s_MeshIndex = new int[] { 0, 1, 2, 3 };

    [SetUp]
    public void PrepareSceneView()
    {
        s_Scene = OpenScene($"{TestUtility.testsRootDirectory}/Scenes/AutoUVScaleUpgrade.unity");
        var root = s_Scene.GetRootGameObjects();

        s_Meshes[0] = root[0].GetComponent<ProBuilderMesh>();
        Assume.That(s_Meshes[0], Is.Not.Null);
        s_Meshes[1] = root[1].GetComponent<ProBuilderMesh>();
        Assume.That(s_Meshes[1], Is.Not.Null);
        s_Meshes[2] = root[2].GetComponent<ProBuilderMesh>();
        Assume.That(s_Meshes[2], Is.Not.Null);
        s_Meshes[3] = root[3].GetComponent<ProBuilderMesh>();
        Assume.That(s_Meshes[3], Is.Not.Null);
    }

    [TearDown]
    public void TearDown() => CloseScene(s_Scene);

    [Test]
    public void LegacyUVs_UpgradeWithoutModifyingPositions([ValueSource("s_MeshIndex")] int index)
    {
        var mesh = s_Meshes[index];
        var original = mesh.textures.ToArray();
        mesh.Rebuild();
        var textures = mesh.texturesInternal;

        foreach (var face in mesh.facesInternal)
        {
            for (int i = 0; i < face.indexesInternal.Length; i++)
                Assert.That(Mathf.Abs((original[i] - textures[i]).magnitude) < k_AllowedFloatError, $"{face.uv}\nAt index {i} {original[i]} != {mesh.textures[i]}");
        }
    }

    [Test]
    public void LegacyUVs_UpgradeIncrementsMeshVersion()
    {
        var mesh = s_Meshes[0];
        Assume.That(mesh, Is.Not.Null);
        Assume.That(mesh.meshFormatVersion < ProBuilderMesh.k_MeshFormatVersionAutoUVScaleOffset);
        mesh.Rebuild();
        Assert.That(mesh.meshFormatVersion >= ProBuilderMesh.k_MeshFormatVersionAutoUVScaleOffset);
    }
}
