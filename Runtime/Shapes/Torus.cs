using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Torus")]
    public class Torus : Shape
    {
        [Range(3, 64)]
        [SerializeField]
        int m_Rows = 16;

        [Range(3, 64)]
        [SerializeField]
        int m_Columns = 24;

        [Min(0.01f)]
        [SerializeField]
        float m_TubeRadius = .1f;

        [Range(0, 360)]
        [SerializeField]
        float m_HorizontalCircumference = 360;

        [Range(0, 360)]
        [SerializeField]
        float m_VerticalCircumference = 360;

        [SerializeField]
        bool m_Smooth = true;

        public override void CopyShape(Shape shape)
        {
            if(shape is Torus)
            {
                Torus torus = (Torus) shape;
                m_Rows = torus.m_Rows;
                m_Columns = torus.m_Columns;
                m_TubeRadius = torus.m_TubeRadius;
                m_HorizontalCircumference = torus.m_HorizontalCircumference;
                m_VerticalCircumference = torus.m_VerticalCircumference;
                m_Smooth = torus.m_Smooth;
            }
        }

        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            bounds.size = mesh.mesh.bounds.size;
            return bounds;
        }

        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(rotation * size);

            var xOuterRadius = Mathf.Clamp(meshSize.x /2f ,.01f, 2048f);
            var yOuterRadius = Mathf.Clamp(meshSize.z /2f ,.01f, 2048f);
            int clampedRows = Mathf.Clamp(m_Rows + 1, 4, 128);
            int clampedColumns = Mathf.Clamp(m_Columns + 1, 4, 128);
            float clampedTubeRadius = Mathf.Clamp(m_TubeRadius, .01f, Mathf.Min(xOuterRadius, yOuterRadius) - .001f);

            xOuterRadius -= clampedTubeRadius;
            yOuterRadius -= clampedTubeRadius;
            float clampedHorizontalCircumference = Mathf.Clamp(m_HorizontalCircumference, .01f, 360f);
            float clampedVerticalCircumference = Mathf.Clamp(m_VerticalCircumference, .01f, 360f);

            List<Vector3> vertices = new List<Vector3>();

            int col = clampedColumns - 1;

            float clampedRadius = xOuterRadius;
            Vector3[] cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, Quaternion.Euler(0,0,0), clampedRadius);

            Vector2 ellipseCoord;
            for (int i = 1; i < clampedColumns; i++)
            {
                vertices.AddRange(cir);
                float angle = (i / (float)col) * clampedHorizontalCircumference;
                //Compute the coordinates of the current point
                ellipseCoord = new Vector2( xOuterRadius * Mathf.Cos(Mathf.Deg2Rad * angle),
                                            yOuterRadius * Mathf.Sin(Mathf.Deg2Rad * angle) );

                //Compute the tangent direction to know how to orient the current slice
                var tangent = new Vector2( -ellipseCoord.y / (yOuterRadius * yOuterRadius), ellipseCoord.x / (xOuterRadius * xOuterRadius));
                Quaternion rot =  Quaternion.Euler(Vector3.up * Vector2.SignedAngle(Vector2.up, tangent.normalized));

                //Get the slice/circle that must be placed at this position
                cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, rot, new Vector3(ellipseCoord.x, 0, -ellipseCoord.y));
                vertices.AddRange(cir);
            }

            List<Face> faces = new List<Face>();
            int fc = 0;

            // faces
            for (int i = 0; i < (clampedColumns - 1) * 2; i += 2)
            {
                for (int n = 0; n < clampedRows - 1; n++)
                {
                    int a = (i + 0) * ((clampedRows - 1) * 2) + (n * 2);
                    int b = (i + 1) * ((clampedRows - 1) * 2) + (n * 2);

                    int c = (i + 0) * ((clampedRows - 1) * 2) + (n * 2) + 1;
                    int d = (i + 1) * ((clampedRows - 1) * 2) + (n * 2) + 1;

                    faces.Add(new Face(new int[] { a, b, c, b, d, c }));
                    faces[fc].smoothingGroup = m_Smooth ? 1 : -1;
                    faces[fc].manualUV = true;

                    fc++;
                }
            }

            for(int i = 0; i < vertices.Count; ++i)
               vertices[i] = rotation * vertices[i];

            mesh.RebuildWithPositionsAndFaces(vertices, faces);

            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
            mesh.Refresh();

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }


        static Vector3[] GetCirclePoints(int segments, float radius, float circumference, Quaternion rotation, float offset)
        {
            float seg = (float)segments - 1;

            Vector3[] v = new Vector3[(segments - 1) * 2];
            v[0] = new Vector3(Mathf.Cos(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);
            v[1] = new Vector3(Mathf.Cos(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);

            v[0] = rotation * ((v[0] + Vector3.right * offset));
            v[1] = rotation * ((v[1] + Vector3.right * offset));

            int n = 2;

            for (int i = 2; i < segments; i++)
            {
                float rad = ((i / seg) * circumference) * Mathf.Deg2Rad;

                v[n + 0] = v[n - 1];
                v[n + 1] = rotation * (new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f) + Vector3.right * offset);

                n += 2;
            }

            return v;
        }

        static Vector3[] GetCirclePoints(int segments, float radius, float circumference, Quaternion rotation, Vector3 offset)
        {
            float seg = (float)segments - 1;

            Vector3[] v = new Vector3[(segments - 1) * 2];
            v[0] = new Vector3(Mathf.Cos(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);
            v[1] = new Vector3(Mathf.Cos(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);

            v[0] = rotation * v[0] + offset;
            v[1] = rotation * v[1] + offset;

            int n = 2;

            for (int i = 2; i < segments; i++)
            {
                float rad = ((i / seg) * circumference) * Mathf.Deg2Rad;

                v[n + 0] = v[n - 1];
                v[n + 1] = rotation * new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f) + offset;

                n += 2;
            }

            return v;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Torus))]
    public class TorusDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_RowsContent = new GUIContent("Rows", L10n.Tr("Set the number of faces used to define the tube's circumference."));
        static readonly GUIContent k_ColumnsContent = new GUIContent("Columns", L10n.Tr("Set the number of faces used to define the torus's circumference / tube's length."));
        static readonly GUIContent k_RadiusContent = new GUIContent("Tube Radius", L10n.Tr("Set the tube's radius."));
        static readonly GUIContent k_HorCircumferenceContent = new GUIContent("Hor. Circ.", L10n.Tr("Circumference of the torus in degrees."));
        static readonly GUIContent k_VertCircumferenceContent = new GUIContent("Vert. Circ.", L10n.Tr("Circumference of the torus' inner pipe in degrees."));
        static readonly GUIContent k_SmoothContent = new GUIContent("Smooth", L10n.Tr("Whether to smooth the edges of the torus."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Torus Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_TubeRadius"), k_RadiusContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Rows"), k_RowsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Columns"), k_ColumnsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HorizontalCircumference"), k_HorCircumferenceContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_VerticalCircumference"), k_VertCircumferenceContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smooth"), k_SmoothContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
