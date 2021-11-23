using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;
using NUnit.Framework;
using UObject = UnityEngine.Object;

class VertexManipulationTests
{
    [Test]
    public static void ExtrudeOrthogonally_OneElementManyTimes_NoYOffsetAccumulates()
    {
        // Generate 1 face plane
        var pb = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1f, 1f, 0, 0, Axis.Up);
        try
        {
            Assume.That(pb.facesInternal.Length, Is.EqualTo(1));
            var face = pb.facesInternal[0];

            // Select face
            var selectedFaces = new List<Face>();
            selectedFaces.Add(face);
            ProBuilderEditor.toolManager.SetSelectMode(SelectMode.Face);
            pb.SetSelectedFaces(selectedFaces);
            MeshSelection.SetSelection(pb.gameObject);

            // Look directly at the plane and focus scene view
            var sceneView = SceneView.lastActiveSceneView;
            sceneView.LookAtDirect(pb.transform.position, Quaternion.LookRotation(Vector3.down), 1f);
            sceneView.Focus();

            var mousePos = sceneView.position.size * .5f;
            mousePos.y += sceneView.rootVisualElement.worldBound.y;

            const int k_ExtrudeCount = 100;
            for (int i = 0; i < k_ExtrudeCount; i++)
            {
                // Press down at the center of the face
                UnityEngine.Event e = new UnityEngine.Event()
                {
                    type = EventType.MouseDown,
                    mousePosition = mousePos,
                    modifiers = EventModifiers.Shift,
                    clickCount = 1,
                    delta = Vector2.zero,
                };
                sceneView.SendEvent(e);

                // Do lateral 1px drag and release
                e.type = EventType.MouseDrag;
                e.delta = new Vector2(1f, 0f);
                mousePos += e.delta;
                e.mousePosition = mousePos;
                sceneView.SendEvent(e);
                e.type = EventType.MouseUp;
                sceneView.SendEvent(e);
            }

            // Check that our face count is correct after all extrusions
            Assume.That(pb.facesInternal.Length, Is.EqualTo(k_ExtrudeCount * 4 + 1));

            // We should have the last extruded face in selection
            var postExtrudeSelectedFaces = pb.GetSelectedFaces();
            Assume.That(postExtrudeSelectedFaces.Length, Is.EqualTo(1));
            var lastExtrudedFace = postExtrudeSelectedFaces[0];
            var faceVertices = pb.GetVertices(lastExtrudedFace.indexes);

            // After many orthogonal extrusions, the last face should still be at y=0 coordinate
            for (int i = 0; i < faceVertices.Length; i++)
                Assert.That(faceVertices[i].position.y, Is.EqualTo(0));
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }
}
