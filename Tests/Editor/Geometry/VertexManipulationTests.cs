using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEditor.EditorTools;
using UObject = UnityEngine.Object;

class VertexManipulationTests
{
    [Test]
    public static void ExtrudeOrthogonally_OneElementManyTimes_NoYOffsetAccumulates()
    {
        // Generate single face plane
        var pb = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1f, 1f, 0, 0, Axis.Up);
        try
        {
            pb.transform.position = Vector3.zero;
            pb.transform.rotation = Quaternion.identity;

            ProBuilderEditor.MenuOpenWindow();
            EditorApplication.ExecuteMenuItem("Window/General/Scene");

            var sceneView = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.SceneView>()[0];
            sceneView.orthographic = true;
            sceneView.drawGizmos = false;
            sceneView.pivot = new Vector3(0, 0, 0);
            sceneView.rotation = Quaternion.AngleAxis(90f, Vector3.right);
            sceneView.size = 2.0f;
            sceneView.Focus();

            var e = new Event();
            e.type = EventType.MouseEnterWindow;
            sceneView.SendEvent(e);

            Assume.That(pb.facesInternal.Length, Is.EqualTo(1));
            var face = pb.facesInternal[0];

            // Select face
            var selectedFaces = new List<Face>();
            selectedFaces.Add(face);
            Tools.current = Tool.Move;
            ProBuilderEditor.toolManager.SetSelectMode(SelectMode.Face);
            pb.SetSelectedFaces(selectedFaces);
            MeshSelection.SetSelection(pb.gameObject);

            // Center mouse position
            var bounds = SceneView.focusedWindow.rootVisualElement.worldBound;
            var mousePos = (bounds.size * 0.5f + bounds.position) + new Vector2Int(1, 1);

            const int k_ExtrudeCount = 100;
            for (int i = 0; i < k_ExtrudeCount; i++)
            {
                // Press down at the center of the face
                e = new UnityEngine.Event()
                {
                    type = EventType.MouseDown,
                    mousePosition = mousePos,
                    modifiers = EventModifiers.None,
                    clickCount = 1,
                    delta = Vector2.zero,
                };
                sceneView.SendEvent(e);

                // Do lateral 1px drag and release
                var mouseDelta =  new Vector2(i % 2 == 0 ? 1f : -1f, 0f);
                mousePos += mouseDelta;

                e.type = EventType.MouseDrag;
                e.mousePosition = mousePos;
                e.modifiers = EventModifiers.Shift;
                e.clickCount = 0;
                e.delta = mouseDelta;
                sceneView.SendEvent(e);

                e.type = EventType.MouseUp;
                e.mousePosition = mousePos;
                e.delta = Vector2.zero;
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
                Assert.That(faceVertices[i].position.y, Is.EqualTo(0f));
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }
}
