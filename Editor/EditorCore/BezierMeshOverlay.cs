//#if USING_SPLINES

using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// An overlay that is responsible of displaying the parameters of the <see cref="BezierMesh"/> it is linked to.
    /// </summary>
    sealed class BezierMeshOverlay : Overlay, ITransientOverlay
    {
        const string k_OverlayName = "Bezier Mesh Inspector";
        List<BezierMesh> m_SelectedMeshes;

        IntSlider m_SegmentIntSlider;
        FloatSlider m_RadiusFloatSlider;
        IntSlider m_FacesIntSlider;

        static StyleSheet s_StyleSheet;

        const string k_PathToStyleSheet =
            "Packages/com.unity.probuilder/Editor/Stylesheets/BezierMeshOverlayStyle.uss";

        bool m_Visible;
        public bool visible => m_Visible;

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

            root.Add(m_SegmentIntSlider);
            root.Add(m_RadiusFloatSlider);
            root.Add(m_FacesIntSlider);

            return root;
        }

        /// <summary>
        /// Initializes the slider and input fields, as well as registering to the callbacks to update their respective
        /// parameters in the <see cref="BezierMesh"/> that they are linked to.
        /// </summary>
        void InitSliderAndInputFields()
        {
            m_SegmentIntSlider = new IntSlider("Segments per Unit", BezierMesh.k_SegmentsMin,
                BezierMesh.k_SegmentsMax)
            {
                tooltip = L10n.Tr("Number of length-wise segments of the mesh per unit length")
            };
            m_SegmentIntSlider.slider.RegisterValueChangedCallback(evt => UpdateMeshSegments());

            m_RadiusFloatSlider =
                new FloatSlider("Radius", BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax)
                {
                    tooltip = L10n.Tr("The distance of the mesh from the center of the spline")
                };
            m_RadiusFloatSlider.slider.RegisterValueChangedCallback(evt => UpdateMeshRadius());

            m_FacesIntSlider =
                new IntSlider("Faces per Segment", BezierMesh.k_FacesMin, BezierMesh.k_FacesMax)
                {
                    tooltip = L10n.Tr("The number of faces around the bezier mesh at each segment")
                };
            m_FacesIntSlider.slider.RegisterValueChangedCallback(evt => UpdateMeshFaces());
        }

        /// <summary>
        /// Updated the visibility of the overlay based on if at least one GameObject with a <see cref="BezierMesh"/>
        /// is currently selected.
        /// </summary>
        void OnSelectionChanged()
        {
            if (m_FacesIntSlider == null || m_RadiusFloatSlider == null ||
                m_SegmentIntSlider == null || m_SelectedMeshes.Count == 0)
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

            m_RadiusFloatSlider.slider.showMixedValue = !isRadiusEqual;
            m_SegmentIntSlider.slider.showMixedValue = !isSegmentsEqual;
            m_FacesIntSlider.slider.showMixedValue = !isFacesEqual;

            if (isSegmentsEqual)
            {
                m_SegmentIntSlider.slider.value = segments;
            }

            if (isRadiusEqual)
            {
                m_RadiusFloatSlider.slider.value = radius;
            }

            if (isFacesEqual)
            {
                m_FacesIntSlider.slider.value = faces;
            }
        }

        void UpdateMeshFaces()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Faces Updated");
                mesh.faceCountPerSegment = m_FacesIntSlider.slider.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshSegments()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Segments per Unit Count Updated");
                mesh.segmentsPerUnit = m_SegmentIntSlider.slider.value;
                mesh.ExtrudeMesh();
            }
        }

        void UpdateMeshRadius()
        {
            foreach (var mesh in m_SelectedMeshes)
            {
                Undo.RecordObject(mesh, "Bezier Mesh Radius Updated");
                mesh.radius = m_RadiusFloatSlider.slider.value;
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
//#endif
