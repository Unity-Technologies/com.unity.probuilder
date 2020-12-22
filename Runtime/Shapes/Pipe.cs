using System.Collections.Generic;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Pipe")]
    public class Pipe : Shape
    {
        [Min(0.01f)]
        [SerializeField]
        float m_Thickness = .25f;

        [Range(3, 64)]
        [SerializeField]
        int m_NumberOfSides = 6;

        [Range(1, 32)]
        [SerializeField]
        int m_HeightSegments = 1;

        public override void UpdatePivot(ProBuilderMesh mesh, PivotLocation pivotLocation)
        {
            if(mesh != null && mesh.mesh != null)
            {
                mesh.SetPivot(pivotLocation, 1);
                UpdateBounds(mesh);
            }
        }

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var height = size.y;
            var xRadius = size.x / 2f;
            var zRadius = size.z / 2f;
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
            for (int i = 0; i < m_HeightSegments; i++)
            {
                // height subdivisions
                float y = i * (height / m_HeightSegments) - baseY;
                float y2 = (i + 1) * (height / m_HeightSegments) - baseY;

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
            mesh.GeometryWithPoints(v.ToArray());

            m_ShapeBox = mesh.mesh.bounds;
        }
    }

    [CustomPropertyDrawer(typeof(Pipe))]
    public class PipeDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Pipe Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Thickness"), new GUIContent("Thickness"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), new GUIContent("Sides Count"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HeightSegments"), new GUIContent("Height Subdivisions"));
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
