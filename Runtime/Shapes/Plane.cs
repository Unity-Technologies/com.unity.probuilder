using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Plane")]
    public class Plane : Shape
    {
        [Min(0)]
        [SerializeField]
        int m_HeightSegments = 1;

        [Min(0)]
        [SerializeField]
        int m_WidthSegments = 1;

        public override void CopyShape(Shape shape)
        {
            if(shape is Plane)
            {
                m_HeightSegments = ((Plane)shape).m_HeightSegments;
                m_WidthSegments = ((Plane)shape).m_WidthSegments;
            }
        }

        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            int w = m_WidthSegments + 1;
            int h = m_HeightSegments + 1;

            Vector2[] p = new Vector2[(w * h) * 4];
            Vector3[] v = new Vector3[(w * h) * 4];

            float width = 1f, height = 1f;
            int i = 0;
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float x0 = x * (width / w) - (width / 2f);
                        float x1 = (x + 1) * (width / w) - (width / 2f);

                        float y0 = y * (height / h) - (height / 2f);
                        float y1 = (y + 1) * (height / h) - (height / 2f);

                        p[i + 0] = new Vector2(x0,    y0);
                        p[i + 1] = new Vector2(x1,    y0);
                        p[i + 2] = new Vector2(x0,    y1);
                        p[i + 3] = new Vector2(x1,    y1);

                        i += 4;
                    }
                }
            }

            for(i = 0; i < v.Length; i++)
                v[i] = new Vector3(p[i].y, 0f, p[i].x);

            mesh.GeometryWithPoints(v);

            return mesh.mesh.bounds;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Plane))]
    public class PlaneDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_HeightCutsContent = new GUIContent("Height Cuts", L10n.Tr("Number of divisions in the plane height."));
        static readonly GUIContent k_WidthCutsContent = new GUIContent("Width Cuts", L10n.Tr("Number of divisions in the plane width."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Plane Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HeightSegments"), k_HeightCutsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_WidthSegments"), k_WidthCutsContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}

