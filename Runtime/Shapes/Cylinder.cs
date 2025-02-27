using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [cylinder](../manual/Cylinder.html) shape.
    /// </summary>
    [Shape("Cylinder")]
    public class Cylinder : Shape
    {
        /// <summary>
        /// Sets the number of sides for the cylinder. The more sides you use, the smoother the sides of the cylinder become.
        /// The default value is 6. Valid values range from 4 to 64.
        /// </summary>
        [SerializeField]
        [Range(3, 64)]
        int m_AxisDivisions = 6;

        /// <summary>
        /// Sets the number of divisions to use for the height of the cylinder.
        /// The default value is 0.
        /// </summary>
        [Min(0)]
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
            m_AxisDivisions = 8;
            m_HeightCuts = 2;
            m_Smooth = false;
        }

        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Cylinder)
            {
                m_AxisDivisions = ((Cylinder)shape).m_AxisDivisions;
                m_HeightCuts = ((Cylinder)shape).m_HeightCuts;
                m_Smooth = ((Cylinder)shape).m_Smooth;
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

            float heightStep = height / (m_HeightCuts + 1);
            Vector2[] circle = new Vector2[m_AxisDivisions];

            // get a circle
            for (int i = 0; i < m_AxisDivisions; i++)
            {
                float angle = i * 360f / m_AxisDivisions;
                circle[i] = Math.PointInEllipseCircumference(xRadius, zRadius, angle, Vector2.zero, out _);
            }

            // add two because end caps
            Vector3[] vertices = new Vector3[(m_AxisDivisions * (m_HeightCuts + 1) * 4) + (m_AxisDivisions * 6)];
            Face[] faces = new Face[m_AxisDivisions * (m_HeightCuts + 1) + (m_AxisDivisions * 2)];

            // build vertex array
            int it = 0;

            // +1 to account for 0 height cuts
            for (int i = 0; i < m_HeightCuts + 1; i++)
            {
                float Y = i * heightStep - height * .5f;
                float Y2 = (i + 1) * heightStep - height * .5f;

                for (int n = 0; n < m_AxisDivisions; n++)
                {
                    vertices[it + 0] = new Vector3(circle[n + 0].x, Y, circle[n + 0].y);
                    vertices[it + 1] = new Vector3(circle[n + 0].x, Y2, circle[n + 0].y);

                    if (n != m_AxisDivisions - 1)
                    {
                        vertices[it + 2] = new Vector3(circle[n + 1].x, Y, circle[n + 1].y);
                        vertices[it + 3] = new Vector3(circle[n + 1].x, Y2, circle[n + 1].y);
                    }
                    else
                    {
                        vertices[it + 2] = new Vector3(circle[0].x, Y, circle[0].y);
                        vertices[it + 3] = new Vector3(circle[0].x, Y2, circle[0].y);
                    }

                    it += 4;
                }
            }

            // wind side faces
            int f = 0;
            for (int i = 0; i < m_HeightCuts + 1; i++)
            {
                for (int n = 0; n < m_AxisDivisions * 4; n += 4)
                {
                    int index = (i * (m_AxisDivisions * 4)) + n;
                    int zero = index;
                    int one = index + 1;
                    int two = index + 2;
                    int three = index + 3;

                    faces[f++] = new Face(
                        new int[6] { zero, one, two, one, three, two },
                        0,
                        AutoUnwrapSettings.tile,
                        m_Smooth ? 1 : -1,
                        -1,
                        -1,
                        false);
                }
            }

            // construct caps separately, cause they aren't wound the same way
            int ind = (m_AxisDivisions * (m_HeightCuts + 1) * 4);
            int f_ind = m_AxisDivisions * (m_HeightCuts + 1);

            for (int n = 0; n < m_AxisDivisions; n++)
            {
                // bottom faces
                var bottomCapHeight = -height * .5f;
                vertices[ind + 0] = new Vector3(circle[n].x, bottomCapHeight, circle[n].y);

                vertices[ind + 1] = new Vector3(0f, bottomCapHeight, 0f);

                if (n != m_AxisDivisions - 1)
                    vertices[ind + 2] = new Vector3(circle[n + 1].x, bottomCapHeight, circle[n + 1].y);
                else
                    vertices[ind + 2] = new Vector3(circle[000].x, bottomCapHeight, circle[000].y);

                faces[f_ind + n] = new Face(new int[3] { ind + 2, ind + 1, ind + 0 });

                ind += 3;

                // top faces
                var topCapHeight = height * .5f;
                vertices[ind + 0] = new Vector3(circle[n].x, topCapHeight, circle[n].y);
                vertices[ind + 1] = new Vector3(0f, topCapHeight, 0f);
                if (n != m_AxisDivisions - 1)
                    vertices[ind + 2] = new Vector3(circle[n + 1].x, topCapHeight, circle[n + 1].y);
                else
                    vertices[ind + 2] = new Vector3(circle[000].x, topCapHeight, circle[000].y);

                faces[f_ind + (n + m_AxisDivisions)] = new Face(new int[3] { ind + 0, ind + 1, ind + 2 });

                ind += 3;
            }

            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = rotation * vertices[i];

            mesh.RebuildWithPositionsAndFaces(vertices, faces);

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Cylinder))]
    public class CylinderDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_SidesContent = new GUIContent("Sides Count", L10n.Tr("Number of sides of the cylinder."));
        static readonly GUIContent k_HeightCutsContent = new GUIContent("Height Cuts", L10n.Tr("Number of divisions in the cylinder height."));
        static readonly GUIContent k_SmoothContent = new GUIContent("Smooth", L10n.Tr("Whether to smooth the edges of the cylinder."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Cylinder Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_AxisDivisions"), k_SidesContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HeightCuts"), k_HeightCutsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smooth"), k_SmoothContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
