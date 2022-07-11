using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    [Overlay(typeof(SceneView), overlayID, "Bezier Mesh Overlay")]
    public class BezierMeshOverlay : Overlay
    {
        private const string overlayID = "Bezier Mesh Overlay";
        private static List<BezierMesh> meshes = new List<BezierMesh>();

        private const float k_RadiusMin = BezierMesh.k_RadiusMin;
        private const float k_RadiusMax = BezierMesh.k_RadiusMax;
        private const int k_FacesMin = BezierMesh.k_FacesMin;
        private const int k_FacesMax = BezierMesh.k_FacesMax;
        private const int k_SegmentsMin = BezierMesh.k_SegmentsMin;
        private const int k_SegmentsMax = BezierMesh.k_SegmentsMax;

        private const int k_OverlayWidth = 350;

        private SliderAndFloatField segments;
        private SliderAndFloatField radius;
        private SliderAndFloatField faces;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                name = "Bezier Mesh Overlay",
                style =
                {
                    width = new StyleLength(new Length(k_OverlayWidth, LengthUnit.Pixel)),
                }
            };

            CreateSegmentsElement();
            CreateRadiusElement();
            CreateFacesElement();

            root.Add(segments);
            root.Add(radius);
            root.Add(faces);

            return root;
        }

        private void CreateSegmentsElement()
        {
            segments = new SliderAndFloatField("Segments per Unit", k_SegmentsMin, k_SegmentsMax, true);
            segments.tooltip = "Number of length-wise segments of the mesh per unit length";
            segments.m_SliderInt.value = segments.m_IntField.value = k_SegmentsMin;

            segments.m_IntField.RegisterValueChangedCallback(evt =>
            {
                segments.m_IntField.value = Mathf.Clamp(segments.m_IntField.value, k_SegmentsMin, k_SegmentsMax);
                segments.m_SliderInt.value = segments.m_IntField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = segments.m_IntField.value;
                    mesh.Extrude3DMesh();
                }
            });

            segments.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                segments.m_IntField.value = segments.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = segments.m_SliderInt.value;
                    mesh.Extrude3DMesh();
                }
            });
        }

        private void CreateRadiusElement()
        {
            radius = new SliderAndFloatField("Radius", k_RadiusMin, k_RadiusMax);
            radius.tooltip = "The distance of the mesh from the center of the spline";
            radius.m_Slider.value = radius.m_FloatField.value = k_RadiusMin;

            radius.m_FloatField.RegisterValueChangedCallback(evt =>
            {
                radius.m_FloatField.value = Mathf.Clamp(radius.m_FloatField.value, k_RadiusMin, k_RadiusMax);
                radius.m_Slider.value = radius.m_FloatField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = radius.m_FloatField.value;
                    mesh.Extrude3DMesh();
                }
            });

            radius.m_Slider.RegisterValueChangedCallback(evt =>
            {
                radius.m_FloatField.value = radius.m_Slider.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = radius.m_Slider.value;
                    mesh.Extrude3DMesh();
                }
            });
        }

        private void CreateFacesElement()
        {
            faces = new SliderAndFloatField("Faces per Segment", k_FacesMin, k_FacesMax, true);
            faces.tooltip = "The number of faces around the bezier mesh at each segment";
            faces.m_SliderInt.value = faces.m_IntField.value = k_FacesMin;

            faces.m_IntField.RegisterValueChangedCallback(evt =>
            {
                faces.m_IntField.value = Mathf.Clamp(faces.m_IntField.value, k_FacesMin, k_FacesMax);
                faces.m_SliderInt.value = faces.m_IntField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = faces.m_IntField.value;
                    mesh.Extrude3DMesh();
                }
            });

            faces.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                faces.m_IntField.value = faces.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = faces.m_SliderInt.value;
                    mesh.Extrude3DMesh();
                }
            });
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
                    meshes.Add((BezierMesh)mesh);
                    hasBezierMesh = true;
                }
            }
            displayed = hasBezierMesh;

            // If only one bezier mesh is selected set overlay parameters to its parameters
            if (meshes.Count == 1)
            {
                radius.m_Slider.value = radius.m_FloatField.value = meshes[0].m_Radius;
                segments.m_SliderInt.value = segments.m_IntField.value = meshes[0].m_SegmentsPerUnit;
                faces.m_SliderInt.value = faces.m_IntField.value = meshes[0].m_FaceCountPerSegment;
            }
        }

        public class SliderAndFloatField : VisualElement
        {
            public Slider m_Slider;
            public SliderInt m_SliderInt;
            public FloatField m_FloatField;
            public IntegerField m_IntField;

            public SliderAndFloatField(string val, float min, float max, bool useIntField = false)
            {
                if (useIntField)
                {
                    m_SliderInt = new SliderInt(val, (int) min, (int) max);
                    m_SliderInt.style.width = new StyleLength(k_OverlayWidth * .87f);
                    Add(m_SliderInt);

                    m_IntField = new IntegerField();
                    Add(m_IntField);
                }
                else
                {
                    m_Slider = new Slider(val, min, max);
                    m_Slider.style.width = new StyleLength(k_OverlayWidth * .87f);
                    Add(m_Slider);

                    m_FloatField = new FloatField();
                    Add(m_FloatField);
                }

                style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            }
        }
    }
}
