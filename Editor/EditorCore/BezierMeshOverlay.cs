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

        private SliderAndInputField m_SegmentSliderAndInputField;
        private SliderAndInputField m_RadiusSliderAndInputField;
        private SliderAndInputField m_FacesSliderAndInputField;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                name = "Bezier Mesh Overlay",
                style =
                {
                    // LengthUnit.Pixel or LengthUnit.Percent ?
                    width = new StyleLength(new Length(k_OverlayWidth, LengthUnit.Pixel)),
                }
            };

            CreateSegmentsElement();
            CreateRadiusElement();
            CreateFacesElement();

            root.Add(m_SegmentSliderAndInputField);
            root.Add(m_RadiusSliderAndInputField);
            root.Add(m_FacesSliderAndInputField);

            return root;
        }

        private void CreateSegmentsElement()
        {
            m_SegmentSliderAndInputField =
                new SliderAndInputField("Segments per Unit", k_SegmentsMin, k_SegmentsMax, true)
                {
                    tooltip = "Number of length-wise segments of the mesh per unit length"
                };
            m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value = k_SegmentsMin;

            m_SegmentSliderAndInputField.m_IntField.RegisterValueChangedCallback(evt =>
            {
                m_SegmentSliderAndInputField.m_IntField.value = Mathf.Clamp(m_SegmentSliderAndInputField.m_IntField.value, k_SegmentsMin, k_SegmentsMax);
                m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = m_SegmentSliderAndInputField.m_IntField.value;
                    mesh.Extrude3DMesh();
                }
            });

            m_SegmentSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_SegmentSliderAndInputField.m_IntField.value = m_SegmentSliderAndInputField.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = m_SegmentSliderAndInputField.m_SliderInt.value;
                    mesh.Extrude3DMesh();
                }
            });
        }

        private void CreateRadiusElement()
        {
            m_RadiusSliderAndInputField = new SliderAndInputField("Radius", k_RadiusMin, k_RadiusMax)
            {
                tooltip = "The distance of the mesh from the center of the spline"
            };
            m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value = k_RadiusMin;

            m_RadiusSliderAndInputField.m_FloatField.RegisterValueChangedCallback(evt =>
            {
                m_RadiusSliderAndInputField.m_FloatField.value = Mathf.Clamp(m_RadiusSliderAndInputField.m_FloatField.value, k_RadiusMin, k_RadiusMax);
                m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = m_RadiusSliderAndInputField.m_FloatField.value;
                    mesh.Extrude3DMesh();
                }
            });

            m_RadiusSliderAndInputField.m_Slider.RegisterValueChangedCallback(evt =>
            {
                m_RadiusSliderAndInputField.m_FloatField.value = m_RadiusSliderAndInputField.m_Slider.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = m_RadiusSliderAndInputField.m_Slider.value;
                    mesh.Extrude3DMesh();
                }
            });
        }

        private void CreateFacesElement()
        {
            m_FacesSliderAndInputField = new SliderAndInputField("Faces per Segment", k_FacesMin, k_FacesMax, true)
            {
                tooltip = "The number of faces around the bezier mesh at each segment"
            };
            m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value = k_FacesMin;

            m_FacesSliderAndInputField.m_IntField.RegisterValueChangedCallback(evt =>
            {
                m_FacesSliderAndInputField.m_IntField.value = Mathf.Clamp(m_FacesSliderAndInputField.m_IntField.value, k_FacesMin, k_FacesMax);
                m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = m_FacesSliderAndInputField.m_IntField.value;
                    mesh.Extrude3DMesh();
                }
            });

            m_FacesSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_FacesSliderAndInputField.m_IntField.value = m_FacesSliderAndInputField.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = m_FacesSliderAndInputField.m_SliderInt.value;
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
                m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value = meshes[0].m_Radius;
                m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value = meshes[0].m_SegmentsPerUnit;
                m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value = meshes[0].m_FaceCountPerSegment;
            }
        }

        public class SliderAndInputField : VisualElement
        {
            public Slider m_Slider;
            public SliderInt m_SliderInt;
            public FloatField m_FloatField;
            public IntegerField m_IntField;

            public SliderAndInputField(string val, float min, float max, bool useIntField = false)
            {
                if (useIntField)
                {
                    m_SliderInt = new SliderInt(val, (int)min, (int)max);
                    m_SliderInt.style.width = new StyleLength(k_OverlayWidth * .85f);
                    Add(m_SliderInt);

                    m_IntField = new IntegerField();
                    Add(m_IntField);
                }
                else
                {
                    m_Slider = new Slider(val, min, max);
                    m_Slider.style.width = new StyleLength(k_OverlayWidth * .85f);
                    Add(m_Slider);

                    m_FloatField = new FloatField();
                    m_FloatField.style.maxWidth = new StyleLength(k_OverlayWidth * .13f);
                    Add(m_FloatField);
                }

                style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            }
        }
    }
}
