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

        private SliderAndInputField m_SegmentSliderAndInputField;
        private SliderAndInputField m_RadiusSliderAndInputField;
        private SliderAndInputField m_FacesSliderAndInputField;

        static StyleSheet s_StyleSheet;

        public override VisualElement CreatePanelContent()
        {

            var root = new VisualElement
            {
                name = "Bezier Mesh Overlay"
            };

            if (s_StyleSheet == null)
                s_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.probuilder/Editor/Stylesheets/BezierMeshOverlayStyle.uss");
            root.styleSheets.Add(s_StyleSheet);

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
                    mesh.ExtrudeMesh();
                }
            });

            m_SegmentSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_SegmentSliderAndInputField.m_IntField.value = m_SegmentSliderAndInputField.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_SegmentsPerUnit = m_SegmentSliderAndInputField.m_SliderInt.value;
                    mesh.ExtrudeMesh();
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
                    mesh.ExtrudeMesh();
                }
            });

            m_RadiusSliderAndInputField.m_Slider.RegisterValueChangedCallback(evt =>
            {
                m_RadiusSliderAndInputField.m_FloatField.value = m_RadiusSliderAndInputField.m_Slider.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_Radius = m_RadiusSliderAndInputField.m_Slider.value;
                    mesh.ExtrudeMesh();
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
                    mesh.ExtrudeMesh();
                }
            });

            m_FacesSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_FacesSliderAndInputField.m_IntField.value = m_FacesSliderAndInputField.m_SliderInt.value;

                foreach (var mesh in meshes)
                {
                    mesh.m_FaceCountPerSegment = m_FacesSliderAndInputField.m_SliderInt.value;
                    mesh.ExtrudeMesh();
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

            if (m_FacesSliderAndInputField == null || m_RadiusSliderAndInputField == null || m_SegmentSliderAndInputField == null)
                return;

            SetParameterValues();
        }

        private void SetParameterValues()
        {
            bool isRadiusEqual = false, isSegmentsEqual = false, isFacesEqual = false;
            var radius = -1f;
            var segment = -1;
            var face = -1;

            // If only one bezier mesh is selected set overlay parameters to its parameters
            if (meshes.Count == 1)
            {
                m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value = meshes[0].m_Radius;
                m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value = meshes[0].m_SegmentsPerUnit;
                m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value = meshes[0].m_FaceCountPerSegment;

                m_RadiusSliderAndInputField.m_FloatField.showMixedValue = false;
                m_SegmentSliderAndInputField.m_IntField.showMixedValue = false;
                m_FacesSliderAndInputField.m_IntField.showMixedValue = false;
            }
            // Show parameters that are equal across all selected bezier meshes, and blank out those that arent
            else
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    if (i == 0)
                    {
                        radius = meshes[i].m_Radius;
                        face = meshes[i].m_FaceCountPerSegment;
                        segment = meshes[i].m_SegmentsPerUnit;
                        continue;
                    }

                    isRadiusEqual = Mathf.Approximately(radius, meshes[i].m_Radius);
                    isSegmentsEqual = Mathf.Approximately(segment, meshes[i].m_SegmentsPerUnit);
                    isFacesEqual = Mathf.Approximately(face, meshes[i].m_FaceCountPerSegment);
                }

                m_RadiusSliderAndInputField.m_FloatField.showMixedValue = !isRadiusEqual;
                m_SegmentSliderAndInputField.m_IntField.showMixedValue = !isSegmentsEqual;
                m_FacesSliderAndInputField.m_IntField.showMixedValue = !isFacesEqual;
            }
        }

        public class SliderAndInputField : VisualElement
        {
            public Slider m_Slider;
            public SliderInt m_SliderInt;
            public FloatField m_FloatField;
            public IntegerField m_IntField;

            const string k_ElementStyle = "slider-and-input-field";
            const string k_SliderStyle = "slider";
            const string k_InputFieldStyle = "input-field";

            public SliderAndInputField(string val, float min, float max, bool useIntField = false)
            {
                AddToClassList(k_ElementStyle);

                if (useIntField)
                {
                    m_SliderInt = new SliderInt(val, (int)min, (int)max);
                    m_SliderInt.AddToClassList(k_SliderStyle);
                    Add(m_SliderInt);

                    m_IntField = new IntegerField();
                    m_IntField.AddToClassList(k_InputFieldStyle);
                    Add(m_IntField);
                }
                else
                {
                    m_Slider = new Slider(val, min, max);
                    m_Slider.AddToClassList(k_SliderStyle);
                    Add(m_Slider);

                    m_FloatField = new FloatField();
                    m_FloatField.AddToClassList(k_InputFieldStyle);
                    Add(m_FloatField);
                }
            }
        }
    }
}
