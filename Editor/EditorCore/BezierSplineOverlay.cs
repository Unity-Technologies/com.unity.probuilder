using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    [Overlay(typeof(SceneView), overlayID, "Bezier Spline Overlay")]
    public class BezierSplineOverlay : Overlay
    {
        private const string overlayID = "Bezier Spline Overlay";
        private static List<BezierMesh> meshes = new List<BezierMesh>();

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "Bezier Spline Overlay" };

            root.style.width = new StyleLength(new Length(500, LengthUnit.Pixel));

            root.Add(new RadiusSlider());
            root.Add(new SegmentsPerUnitSlider());
            root.Add(new FacesPerSegmentSlider());

            return root;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView view)
        {
            var selected = Selection.gameObjects;
            var hasBezierMesh = false;
            meshes.Clear();

            foreach (var obj in selected)
            {
                if (obj.TryGetComponent(typeof(BezierMesh), out Component mesh))
                {
                    meshes.Add((BezierMesh) mesh);
                    hasBezierMesh = true;
                }
            }

            displayed = hasBezierMesh;
        }

        public class RadiusSlider : Slider
        {
            public RadiusSlider() : base("Radius", 0.01f, 128f)
            {
                this.RegisterValueChangedCallback(EditMesh);
                this.style.flexGrow = 1;

                if (meshes.Count == 1)
                    value = meshes[0].m_Radius;
            }

            private void EditMesh(ChangeEvent<float> evnt)
            {
                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = value;
                    mesh.Extrude3DMesh();
                }
            }
        }

        public class SegmentsPerUnitSlider : SliderInt
        {
            public SegmentsPerUnitSlider() : base("Segments Per Unit", 1, 64)
            {
                this.RegisterValueChangedCallback(EditMesh);
                this.style.flexGrow = 1;

                if (meshes.Count == 1)
                    value = meshes[0].m_SegmentsPerUnit;
            }

            private void EditMesh(ChangeEvent<int> evnt)
            {
                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = value;
                    mesh.Extrude3DMesh();
                }
            }
        }

        public class FacesPerSegmentSlider : SliderInt
        {
            public FacesPerSegmentSlider() : base("Faces Per Segment", 3, 256)
            {
                this.RegisterValueChangedCallback(EditMesh);
                this.style.flexGrow = 1;

                if (meshes.Count == 1)
                    value = meshes[0].m_FaceCountPerSegment;
            }

            private void EditMesh(ChangeEvent<int> evnt)
            {
                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = value;
                    mesh.Extrude3DMesh();
                }
            }
        }
    }
}
