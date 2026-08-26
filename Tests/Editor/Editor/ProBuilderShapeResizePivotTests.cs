using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UObject = UnityEngine.Object;

public class ProBuilderShapeResizePivotTests
{
    ProBuilderMesh m_PBMesh;

    [TearDown]
    public void TearDown()
    {
        if (m_PBMesh != null)
            UObject.DestroyImmediate(m_PBMesh.gameObject);
    }

    // Reproduces: draw a shape with Pivot = First Vertex (pivot sits at a corner, not the center),
    // then edit its size directly in Shape Settings (e.g. 2x2x2 -> 1x1x1). The pivot/gizmo must stay
    // where it was, and the shape must be rebuilt against that same pivot corner at the new size -
    // not left floating at the old size's center. Both cases resize to the same (1,1,1) target, so
    // both must land on the exact same expected center - proving the old size's sign (e.g. a Stairs
    // shape drawn with a negative Z size) can't leak into the new center's placement.
    [TestCase(2f, 2f, 2f, TestName = "AllPositiveInitialSize")]
    [TestCase(2f, 2f, -2f, TestName = "NegativeAxisInInitialSize")]
    public void UpdateShape_AfterResizeWithFirstVertexPivot_KeepsShapeAnchoredAtPivot(float initialX, float initialY, float initialZ)
    {
        m_PBMesh = ShapeFactory.Instantiate<Stairs>();
        var shapeComponent = m_PBMesh.GetComponent<ProBuilderShape>();

        var pivotPosition = Vector3.zero;
        var initialSize = new Vector3(initialX, initialY, initialZ);
        var initialBounds = new Bounds(pivotPosition + initialSize * 0.5f, initialSize);
        shapeComponent.Rebuild(pivotPosition, Quaternion.identity, initialBounds);

        // Simulate editing the size field in the Shape Settings inspector down to 1x1x1.
        shapeComponent.size = Vector3.one;
        shapeComponent.UpdateShape();

        Assert.That(shapeComponent.transform.position, Is.EqualTo(pivotPosition));

        var expectedCenter = pivotPosition + Vector3.one * 0.5f;
        Assert.That(shapeComponent.shapeWorldCenter.x, Is.EqualTo(expectedCenter.x).Within(0.0001f));
        Assert.That(shapeComponent.shapeWorldCenter.y, Is.EqualTo(expectedCenter.y).Within(0.0001f));
        Assert.That(shapeComponent.shapeWorldCenter.z, Is.EqualTo(expectedCenter.z).Within(0.0001f));
    }
}
