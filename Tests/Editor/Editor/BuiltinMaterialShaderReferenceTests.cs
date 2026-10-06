using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

class BuiltinMaterialShaderReferenceTests
{
    const string k_ShaderGraphAssetPath = "Packages/com.unity.probuilder/Content/Shader/Standard Vertex Color.shadergraph";

    static readonly Regex k_ShaderReference = new Regex(@"m_Shader: \{fileID: (-?\d+), guid: ([0-9a-f]{32})");

    static readonly string[] k_MaterialAssetPaths =
    {
        "Packages/com.unity.probuilder/Content/Resources/Materials/ProBuilderDefault.mat",
        "Packages/com.unity.probuilder/Content/Material/Checker.mat"
    };

    const long k_LegacyShaderLocalId = 4800000;

    static readonly object[] k_MigratedSurfaceShaders =
    {
        new object[] { "ProBuilder/Diffuse Vertex Color", "911130a939bf84843bcc4211c327f579" },
        new object[] { "ProBuilder/Diffuse Texture Blend", "33cb4e8ff0c1d43ac8ec40692656c7ed" }
    };

    static string ResolveDiskPath(string assetPath)
    {
        var package = PackageInfo.FindForAssetPath(assetPath);

        if (package == null)
            return assetPath;

        return package.resolvedPath + assetPath.Substring(package.assetPath.Length);
    }

    // UUM-150702
    [Test]
    [TestCaseSource(nameof(k_MaterialAssetPaths))]
    public void SerializedShaderReferenceResolvesToShaderGraphShader(string materialAssetPath)
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(k_ShaderGraphAssetPath);
        Assert.That(shader, Is.Not.Null, k_ShaderGraphAssetPath);

        Assert.That(
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out var shaderGuid, out long shaderLocalId),
            Is.True,
            k_ShaderGraphAssetPath);

        var diskPath = ResolveDiskPath(materialAssetPath);
        Assert.That(File.Exists(diskPath), Is.True, diskPath);

        // Matches the material's serialized shader reference, for example:
        // m_Shader: {fileID: -6465566751694194690, guid: 8547ec39534635c4289065e5c5033f43, type: 3}
        var match = k_ShaderReference.Match(File.ReadAllText(diskPath));
        Assert.That(match.Success, Is.True, materialAssetPath);

        // Group 2 is guid, identifying the asset file itself.
        Assert.That(match.Groups[2].Value, Is.EqualTo(shaderGuid), materialAssetPath);

        // Group 1 is fileID, the shader's local identifier within the referenced asset.
        Assert.That(long.Parse(match.Groups[1].Value), Is.EqualTo(shaderLocalId), materialAssetPath);
    }

    [Test]
    [TestCaseSource(nameof(k_MaterialAssetPaths))]
    public void LoadedMaterialHasSupportedShader(string materialAssetPath)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        Assert.That(material, Is.Not.Null, materialAssetPath);
        Assert.That(material.shader, Is.Not.Null, materialAssetPath);
        Assert.That(material.shader.name, Does.Not.StartWith("Hidden/InternalErrorShader"), materialAssetPath);
    }

    [Test]
    [TestCaseSource(nameof(k_MigratedSurfaceShaders))]
    public void MigratedSurfaceShaderKeepsLegacyNameAndReference(string shaderName, string legacyGuid)
    {
        var shader = Shader.Find(shaderName);
        Assert.That(shader, Is.Not.Null, shaderName);

        Assert.That(
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out var shaderGuid, out long shaderLocalId),
            Is.True,
            shaderName);

        Assert.That(shaderGuid, Is.EqualTo(legacyGuid), shaderName);
        Assert.That(shaderLocalId, Is.EqualTo(k_LegacyShaderLocalId), shaderName);
    }
}
