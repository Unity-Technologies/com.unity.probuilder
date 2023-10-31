using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using ToolManager = UnityEditor.EditorTools.ToolManager;

public class CollapseVerticesTest
{
    ProBuilderMesh m_PBMesh;

    [SetUp]
    public void Setup()
    {
        ToolManager.SetActiveContext<PositionToolContext>();
        m_PBMesh = ShapeFactory.Instantiate(typeof(Cube));
        ProBuilderEditor.selectMode = SelectMode.Vertex;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.Vertex));
        Assume.That(typeof(VertexManipulationTool).IsAssignableFrom(ToolManager.activeToolType));
    }

    [TearDown]
    public void Cleanup()
    {
        if (m_PBMesh != null)
            UObject.DestroyImmediate(m_PBMesh.gameObject);

        ToolManager.SetActiveContext<GameObjectToolContext>();
    }

    [Test]
    public void CollapseVertices_SelectSharedVertex_ActionDisabled()
    {
        Assert.That(m_PBMesh, Is.Not.Null);

        var sharedVertices = m_PBMesh.sharedVerticesInternal;
        Assert.That(sharedVertices, Is.Not.Null);
        Assert.That(sharedVertices.Length, Is.GreaterThanOrEqualTo(1));

        var sharedVertex = sharedVertices[0];
        Assert.That(sharedVertex.Count, Is.GreaterThan(1));

        // Set the selected vertices to all vertices belonging to a single shared vertex
        m_PBMesh.SetSelectedVertices(sharedVertex);
        Assert.That(m_PBMesh.selectedIndexesInternal.Length, Is.EqualTo(sharedVertex.Count));

        MeshSelection.SetSelection(m_PBMesh.gameObject);
        MeshSelection.OnObjectSelectionChanged();

        UnityEditor.ProBuilder.Actions.CollapseVertices collapseVertices = new UnityEditor.ProBuilder.Actions.CollapseVertices();

        Assert.That(collapseVertices.enabled, Is.False);
    }

    [Test]
    public void CollapseVertices_SelectedSharedVertices_ActionEnabled()
    {
        Assume.That(m_PBMesh, Is.Not.Null);

        MeshSelection.SetSelection(m_PBMesh.gameObject);
        int[] vertexSelection = new[] { 0, 1, 2, 3 };
        m_PBMesh.SetSelectedVertices(vertexSelection);

        Assume.That(m_PBMesh.selectedIndexesInternal, Is.EquivalentTo(vertexSelection));
        Assume.That(MeshSelection.selectedVertexCount, Is.EqualTo(vertexSelection.Length));

        var collapseAction = new UnityEditor.ProBuilder.Actions.CollapseVertices();
        Assert.That(collapseAction.enabled, Is.True);
    }
}
