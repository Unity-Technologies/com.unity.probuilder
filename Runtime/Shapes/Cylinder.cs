using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

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

        [Min(-1)]
        [SerializeField]
        int m_Smoothing = -1;

        public override void UpdateBounds(ProBuilderMesh mesh)
        {
            m_ShapeBox = mesh.mesh.bounds;
            Vector3 boxSize = m_ShapeBox.size;
            boxSize.x = boxSize.z = Mathf.Max(boxSize.x, boxSize.z);
            m_ShapeBox.size = boxSize;
        }

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var radius = Mathf.Min(size.x, size.z) * .5f;
            var height = size.y;

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
            Vector3[] verts = new Vector3[(m_AxisDivisions * (m_HeightCuts + 1) * 4) + (m_AxisDivisions * 6)];
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
                    verts[it + 0] = new Vector3(circle[n + 0].x, Y, circle[n + 0].z);
                    verts[it + 1] = new Vector3(circle[n + 0].x, Y2, circle[n + 0].z);

                    if (n != m_AxisDivisions - 1)
                    {
                        verts[it + 2] = new Vector3(circle[n + 1].x, Y, circle[n + 1].z);
                        verts[it + 3] = new Vector3(circle[n + 1].x, Y2, circle[n + 1].z);
                    }
                    else
                    {
                        verts[it + 2] = new Vector3(circle[0].x, Y, circle[0].z);
                        verts[it + 3] = new Vector3(circle[0].x, Y2, circle[0].z);
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
                        m_Smoothing,
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
                verts[ind + 0] = new Vector3(circle[n].x, bottomCapHeight, circle[n].z);

                verts[ind + 1] = new Vector3(0f, bottomCapHeight, 0f);

                if (n != m_AxisDivisions - 1)
                    verts[ind + 2] = new Vector3(circle[n + 1].x, bottomCapHeight, circle[n + 1].z);
                else
                    verts[ind + 2] = new Vector3(circle[000].x, bottomCapHeight, circle[000].z);

                faces[f_ind + n] = new Face(new int[3] { ind + 2, ind + 1, ind + 0 });

                ind += 3;

                // top faces
                var topCapHeight = height * .5f;
                verts[ind + 0] = new Vector3(circle[n].x, topCapHeight, circle[n].z);
                verts[ind + 1] = new Vector3(0f, topCapHeight, 0f);
                if (n != m_AxisDivisions - 1)
                    verts[ind + 2] = new Vector3(circle[n + 1].x, topCapHeight, circle[n + 1].z);
                else
                    verts[ind + 2] = new Vector3(circle[000].x, topCapHeight, circle[000].z);

                faces[f_ind + (n + m_AxisDivisions)] = new Face(new int[3] { ind + 0, ind + 1, ind + 2 });

                ind += 3;
            }

            mesh.RebuildWithPositionsAndFaces(verts, faces);

            m_ShapeBox = mesh.mesh.bounds;
            Vector3 boxSize = m_ShapeBox.size;
            boxSize.x = boxSize.z = Mathf.Max(boxSize.x, boxSize.z);
            m_ShapeBox.size = boxSize;
        }
    }

    [CustomPropertyDrawer(typeof(Cylinder))]
    public class CylinderDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Cylinder Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_AxisDivisions"), new GUIContent("Sides Count"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HeightCuts"), new GUIContent("Height Cuts"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smoothing"), new GUIContent("Smoothing Group"));
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
