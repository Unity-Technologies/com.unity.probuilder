using System.Linq;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor.ProBuilder;

namespace UnityEngine.ProBuilder.EditorTests.Geometry
{
    public class CollapseVerticesTest
    {
        ProBuilderMesh m_PBMesh = null;
        SelectMode m_PreviousSelectMode;

        [SetUp]
        public void Setup()
        {
            m_PBMesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            m_PreviousSelectMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Vertex;
        }

        [TearDown]
        public void Cleanup()
        {
            if(m_PBMesh)
            {
                UObject.DestroyImmediate(m_PBMesh.gameObject);
            }
            ProBuilderEditor.selectMode = m_PreviousSelectMode;
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
            // check that selecting two shared vertices will enable collapse vertices
            Assert.That(m_PBMesh, Is.Not.Null);

            var sharedVertices = m_PBMesh.sharedVerticesInternal;
            Assert.That(sharedVertices, Is.Not.Null);
            Assert.That(sharedVertices.Length, Is.GreaterThanOrEqualTo(2));

            var selectedVertices = sharedVertices[0].Union(sharedVertices[1]);
            Debug.Log("selected: " + string.Join(", ", selectedVertices));
            Assert.That(selectedVertices.Count(), Is.GreaterThan(1));

            // Set the selected vertices to two different shared vertices (collapsable)
            m_PBMesh.SetSelectedVertices(selectedVertices);
            Assert.That(m_PBMesh.selectedIndexesInternal.Length, Is.EqualTo(selectedVertices.Count()));

            MeshSelection.SetSelection(m_PBMesh.gameObject);
            MeshSelection.OnObjectSelectionChanged();

            UnityEditor.ProBuilder.Actions.CollapseVertices collapseVertices = new UnityEditor.ProBuilder.Actions.CollapseVertices();

            Assert.That(collapseVertices.enabled, Is.True);
        }
    }
}
