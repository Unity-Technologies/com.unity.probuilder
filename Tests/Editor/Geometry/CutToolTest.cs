using NUnit.Framework;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using Plane = UnityEngine.Plane;
using UObject = UnityEngine.Object;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

public class CutToolTest
{
    ProBuilderMesh m_PBMesh;
    bool m_OpenedWindow = false;
    SelectMode m_PreviousSelectMode;

    [SetUp]
    public void Setup()
    {
        // make sure the ProBuilder window is open
        if (ProBuilderEditor.instance == null)
            ProBuilderEditor.MenuOpenWindow();

        Assume.That(ProBuilderEditor.instance, Is.Not.Null);

        m_PBMesh = ShapeFactory.Instantiate(typeof(UnityEngine.ProBuilder.Shapes.Plane));
        MeshSelection.SetSelection(m_PBMesh.gameObject);
        MeshSelection.OnObjectSelectionChanged();

        m_PreviousSelectMode = ProBuilderEditor.selectMode;
        ProBuilderEditor.selectMode = SelectMode.Object;
    }

    [TearDown]
    public void Cleanup()
    {
        if (m_PBMesh != null)
            UObject.DestroyImmediate(m_PBMesh.gameObject);

        ProBuilderEditor.selectMode = m_PreviousSelectMode;

        // close editor window if we had to open it
        if (m_OpenedWindow && ProBuilderEditor.instance != null)
        {
            ProBuilderEditor.instance.Close();
        }
    }

    [Test]
    public void CutTool_EdgeToEdgeCut_TestInsertOnEdge_TestCreatesTwoFaces()
    {
        CutTool tool = ScriptableObject.CreateInstance<CutTool>();
        ToolManager.SetActiveTool(tool);

        int originalFaceCount = m_PBMesh.faces.Count;
        Face face = m_PBMesh.faces[0];
        Assert.That(face, Is.Not.Null);

        Vertex[] vertices = m_PBMesh.GetVertices();
        var faceIndexes = face.distinctIndexes;
        Assert.That(faceIndexes.Count, Is.EqualTo(4));

        Vector3 pos_a = Math.Average(new Vector3[]{vertices[faceIndexes[0]].position, vertices[faceIndexes[1]].position});
        Vector3 pos_b = Math.Average(new Vector3[]{vertices[faceIndexes[2]].position, vertices[faceIndexes[3]].position});

        tool.UpdateCurrentPosition(face, pos_a, Vector3.up);
        Assert.That(tool.m_TargetFace, Is.Null);
        Assert.That(tool.m_CurrentFace, Is.EqualTo(face));
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.AddedOnEdge));

        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(1));
        Assert.That(tool.m_TargetFace, Is.EqualTo(face));
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(0));

        tool.UpdateCurrentPosition(face, pos_b, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex));

        tool.m_SnappingPoint = true;
        tool.UpdateCurrentPosition(face, pos_b, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.AddedOnEdge));

        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(2));

        ActionResult result = tool.DoCut();
        Assert.That(result.status, Is.EqualTo(ActionResult.Success.status));
        Assert.That(m_PBMesh.faces.Count, Is.EqualTo(originalFaceCount -1 /*removed face*/ +2 /*added faces*/));

        Object.DestroyImmediate(tool);
    }

    [Test]
    public void CutTool_CutUsingExistingVertexAndNewOne_TestInsertionTypes_TestCreatesTwoFaces()
    {
        CutTool tool = ScriptableObject.CreateInstance<CutTool>();
        ToolManager.SetActiveTool(tool);

        int originalFaceCount = m_PBMesh.faces.Count;
        Face face = m_PBMesh.faces[0];
        Assume.That(face, Is.Not.Null);

        Vertex[] vertices = m_PBMesh.GetVertices();
        var faceIndexes = face.distinctIndexes;
        Assume.That(faceIndexes.Count, Is.EqualTo(4));

        Vector3 pos_a = vertices[faceIndexes[0]].position;
        Vector3 pos_b = Math.Average(new Vector3[]{vertices[faceIndexes[1]].position, vertices[faceIndexes[2]].position, vertices[faceIndexes[3]].position});

        tool.UpdateCurrentPosition(face, pos_a, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.ExistingVertex));

        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(1));
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(0));

        tool.UpdateCurrentPosition(face, pos_b, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex));

        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(2));
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(1));

        ActionResult result = tool.DoCut();
        Assert.That(result.status, Is.EqualTo(ActionResult.Success.status));
        Assert.That(m_PBMesh.faces.Count, Is.EqualTo(originalFaceCount -1 /*removed face*/ +2 /*added faces*/));

        Object.DestroyImmediate(tool);
    }

    [Test]
    public void CutTool_CutUsing3NewVertices_TestInsertionTypes_TestCreates3Faces()
    {
        CutTool tool = ScriptableObject.CreateInstance<CutTool>();
        ToolManager.SetActiveTool(tool);

        int originalFaceCount = m_PBMesh.faces.Count;
        Face face = m_PBMesh.faces[0];
        Assume.That(face, Is.Not.Null);

        Vertex[] vertices = m_PBMesh.GetVertices();
        var faceIndexes = face.distinctIndexes;
        Assume.That(faceIndexes.Count, Is.EqualTo(4));

        Vector3 pos_a = Math.Average(new Vector3[]{vertices[faceIndexes[0]].position, vertices[faceIndexes[1]].position, vertices[faceIndexes[2]].position});
        Vector3 pos_b = Math.Average(new Vector3[]{vertices[faceIndexes[1]].position, vertices[faceIndexes[2]].position, vertices[faceIndexes[3]].position});
        Vector3 pos_c = Math.Average(new Vector3[]{vertices[faceIndexes[0]].position, vertices[faceIndexes[1]].position, vertices[faceIndexes[3]].position});

        //Creating a first new vertex
        tool.UpdateCurrentPosition(face, pos_a, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex));

        //Insert first vertex to the path
        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(1));
        //No connection is created yet
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(0));

        //Creating a second new vertex
        tool.UpdateCurrentPosition(face, pos_b, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex));

        //Insert 2nd point to the path
        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(2));
        //Check that the created path is connected twice to the containing face
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(2));

        //Creating a third new vertex
        tool.UpdateCurrentPosition(face, pos_c, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex));

        //Insert 3rd point to the path
        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(3));
        //Check that the created path is connected twice to the containing face
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(2));

        //Creating a 4th new vertex already contained in the shape
        tool.UpdateCurrentPosition(face, pos_a, Vector3.up);
        Assert.That(tool.m_CurrentVertexTypes, Is.EqualTo(CutTool.VertexTypes.NewVertex|CutTool.VertexTypes.VertexInShape));

        //Insert 4th point to the path
        tool.AddCurrentPositionToPath();
        Assert.That(tool.m_CutPath.Count, Is.EqualTo(4));
        Assert.That(tool.m_MeshConnections.Count, Is.EqualTo(2));

        ActionResult result = tool.DoCut();
        Assert.That(result.status, Is.EqualTo(ActionResult.Success.status));
        Assert.That(m_PBMesh.faces.Count, Is.EqualTo(originalFaceCount -1 /*removed face*/ +3 /*added faces*/));

        Object.DestroyImmediate(tool);
    }

}
