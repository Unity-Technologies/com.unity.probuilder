using NUnit.Framework;
using UnityEditor.ProBuilder;
using UnityEngine;

public class DrawShapeToolTests
{
    [Test]
    public void CubeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateCubeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Cube", icon.text);
        Assert.AreEqual("Create Cube", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void SphereToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateSphereTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Sphere", icon.text);
        Assert.AreEqual("Create Sphere", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void PlaneToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreatePlaneTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Plane", icon.text);
        Assert.AreEqual("Create Plane", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void CylinderToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateCylinderTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Cylinder", icon.text);
        Assert.AreEqual("Create Cylinder", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void ConeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateConeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Cone", icon.text);
        Assert.AreEqual("Create Cone", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void PrismToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreatePrismTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Prism", icon.text);
        Assert.AreEqual("Create Prism", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void StairsToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateStairsTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Stairs", icon.text);
        Assert.AreEqual("Create Stairs", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void TorusToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateTorusTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Torus", icon.text);
        Assert.AreEqual("Create Torus", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void PipeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreatePipeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Pipe", icon.text);
        Assert.AreEqual("Create Pipe", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void ArchToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateArchTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Arch", icon.text);
        Assert.AreEqual("Create Arch", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void DoorToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateDoorTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Door", icon.text);
        Assert.AreEqual("Create Door", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void SpriteToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<CreateSpriteTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create Sprite", icon.text);
        Assert.AreEqual("Create Sprite", icon.tooltip);
        Object.DestroyImmediate(tool);
    }
}

public class PolyShapeToolTests
{
    [Test]
    public void DrawPolyShapeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<DrawPolyShapeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Create PolyShape", icon.text);
        Assert.AreEqual("Create PolyShape", icon.tooltip);
        Object.DestroyImmediate(tool);
    }

    [Test]
    public void PolyShapeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<PolyShapeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Edit PolyShape", icon.text);
        Assert.AreEqual("Edit PolyShape", icon.tooltip);
        Object.DestroyImmediate(tool);
    }
}

public class EditShapeToolTests
{
    [Test]
    public void EditShapeToolToolbarIcon_HasCorrectTextAndTooltip()
    {
        var tool = ScriptableObject.CreateInstance<EditShapeTool>();
        var icon = tool.toolbarIcon;
        Assert.AreEqual("Edit Shape", icon.text);
        Assert.AreEqual("Edit ProBuilder Shape", icon.tooltip);
        Object.DestroyImmediate(tool);
    }
}
