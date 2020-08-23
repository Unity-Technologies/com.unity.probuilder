using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

public class AutoUVUnwrapScaleUpgradeTests
{
    const float k_AllowedFloatError = .00001f;

    static Scene s_Scene;
    static ProBuilderMesh[] s_Meshes = new ProBuilderMesh[4];
    // ValueSource doesn't work when the array is populated in setup
    static int[] s_MeshIndex = new int[] { 0, 1, 2, 3 };

    static IEnumerable<GameObject> rootGameObjects
    {
        get { return s_Scene.GetRootGameObjects(); }
    }

    [OneTimeSetUp]
    public void PrepareSceneView()
    {
        s_Scene = EditorSceneManager.OpenScene("Packages/com.unity.probuilder/Tests/Scenes/AutoUVScaleUpgrade.unity");
        var root = s_Scene.GetRootGameObjects();

        s_Meshes[0] = root[0].GetComponent<ProBuilderMesh>();
        s_Meshes[1] = root[1].GetComponent<ProBuilderMesh>();
        s_Meshes[2] = root[2].GetComponent<ProBuilderMesh>();
        s_Meshes[3] = root[3].GetComponent<ProBuilderMesh>();
    }

    [Test]
    public void LegacyUVs_UpgradeWithoutModifyingPositions([ValueSource("s_MeshIndex")] int index)
    {
        var mesh = s_Meshes[index];
        var original = mesh.textures.ToArray();
        Assert.That(mesh.meshFormatVersion < ProBuilderMesh.k_MeshFormatVersionAutoUVScaleOffset, mesh.gameObject.name);
        mesh.Rebuild();

        for (int i = 0; i < original.Length; i++)
            Assert.That(Mathf.Abs((original[i] - mesh.textures[i]).magnitude) < k_AllowedFloatError);
        // Wasn't able to get Vec2 comparison working with Within constraint, not sure why
        // Assert.That(mesh.textures, Is.EqualTo(original).Within(k_AllowedFloatError), $"{mesh.gameObject.name}");
    }
}
