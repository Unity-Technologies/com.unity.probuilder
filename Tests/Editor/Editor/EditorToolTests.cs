using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UObject = UnityEngine.Object;
using ToolManager = UnityEditor.EditorTools.ToolManager;

public class EditorToolTests
{
    ProBuilderMesh m_PBMesh;

    [OneTimeSetUp]
    public void SetUp()
    {
        m_PBMesh = ShapeFactory.Instantiate(typeof(Cube));
        MeshSelection.SetSelection(m_PBMesh.gameObject);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ToolManager.SetActiveContext<PositionToolContext>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        ToolManager.SetActiveContext<GameObjectToolContext>();

        if (m_PBMesh != null)
            UObject.DestroyImmediate(m_PBMesh.gameObject);
    }

    [Test]
    public void SetToolKeepsContext()
    {
        ProBuilderEditor.selectMode = SelectMode.Face;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.Face));
        Assume.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
        Tools.current = Tool.Rotate;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderRotateTool)));
    }

    [Test]
    public void SetSelectModeSetsTool()
    {
        Assume.That(ProBuilderEditor.instance, Is.Not.Null);
        ProBuilderEditor.selectMode = SelectMode.None;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.None));
        ToolManager.SetActiveContext<PositionToolContext>();
        Tools.current = Tool.Move;

        ProBuilderEditor.selectMode = SelectMode.Face;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
    }
}
