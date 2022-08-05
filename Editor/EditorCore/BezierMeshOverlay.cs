#if USING_SPLINES && UNITY_2021_3_OR_NEWER

using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// An overlay that is responsible of displaying the parameters of the <see cref="BezierMesh"/> it is linked to.
    /// </summary>
#if !UNITY_2022_1_OR_NEWER
    [Overlay(typeof(SceneView), k_OverlayName, k_OverlayName)]
#endif
    class BezierMeshOverlay : Overlay, ITransientOverlay
    {
        const string k_OverlayName = "Bezier Mesh Inspector";
        List<BezierMesh> m_SelectedMeshes;

        SliderInt m_SegmentsSlider;
        Slider m_RadiusSlider;
        SliderInt m_FacesSlider;
        private const string k_SliderStyle = "slider-and-input-field";

        static StyleSheet s_StyleSheet;

        const string k_PathToStyleSheet =
            "Packages/com.unity.probuilder/Editor/Stylesheets/BezierMeshOverlayStyle.uss";

        bool m_Visible;
        public bool visible => m_Visible;

        public BezierMeshOverlay()
        {
            m_SelectedMeshes = new List<BezierMesh>();
            displayName = k_OverlayName;
            InitSliderAndInputFields();
        }

        public BezierMeshOverlay(List<BezierMesh> meshes, bool isVisible)
        {
            displayName = k_OverlayName;
            m_Visible = isVisible;
            m_SelectedMeshes = meshes;
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

            root.Add(m_SegmentsSlider);
            root.Add(m_RadiusSlider);
            root.Add(m_FacesSlider);

            return root;
        }

        /// <summary>
        /// Initializes the slider and input fields, as well as registering to the callbacks to update their respective
        /// parameters in the <see cref="BezierMesh"/> that they are linked to.
        /// </summary>
        void InitSliderAndInputFields()
        {
            m_SegmentsSlider = new SliderInt("Segments per Unit", BezierMesh.k_SegmentsMin,
                BezierMesh.k_SegmentsMax)
            {
                tooltip = L10n.Tr("Number of length-wise segments of the mesh per unit length"),
                showInputField = true,
                value = BezierMesh.k_SegmentsMin
            };
            m_SegmentsSlider.AddToClassList(k_SliderStyle);
            m_SegmentsSlider.RegisterValueChangedCallback(evt => UpdateMeshSegments());

            m_RadiusSlider =
                new Slider("Radius", BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax)
                {
                    tooltip = L10n.Tr("The distance of the mesh from the center of the spline"),
                    showInputField = true,
                    value = BezierMesh.k_RadiusMin
                };
            m_RadiusSlider.AddToClassList(k_SliderStyle);
            m_RadiusSlider.RegisterValueChangedCallback(evt => UpdateMeshRadius());

            m_FacesSlider =
                new SliderInt("Faces per Segment", BezierMesh.k_FacesMin, BezierMesh.k_FacesMax)
                {
                    tooltip = L10n.Tr("The number of faces around the bezier mesh at each segment"),
                    showInputField = true,
                    value = BezierMesh.k_FacesMin
                };
            m_FacesSlider.AddToClassList(k_SliderStyle);
            m_FacesSlider.RegisterValueChangedCallback(evt => UpdateMeshFaces());
        }

        /// <summary>
        /// Updates the visibility of the overlay if at least one GameObject with a <see cref="BezierMesh"/>
        /// is currently selected.
        /// </summary>
        void OnSelectionChanged()
        {
#if !UNITY_2022_1_OR_NEWER
            var hasBezierMesh = false;
            m_SelectedMeshes.Clear();
            foreach (var selected in Selection.gameObjects)
            {
                if (selected.TryGetComponent(out BezierMesh mesh))
                {
                    m_SelectedMeshes.Add(mesh);
                    hasBezierMesh = true;
                }
            }

            m_Visible = hasBezierMesh;
#endif

            if (m_FacesSlider == null || m_RadiusSlider == null ||
                m_SegmentsSlider == null || m_SelectedMeshes.Count == 0)
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
            var radius = count > 0 ? m_SelectedMeshes[0].radius : -1f;
            var segments = count > 0 ? m_SelectedMeshes[0].segmentsPerUnit : -1;
            var faces = count > 0 ? m_SelectedMeshes[0].faceCountPerSegment : -1;

            for (int i = 1; i < count; ++i)
            {
                isRadiusEqual = Mathf.Approximately(radius, m_SelectedMeshes[i].radius);
                isSegmentsEqual = Mathf.Approximately(segments, m_SelectedMeshes[i].segmentsPerUnit);
                isFacesEqual = Mathf.Approximately(faces, m_SelectedMeshes[i].faceCountPerSegment);
            }

            m_RadiusSlider.showMixedValue = !isRadiusEqual;
            m_SegmentsSlider.showMixedValue = !isSegmentsEqual;
            m_FacesSlider.showMixedValue = !isFacesEqual;

            if (isSegmentsEqual)
                m_SegmentsSlider.SetValueWithoutNotify(segments);

            if (isRadiusEqual)
                m_RadiusSlider.SetValueWithoutNotify(radius);

            if (isFacesEqual)
                m_FacesSlider.SetValueWithoutNotify(faces);
        }

        void UpdateMeshFaces()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Faces Updated");
                mesh.faceCountPerSegment = m_FacesSlider.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshSegments()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Segments per Unit Count Updated");
                mesh.segmentsPerUnit = m_SegmentsSlider.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshRadius()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Radius Updated");
                mesh.radius = m_RadiusSlider.value;
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
#endif
