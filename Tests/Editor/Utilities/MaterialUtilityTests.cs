using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MaterialUtilityTests
{
    static IEnumerable<List<int>> s_MaterialIndexToRemove
    {
        get
        {
            yield return new List<int> { 0 };
            yield return new List<int> { 1 };
            yield return new List<int> { 1, 2 };
            yield return new List<int> { 0, 2 };
        }
    }

    static IEnumerable<string> s_PrefabResourcePaths
    {
        get
        {
            yield return "[Submesh-1  Material-1]";
            yield return "[Submesh-1  Material-2]";
            yield return "[Submesh-2  Material-2]";
            yield return "[Submesh-2  Material-4]";
            yield return "[Submesh-4  Material-4]";
        }
    }

    [Test]
    public void RemoveMaterialsAndTrimExcess_MaterialCountIsEqualToSubmeshCount(
        [ValueSource(nameof(s_PrefabResourcePaths))] string prefabResourcePath)
    {
        var prefab = Resources.Load<GameObject>($"TestCube " + prefabResourcePath);
        Assume.That(prefab, Is.Not.Null);

        var renderer = Object.Instantiate(prefab).GetComponent<Renderer>();
        Assume.That(renderer, Is.Not.Null);

        var probuilderFilter = renderer.GetComponent<ProBuilderMesh>();
        Assume.That(probuilderFilter, Is.Not.Null);
        
        MaterialUtility.RemoveMaterialsAndTrimExcess(renderer, new List<int>(), Submesh.GetSubmeshCount(probuilderFilter));

        Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(probuilderFilter.mesh.subMeshCount));
    }

    [Test]
    public void RemoveMaterialsAndTrimExcess_RemoveMaterial_MaterialIsRemoved(
        [ValueSource(nameof(s_MaterialIndexToRemove))] List<int> indicesToRemove,
        [ValueSource(nameof(s_PrefabResourcePaths))] string prefabResourcePath)
    {
        var prefab = Resources.Load<GameObject>($"TestCube " + prefabResourcePath);
        Assume.That(prefab, Is.Not.Null);

        var renderer = Object.Instantiate(prefab).GetComponent<Renderer>();
        Assume.That(renderer, Is.Not.Null);

        var probuilderFilter = renderer.GetComponent<ProBuilderMesh>();
        Assume.That(probuilderFilter, Is.Not.Null);

        List<Material> expectedResult = new List<Material>(renderer.sharedMaterials);
        for (int i = expectedResult.Count - 1; i >= 0; --i)
            if (indicesToRemove.Contains(i))
                expectedResult.RemoveAt(i);

        var submeshCount = Submesh.GetSubmeshCount(probuilderFilter);
        MaterialUtility.RemoveMaterialsAndTrimExcess(renderer, indicesToRemove, submeshCount);

        var expectedMaterialCount = Mathf.Min(submeshCount, expectedResult.Count);
        Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(expectedMaterialCount));
        for (int i = 0; i < expectedMaterialCount; ++i) 
            Assert.That(renderer.sharedMaterials[i], Is.EqualTo(expectedResult[i]));
    }
}
