using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    [Overlay(typeof(SceneView), overlayID, "Bezier Mesh Overlay")]
    public class BezierMeshTransientOverlay : Overlay, ITransientOverlay
    {
        private const string overlayID = "Bezier Mesh Overlay";
        private static List<BezierMesh> meshes = new List<BezierMesh>();

        private SliderAndInputField m_SegmentSliderAndInputField;
        private SliderAndInputField m_RadiusSliderAndInputField;
        private SliderAndInputField m_FacesSliderAndInputField;

        static StyleSheet s_StyleSheet;

        public bool m_Visisble;
        public bool visible => m_Visisble;

        public BezierMeshTransientOverlay()
        {
            CreateSegmentsElement();
            CreateRadiusElement();
            CreateFacesElement();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                name = "Bezier Mesh Overlay"
            };

            if (s_StyleSheet == null)
                s_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.probuilder/Editor/Stylesheets/BezierMeshOverlayStyle.uss");
            root.styleSheets.Add(s_StyleSheet);

            root.Add(m_SegmentSliderAndInputField);
            root.Add(m_RadiusSliderAndInputField);
            root.Add(m_FacesSliderAndInputField);

            return root;
        }

        private void CreateSegmentsElement()
        {
            m_SegmentSliderAndInputField =
                new SliderAndInputField("Segments per Unit", BezierMesh.k_SegmentsMin, BezierMesh.k_SegmentsMax, true)
                {
                    tooltip = "Number of length-wise segments of the mesh per unit length"
                };

            m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value = BezierMesh.k_SegmentsMin;

            m_SegmentSliderAndInputField.m_IntField.RegisterValueChangedCallback(evt =>
            {
                m_SegmentSliderAndInputField.m_IntField.value = Mathf.Clamp(m_SegmentSliderAndInputField.m_IntField.value, BezierMesh.k_SegmentsMin, BezierMesh.k_SegmentsMax);
                m_SegmentSliderAndInputField.m_SliderInt.value = m_SegmentSliderAndInputField.m_IntField.value;

                UpdateMesh();
            });

            m_SegmentSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_SegmentSliderAndInputField.m_IntField.value = m_SegmentSliderAndInputField.m_SliderInt.value;

                UpdateMesh();
            });
        }

        private void CreateRadiusElement()
        {
            m_RadiusSliderAndInputField = new SliderAndInputField("Radius", BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax)
            {
                tooltip = "The distance of the mesh from the center of the spline"
            };
            m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value = BezierMesh.k_RadiusMin;

            m_RadiusSliderAndInputField.m_FloatField.RegisterValueChangedCallback(evt =>
            {
                m_RadiusSliderAndInputField.m_FloatField.value = Mathf.Clamp(m_RadiusSliderAndInputField.m_FloatField.value, BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax);
                m_RadiusSliderAndInputField.m_Slider.value = m_RadiusSliderAndInputField.m_FloatField.value;

                UpdateMesh();
            });

            m_RadiusSliderAndInputField.m_Slider.RegisterValueChangedCallback(evt =>
            {
                m_RadiusSliderAndInputField.m_FloatField.value = m_RadiusSliderAndInputField.m_Slider.value;

                UpdateMesh();
            });
        }

        private void CreateFacesElement()
        {
            m_FacesSliderAndInputField = new SliderAndInputField("Faces per Segment", BezierMesh.k_FacesMin, BezierMesh.k_FacesMax, true)
            {
                tooltip = "The number of faces around the bezier mesh at each segment"
            };
            m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value = BezierMesh.k_FacesMin;

            m_FacesSliderAndInputField.m_IntField.RegisterValueChangedCallback(evt =>
            {
                m_FacesSliderAndInputField.m_IntField.value = Mathf.Clamp(m_FacesSliderAndInputField.m_IntField.value, BezierMesh.k_FacesMin, BezierMesh.k_FacesMax);
                m_FacesSliderAndInputField.m_SliderInt.value = m_FacesSliderAndInputField.m_IntField.value;

                UpdateMesh();
            });

            m_FacesSliderAndInputField.m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_FacesSliderAndInputField.m_IntField.value = m_FacesSliderAndInputField.m_SliderInt.value;

                UpdateMesh();
            });
        }

        void UpdateMesh()
        {
            foreach (var mesh in meshes)
            {
                mesh.m_SegmentsPerUnit = m_SegmentSliderAndInputField.m_IntField.value;
                mesh.m_Radius = m_RadiusSliderAndInputField.m_FloatField.value;
                mesh.m_FaceCountPerSegment = m_FacesSliderAndInputField.m_IntField.value;

                mesh.ExtrudeMesh();
            }
        }

        private void OnSelectionChanged()
        {
            var hasBezierMesh = false;
            meshes.Clear();

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent(typeof(BezierMesh), out Component mesh))
                {
                    meshes.Add((BezierMesh)mesh);
                    hasBezierMesh = true;
                }
            }

            m_Visisble = hasBezierMesh;

            if (m_FacesSliderAndInputField == null || m_RadiusSliderAndInputField == null || m_SegmentSliderAndInputField == null
                || Selection.gameObjects.Length == 0)
                return;

            SetParameterValues();
        }

        private void SetParameterValues()
        {
            // If only one bezier mesh is selected set overlay parameters to its parameters
            if (meshes.Count == 1)
            {
                m_SegmentSliderAndInputField.m_SliderInt.SetValueWithoutNotify(meshes[0].m_SegmentsPerUnit);
                m_SegmentSliderAndInputField.m_IntField.SetValueWithoutNotify(meshes[0].m_SegmentsPerUnit);
                m_RadiusSliderAndInputField.m_Slider.SetValueWithoutNotify(meshes[0].m_Radius);
                m_RadiusSliderAndInputField.m_FloatField.SetValueWithoutNotify(meshes[0].m_Radius);
                m_FacesSliderAndInputField.m_SliderInt.SetValueWithoutNotify(meshes[0].m_FaceCountPerSegment);
                m_FacesSliderAndInputField.m_IntField.SetValueWithoutNotify(meshes[0].m_FaceCountPerSegment);

                m_SegmentSliderAndInputField.m_IntField.showMixedValue = false;
                m_RadiusSliderAndInputField.m_FloatField.showMixedValue = false;
                m_FacesSliderAndInputField.m_IntField.showMixedValue = false;
            }
            // Show parameters that are equal across all selected bezier meshes, and blank out those that arent
            else
            {
                bool isRadiusEqual = false, isSegmentsEqual = false, isFacesEqual = false;
                var radius = -1f;
                var segment = -1;
                var face = -1;

                for (int i = 0; i < meshes.Count; i++)
                {
                    isRadiusEqual = Mathf.Approximately(radius, meshes[i].m_Radius);
                    isSegmentsEqual = Mathf.Approximately(segment, meshes[i].m_SegmentsPerUnit);
                    isFacesEqual = Mathf.Approximately(face, meshes[i].m_FaceCountPerSegment);

                    if (i == 0)
                    {
                        radius = meshes[i].m_Radius;
                        face = meshes[i].m_FaceCountPerSegment;
                        segment = meshes[i].m_SegmentsPerUnit;
                    }
                }

                m_RadiusSliderAndInputField.m_FloatField.showMixedValue = !isRadiusEqual;
                m_SegmentSliderAndInputField.m_IntField.showMixedValue = !isSegmentsEqual;
                m_FacesSliderAndInputField.m_IntField.showMixedValue = !isFacesEqual;
            }
        }

        public override void OnCreated()
        {
            base.OnCreated();
            Selection.selectionChanged += OnSelectionChanged;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public class SliderAndInputField : VisualElement
        {
            public Slider m_Slider;
            public SliderInt m_SliderInt;
            public FloatField m_FloatField;
            public IntegerField m_IntField;

            const string k_ElementStyle = "slider-and-input-field";


            public SliderAndInputField(string val, float min, float max, bool useIntField = false)
            {
                AddToClassList(k_ElementStyle);

                if (useIntField)
                {
                    m_SliderInt = new SliderInt(val, (int)min, (int)max);
                    Add(m_SliderInt);

                    m_IntField = new IntegerField();
                    Add(m_IntField);
                }
                else
                {
                    m_Slider = new Slider(val, min, max);
                    Add(m_Slider);

                    m_FloatField = new FloatField();
                    Add(m_FloatField);
                }
            }
        }
    }
}
