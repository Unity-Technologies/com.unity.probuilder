using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Cylinder")]
    public class Cylinder : Shape
    {
        [Range(4,64)]
        [SerializeField]
        int m_AxisDivisions = 6;

        [Min(0)]
        [SerializeField]
        int m_HeightCuts = 0;

        [SerializeField]
        bool m_Smooth = true;

        public override void CopyShape(Shape shape)
        {
            if(shape is Cylinder)
            {
                m_AxisDivisions = ((Cylinder)shape).m_AxisDivisions;
                m_HeightCuts = ((Cylinder)shape).m_HeightCuts;
                m_Smooth = ((Cylinder)shape).m_Smooth;
            }
        }

        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            var upLocalAxis = rotation * Vector3.up;
            upLocalAxis = Math.Abs(upLocalAxis);

            bounds = mesh.mesh.bounds;
            Vector3 boxSize = bounds.size;
            var maxAxis = Mathf.Max(Mathf.Max(
                    (1f - upLocalAxis.x)*boxSize.x,
                    (1f - upLocalAxis.y)*boxSize.y),
                (1f - upLocalAxis.z)*boxSize.z);
            boxSize.x = Mathf.Lerp(maxAxis, boxSize.x, upLocalAxis.x);
            boxSize.y = Mathf.Lerp(maxAxis, boxSize.y, upLocalAxis.y);
            boxSize.z = Mathf.Lerp(maxAxis, boxSize.z, upLocalAxis.z);
            bounds.size = boxSize;

            return bounds;
        }

        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(size);
            var radius = Mathf.Min(meshSize.x, meshSize.z) * .5f;
            var height = meshSize.y;

            if (m_AxisDivisions % 2 != 0)
                m_AxisDivisions++;

            float stepAngle = 360f / m_AxisDivisions;
            float heightStep = height / (m_HeightCuts + 1);

            Vector3[] circle = new Vector3[m_AxisDivisions];

            // get a circle
            for (int i = 0; i < m_AxisDivisions; i++)
            {
                float angle0 = stepAngle * i * Mathf.Deg2Rad;

                float x = Mathf.Cos(angle0) * radius;
                float z = Mathf.Sin(angle0) * radius;

                circle[i] = new Vector3(x, 0f, z);
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
                    vertices[it + 0] = new Vector3(circle[n + 0].x, Y, circle[n + 0].z);
                    vertices[it + 1] = new Vector3(circle[n + 0].x, Y2, circle[n + 0].z);

                    if (n != m_AxisDivisions - 1)
                    {
                        vertices[it + 2] = new Vector3(circle[n + 1].x, Y, circle[n + 1].z);
                        vertices[it + 3] = new Vector3(circle[n + 1].x, Y2, circle[n + 1].z);
                    }
                    else
                    {
                        vertices[it + 2] = new Vector3(circle[0].x, Y, circle[0].z);
                        vertices[it + 3] = new Vector3(circle[0].x, Y2, circle[0].z);
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
                vertices[ind + 0] = new Vector3(circle[n].x, bottomCapHeight, circle[n].z);

                vertices[ind + 1] = new Vector3(0f, bottomCapHeight, 0f);

                if (n != m_AxisDivisions - 1)
                    vertices[ind + 2] = new Vector3(circle[n + 1].x, bottomCapHeight, circle[n + 1].z);
                else
                    vertices[ind + 2] = new Vector3(circle[000].x, bottomCapHeight, circle[000].z);

                faces[f_ind + n] = new Face(new int[3] { ind + 2, ind + 1, ind + 0 });

                ind += 3;

                // top faces
                var topCapHeight = height * .5f;
                vertices[ind + 0] = new Vector3(circle[n].x, topCapHeight, circle[n].z);
                vertices[ind + 1] = new Vector3(0f, topCapHeight, 0f);
                if (n != m_AxisDivisions - 1)
                    vertices[ind + 2] = new Vector3(circle[n + 1].x, topCapHeight, circle[n + 1].z);
                else
                    vertices[ind + 2] = new Vector3(circle[000].x, topCapHeight, circle[000].z);

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
