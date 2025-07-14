using System.Linq;
using NUnit.Framework;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.VersionControl;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;


public class DisabledMenuTests
{
    ProBuilderMesh m_Cube;
    private bool m_ManifoldAllowed;

    [SetUp]
    public void SetUp()
    {
        m_Cube = ShapeFactory.Instantiate(typeof(Cube));
        MeshSelection.SetSelection(m_Cube.gameObject);
        m_ManifoldAllowed = ProBuilderEditor.s_AllowNonManifoldActions;
        ProBuilderEditor.s_AllowNonManifoldActions.SetValue(false);
    }

    [Test]
    public void ExtrudeEnabledWithNonManifold()
    {
        ProBuilderEditor.selectMode = SelectMode.Edge;
        m_Cube.SetSelectedEdges(m_Cube.faces.First().edges.ToArray());

        var extrude = EditorToolbarLoader.GetInstance<ExtrudeEdges>();
        Assert.NotNull(extrude);
        ProBuilderEditor.s_AllowNonManifoldActions.SetValue(false);
        Assert.False(extrude.enabled);
        ProBuilderEditor.s_AllowNonManifoldActions.SetValue(true);
        Assert.True(extrude.enabled);
    }

    [TearDown]
    public void TearDown()
    {
        if (m_Cube != null)
        {
            UnityEngine.Object.DestroyImmediate(m_Cube.gameObject);
            m_Cube = null;
            ProBuilderEditor.s_AllowNonManifoldActions.SetValue(m_ManifoldAllowed);
        }
    }

}

