using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// An overlay that is responsible of displaying the parameters of the <see cref="BezierMesh"/> it is linked to.
    /// </summary>
    sealed class BezierMeshTransientOverlay : Overlay, ITransientOverlay
    {
        private const string k_OverlayName = "Bezier Mesh Inspector";
        private List<BezierMesh> m_SelectedMeshes;

        private SliderAndIntegerField m_SegmentSliderAndIntegerField;
        private SliderAndFloatField m_RadiusSliderAndFloatField;
        private SliderAndIntegerField m_FacesSliderAndIntegerField;

        static StyleSheet s_StyleSheet;

        private const string k_PathToStyleSheet =
            "Packages/com.unity.probuilder/Editor/Stylesheets/BezierMeshOverlayStyle.uss";

        private bool m_Visisble;
        public bool visible => m_Visisble;

        public BezierMeshTransientOverlay()
        {
            displayName = k_OverlayName;
            m_SelectedMeshes = new List<BezierMesh>();
            InitSliderAndInputFields();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                name = "Bezier Mesh Overlay"
            };

            if (s_StyleSheet == null)
                s_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_PathToStyleSheet);

            root.styleSheets.Add(s_StyleSheet);

            root.Add(m_SegmentSliderAndIntegerField);
            root.Add(m_RadiusSliderAndFloatField);
            root.Add(m_FacesSliderAndIntegerField);

            return root;
        }

        /// <summary>
        /// Initializes the slider and input fields, as well as registering to the callbacks to update their respective
        /// parameters in the <see cref="BezierMesh"/> that they are linked to.
        /// </summary>
        void InitSliderAndInputFields()
        {
            m_SegmentSliderAndIntegerField = new SliderAndIntegerField("Segments per Unit", BezierMesh.k_SegmentsMin,
                BezierMesh.k_SegmentsMax)
            {
                tooltip = L10n.Tr("Number of length-wise segments of the mesh per unit length")
            };
            m_SegmentSliderAndIntegerField.m_IntField.RegisterValueChangedCallback(evt => UpdateMeshSegments());
            m_SegmentSliderAndIntegerField.m_SliderInt.RegisterValueChangedCallback(evt => UpdateMeshSegments());

            m_RadiusSliderAndFloatField =
                new SliderAndFloatField("Radius", BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax)
                {
                    tooltip = L10n.Tr("The distance of the mesh from the center of the spline")
                };
            m_RadiusSliderAndFloatField.m_Slider.RegisterValueChangedCallback(evt => UpdateMeshRadius());
            m_RadiusSliderAndFloatField.m_FloatField.RegisterValueChangedCallback(evt => UpdateMeshRadius());

            m_FacesSliderAndIntegerField =
                new SliderAndIntegerField("Faces per Segment", BezierMesh.k_FacesMin, BezierMesh.k_FacesMax)
                {
                    tooltip = L10n.Tr("The number of faces around the bezier mesh at each segment")
                };
            m_FacesSliderAndIntegerField.m_SliderInt.RegisterValueChangedCallback(evt => UpdateMeshFaces());
            m_FacesSliderAndIntegerField.m_IntField.RegisterValueChangedCallback(evt => UpdateMeshFaces());
        }

        /// <summary>
        /// Updated the visibility of the overlay based on if at least one GameObject with a <see cref="BezierMesh"/>
        /// is currently selected.
        /// </summary>
        void OnSelectionChanged()
        {
            var hasBezierMesh = false;
            m_SelectedMeshes.Clear();

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent(out BezierMesh mesh))
                {
                    m_SelectedMeshes.Add(mesh);
                    hasBezierMesh = true;
                }
            }

            m_Visisble = hasBezierMesh;

            if (m_FacesSliderAndIntegerField == null || m_RadiusSliderAndFloatField == null ||
                m_SegmentSliderAndIntegerField == null || Selection.gameObjects.Length == 0)
                return;

            SetParameterValues();
        }

        /// <summary>
        /// Handles setting the parameters in the overlay to the selected Bezier Meshes' values if they are all equal,
        /// otherwise blanks out the parameters that aren't equal.
        /// </summary>
        void SetParameterValues()
        {
            bool isRadiusEqual = true, isSegmentsEqual = true, isFacesEqual = true;
            var count = m_SelectedMeshes.Count;
            var radius = count > 0 ? m_SelectedMeshes[0].Radius : -1f;
            var segments = count > 0 ? m_SelectedMeshes[0].SegmentsPerUnit : -1;
            var faces = count > 0 ? m_SelectedMeshes[0].FaceCountPerSegment : -1;

            for (int i = 1; i < count; ++i)
            {
                isRadiusEqual = Mathf.Approximately(radius, m_SelectedMeshes[i].Radius);
                isSegmentsEqual = Mathf.Approximately(segments, m_SelectedMeshes[i].SegmentsPerUnit);
                isFacesEqual = Mathf.Approximately(faces, m_SelectedMeshes[i].FaceCountPerSegment);
            }

            m_RadiusSliderAndFloatField.m_FloatField.showMixedValue = !isRadiusEqual;
            m_SegmentSliderAndIntegerField.m_IntField.showMixedValue = !isSegmentsEqual;
            m_FacesSliderAndIntegerField.m_IntField.showMixedValue = !isFacesEqual;

            if (isSegmentsEqual)
            {
                m_SegmentSliderAndIntegerField.m_SliderInt.value = segments;
                m_SegmentSliderAndIntegerField.m_IntField.value = segments;
            }

            if (isRadiusEqual)
            {
                m_RadiusSliderAndFloatField.m_Slider.value = radius;
                m_RadiusSliderAndFloatField.m_FloatField.value = radius;
            }

            if (isFacesEqual)
            {
                m_FacesSliderAndIntegerField.m_SliderInt.value = faces;
                m_FacesSliderAndIntegerField.m_IntField.value = faces;
            }
        }

        void UpdateMeshFaces()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
#if UNITY_EDITOR
                Undo.RecordObject(mesh, "Bezier Mesh Faces Updated");
#endif
                mesh.FaceCountPerSegment = m_FacesSliderAndIntegerField.m_IntField.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshSegments()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
#if UNITY_EDITOR
                Undo.RecordObject(mesh, "Bezier Mesh Segments per Unit Count Updated");
#endif
                mesh.SegmentsPerUnit = m_SegmentSliderAndIntegerField.m_IntField.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshRadius()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
#if UNITY_EDITOR
                Undo.RecordObject(mesh, "Bezier Mesh Radius Updated");
#endif
                mesh.Radius = m_RadiusSliderAndFloatField.m_FloatField.value;
                mesh.ExtrudeMesh();
            }
        }

        public override void OnCreated()
        {
            base.OnCreated();
            Selection.selectionChanged += OnSelectionChanged;
            Undo.undoRedoPerformed += SetParameterValues;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            Selection.selectionChanged -= OnSelectionChanged;
            Undo.undoRedoPerformed -= SetParameterValues;
        }
    }
}
