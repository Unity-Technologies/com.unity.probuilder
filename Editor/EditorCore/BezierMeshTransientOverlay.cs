using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;

namespace UnityEngine.ProBuilder
{
    // skip on the attribute, instantiate it in BezierMeshEditor -> ping @karl for more info on this
    [Overlay(typeof(SceneView), overlayID, "Bezier Mesh Overlay")]
    public class BezierMeshTransientOverlay : Overlay, ITransientOverlay
    {
        private const string overlayID = "Bezier Mesh Overlay";
        private static List<BezierMesh> meshes = new List<BezierMesh>();

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

        void InitSliderAndInputFields()
        {
            m_SegmentSliderAndIntegerField = new SliderAndIntegerField("Segments per Unit", BezierMesh.k_SegmentsMin,
                BezierMesh.k_SegmentsMax)
            {
                tooltip = L10n.Tr("Number of length-wise segments of the mesh per unit length")
            };
            m_SegmentSliderAndIntegerField.m_IntField.RegisterValueChangedCallback(evt => UpdateMesh());
            m_SegmentSliderAndIntegerField.m_SliderInt.RegisterValueChangedCallback(evt => UpdateMesh());

            m_RadiusSliderAndFloatField =
                new SliderAndFloatField("Radius", BezierMesh.k_RadiusMin, BezierMesh.k_RadiusMax)
                {
                    tooltip = L10n.Tr("The distance of the mesh from the center of the spline")
                };
            m_RadiusSliderAndFloatField.m_Slider.RegisterValueChangedCallback(evt => UpdateMesh());
            m_RadiusSliderAndFloatField.m_FloatField.RegisterValueChangedCallback(evt => UpdateMesh());

            m_FacesSliderAndIntegerField =
                new SliderAndIntegerField("Faces per Segment", BezierMesh.k_FacesMin, BezierMesh.k_FacesMax)
                {
                    tooltip = L10n.Tr("The number of faces around the bezier mesh at each segment")
                };
            m_FacesSliderAndIntegerField.m_SliderInt.RegisterValueChangedCallback(evt => UpdateMesh());
            m_FacesSliderAndIntegerField.m_IntField.RegisterValueChangedCallback(evt => UpdateMesh());
        }

        void UpdateMesh()
        {
            foreach (var mesh in meshes)
            {
                mesh.SetParameters(m_RadiusSliderAndFloatField.m_FloatField.value,
                    m_FacesSliderAndIntegerField.m_IntField.value,
                    m_SegmentSliderAndIntegerField.m_IntField.value);
            }
        }

        void OnSelectionChanged()
        {
            var hasBezierMesh = false;
            meshes.Clear();

            foreach (var obj in Selection.gameObjects)
            {
                // use generic method instead -> avoids the explicit cast
                if (obj.TryGetComponent(typeof(BezierMesh), out Component mesh))
                {
                    meshes.Add((BezierMesh)mesh);
                    hasBezierMesh = true;
                }
            }

            m_Visisble = hasBezierMesh;

            if (m_FacesSliderAndIntegerField == null || m_RadiusSliderAndFloatField == null ||
                m_SegmentSliderAndIntegerField == null
                || Selection.gameObjects.Length == 0)
                return;

            SetParameterValues();
        }

        // Show parameters that are equal across all selected bezier meshes, and blank out those that arent
        void SetParameterValues()
        {
            bool isRadiusEqual = true, isSegmentsEqual = true, isFacesEqual = true;
            var count = meshes.Count;
            var radius = count > 0 ? meshes[0].m_Radius : -1f;
            var segments = count > 0 ? meshes[0].m_SegmentsPerUnit : -1;
            var faces = count > 0 ? meshes[0].m_FaceCountPerSegment : -1;

            for (int i = 1; i < count; ++i)
            {
                isRadiusEqual = Mathf.Approximately(radius, meshes[i].m_Radius);
                isSegmentsEqual = Mathf.Approximately(segments, meshes[i].m_SegmentsPerUnit);
                isFacesEqual = Mathf.Approximately(faces, meshes[i].m_FaceCountPerSegment);
            }

            if (isSegmentsEqual)
            {
                m_SegmentSliderAndIntegerField.m_SliderInt.SetValueWithoutNotify(segments);
                m_SegmentSliderAndIntegerField.m_IntField.SetValueWithoutNotify(segments);
            }

            if (isRadiusEqual)
            {
                m_RadiusSliderAndFloatField.m_Slider.SetValueWithoutNotify(radius);
                m_RadiusSliderAndFloatField.m_FloatField.SetValueWithoutNotify(radius);
            }

            if (isFacesEqual)
            {
                m_FacesSliderAndIntegerField.m_SliderInt.SetValueWithoutNotify(faces);
                m_FacesSliderAndIntegerField.m_IntField.SetValueWithoutNotify(faces);
            }

            m_RadiusSliderAndFloatField.m_FloatField.showMixedValue = !isRadiusEqual;
            m_SegmentSliderAndIntegerField.m_IntField.showMixedValue = !isSegmentsEqual;
            m_FacesSliderAndIntegerField.m_IntField.showMixedValue = !isFacesEqual;
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
