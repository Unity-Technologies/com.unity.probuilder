using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [pipe](../manual/Pipe.html) shape.
    /// </summary>
    [Shape("Pipe")]
    public class Pipe : Shape
    {
        /// <summary>
        /// Sets the thickness of the walls of the pipe in meters. The thicker the value, the smaller the hole becomes.
        /// The default value is 0.25. The minimum value is 0.01.
        /// </summary>
        [Min(0.01f)]
        [SerializeField]
        float m_Thickness = .25f;

        /// <summary>
        /// Sets the number of sides for the pipe. The more sides you use, the smoother the sides of the pipe become.
        /// The default value is 6. Valid values range from 3 to 64.
        /// </summary>
        [Range(3, 64)]
        [SerializeField]
        int m_NumberOfSides = 6;

        /// <summary>
        /// Sets the number of divisions to use for the height of the pipe.
        /// The default value is 0. Valid values range from 0 to 31.
        /// </summary>
        [Range(0, 31)]
        [SerializeField]
        int m_HeightCuts = 0;

        /// <summary>
        /// Determines whether to smooth the edges of the polygons.
        /// This property is enabled by default.
        /// </summary>
        [SerializeField]
        bool m_Smooth = true;
        
        internal override void SetParametersToBuiltInShape()
        {
            m_Thickness = 0.25f;
            m_NumberOfSides = 8;
            m_HeightCuts = 2;
            m_Smooth = false;
        }

        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Pipe)
            {
                Pipe pipe = (Pipe) shape;
                m_Thickness = pipe.m_Thickness;
                m_NumberOfSides = pipe.m_NumberOfSides;
                m_HeightCuts = pipe.m_HeightCuts;
                m_Smooth = pipe.m_Smooth;
            }
        }

        /// <inheritdoc/>
        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            bounds.size = size;
            return bounds;
        }

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var upDir = Vector3.Scale(rotation * Vector3.up, size) ;
            var rightDir = Vector3.Scale(rotation * Vector3.right, size) ;
            var forwardDir = Vector3.Scale(rotation * Vector3.forward, size) ;

            var height = upDir.magnitude;
            var xRadius = rightDir.magnitude / 2f;
            var zRadius = forwardDir.magnitude / 2f;

            // template is outer ring - radius refers to outer ring always
            Vector2[] templateOut = new Vector2[m_NumberOfSides];
            Vector2[] templateIn = new Vector2[m_NumberOfSides];

            Vector2 tangent;
            for (int i = 0; i < m_NumberOfSides; i++)
            {
                float angle = i * ( 360f / m_NumberOfSides );
                templateOut[i] = Math.PointInEllipseCircumference(xRadius, zRadius, angle, Vector2.zero, out tangent);

                Vector2 tangentOrtho = new Vector2(-tangent.y, tangent.x);
                templateIn[i] = templateOut[i] + (m_Thickness * tangentOrtho);
            }

            List<Vector3> v = new List<Vector3>();
            var baseY = height / 2f;
            // build out sides
            Vector2 tmp, tmp2, tmp3, tmp4;
            var heightSegments = m_HeightCuts + 1;
            for (int i = 0; i < heightSegments; i++)
            {
                // height subdivisions
                float y = i * (height / heightSegments) - baseY;
                float y2 = (i + 1) * (height / heightSegments) - baseY;

                for (int n = 0; n < m_NumberOfSides; n++)
                {
                    tmp = templateOut[n];
                    tmp2 = n < (m_NumberOfSides - 1) ? templateOut[n + 1] : templateOut[0];

                    // outside quads
                    Vector3[] qvo = new Vector3[4]
                    {
                        new Vector3(tmp2.x, y, tmp2.y),
                        new Vector3(tmp.x, y, tmp.y),
                        new Vector3(tmp2.x, y2, tmp2.y),
                        new Vector3(tmp.x, y2, tmp.y)
                    };

                    // inside quad
                    tmp = templateIn[n];
                    tmp2 = n < (m_NumberOfSides - 1) ? templateIn[n + 1] : templateIn[0];
                    Vector3[] qvi = new Vector3[4]
                    {
                        new Vector3(tmp.x, y, tmp.y),
                        new Vector3(tmp2.x, y, tmp2.y),
                        new Vector3(tmp.x, y2, tmp.y),
                        new Vector3(tmp2.x, y2, tmp2.y)
                    };

                    v.AddRange(qvo);
                    v.AddRange(qvi);
                }
            }

            // build top and bottom
            for (int i = 0; i < m_NumberOfSides; i++)
            {
                tmp = templateOut[i];
                tmp2 = (i < m_NumberOfSides - 1) ? templateOut[i + 1] : templateOut[0];
                tmp3 = templateIn[i];
                tmp4 = (i < m_NumberOfSides - 1) ? templateIn[i + 1] : templateIn[0];

                // top
                Vector3[] tpt = new Vector3[4]
                {
                    new Vector3(tmp2.x, height-baseY, tmp2.y),
                    new Vector3(tmp.x,  height-baseY, tmp.y),
                    new Vector3(tmp4.x, height-baseY, tmp4.y),
                    new Vector3(tmp3.x, height-baseY, tmp3.y)
                };

                // bottom
                Vector3[] tpb = new Vector3[4]
                {
                    new Vector3(tmp.x, -baseY, tmp.y),
                    new Vector3(tmp2.x, -baseY, tmp2.y),
                    new Vector3(tmp3.x, -baseY, tmp3.y),
                    new Vector3(tmp4.x, -baseY, tmp4.y),
                };

                v.AddRange(tpb);
                v.AddRange(tpt);
            }

            for(int i = 0; i < v.Count; i++)
                 v[i] = rotation * v[i];

            mesh.GeometryWithPoints(v.ToArray());

            //Smooth internal and external faces
            if(m_Smooth)
            {
                int smoothCount = 2 * heightSegments * m_NumberOfSides;
                for(int i = 0; i < smoothCount; i++)
                    mesh.facesInternal[i].smoothingGroup = 1;
            }

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Pipe))]
    public class PipeDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_ThicknessContent = new GUIContent("Thickness", L10n.Tr("Thickness of the pipe borders. Larger value creates a smaller hole."));
        static readonly GUIContent k_SidesContent = new GUIContent("Sides Count", L10n.Tr("Number of sides of the pipe."));
        static readonly GUIContent k_HeightCutsContent = new GUIContent("Height Cuts", L10n.Tr("Number of divisions in the pipe height."));
        static readonly GUIContent k_SmoothContent = new GUIContent("Smooth", L10n.Tr("Whether to smooth the edges of the pipe."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Pipe Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Thickness"), k_ThicknessContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), k_SidesContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HeightCuts"), k_HeightCutsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smooth"), k_SmoothContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
