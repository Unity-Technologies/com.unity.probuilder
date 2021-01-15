using System.Collections.Generic;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Arch")]
    public class Arch : Shape
    {
        [Min(0.01f)]
        [SerializeField]
        float m_Thickness = .1f;

        [Range(3, 200)]
        [SerializeField]
        int m_NumberOfSides = 6;

        [Range(1, 360)]
        [SerializeField]
        float m_ArchDegrees = 180;

        [SerializeField]
        bool m_EndCaps = true;

        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            bounds.size = mesh.mesh.bounds.size;
            return bounds;
        }

        Vector3[] GetFace(Vector2 vertex1, Vector2 vertex2, float depth)
        {
            return new Vector3[4]
            {
                new Vector3(vertex1.x, vertex1.y, depth),
                new Vector3(vertex2.x,  vertex2.y, depth),
                new Vector3(vertex1.x, vertex1.y, -depth),
                new Vector3(vertex2.x, vertex2.y, -depth)
            };
        }

        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var upDir = Vector3.Scale(rotation * Vector3.up, size) ;
            var rightDir = Vector3.Scale(rotation * Vector3.right, size) ;
            var forwardDir = Vector3.Scale(rotation * Vector3.forward, size) ;

            var xRadius = rightDir.magnitude / 2f;
            var yRadius = upDir.magnitude;
            var depth = forwardDir.magnitude / 2f;

            var radialCuts = m_NumberOfSides;
            var angle = m_ArchDegrees;
            var templateOut = new Vector2[radialCuts];
            var templateIn = new Vector2[radialCuts];

            if(angle < 90f)
                xRadius *= 2f;
            else if(angle < 180f)
                xRadius *= 1f+ Mathf.Lerp(1f, 0f, Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad)));
            else if(angle > 180f)
                yRadius /= 1f+ Mathf.Lerp(0f, 1f, (angle - 180f)/90f);

            for (int i = 0; i < radialCuts; i++)
            {
                var currentAngle = i * ( angle / ( radialCuts - 1 ) );
                Vector2 tangent;
                templateOut[i] = Math.PointInEllipseCircumference(xRadius, yRadius, currentAngle, Vector2.zero, out tangent);
                templateIn[i] = Math.PointInEllipseCircumference(xRadius - m_Thickness, yRadius - m_Thickness, currentAngle, Vector2.zero, out tangent);
            }

            List<Vector3> v = new List<Vector3>();

            Vector2 tmp, tmp2, tmp3, tmp4;

            float y = -depth;

            for (int n = 0; n < radialCuts - 1; n++)
            {
                // outside faces
                tmp = templateOut[n];
                tmp2 = n < (radialCuts - 1) ? templateOut[n + 1] : templateOut[n];

                Vector3[] qvo = GetFace(tmp, tmp2, -depth);

                // inside faces
                tmp = templateIn[n];
                tmp2 = n < (radialCuts - 1) ? templateIn[n + 1] : templateIn[n];

                Vector3[] qvi = GetFace(tmp2, tmp, -depth);

                v.AddRange(qvo);

                if (n != radialCuts - 1)
                    v.AddRange(qvi);

                // left side bottom face
                if (angle < 360f && m_EndCaps)
                {
                    if (n == 0)
                        v.AddRange(GetFace(templateOut[n], templateIn[n], depth));

                    // ride side bottom face
                    if (n == radialCuts - 2)
                        v.AddRange(GetFace(templateIn[n+1], templateOut[n+1], depth));
                }
            }

            // build front and back faces
            for (int i = 0; i < radialCuts - 1; i++)
            {
                tmp = templateOut[i];
                tmp2 = (i < radialCuts - 1) ? templateOut[i + 1] : templateOut[i];
                tmp3 = templateIn[i];
                tmp4 = (i < radialCuts - 1) ? templateIn[i + 1] : templateIn[i];

                // front
                Vector3[] tpb = new Vector3[4]
                {
                    new Vector3(tmp.x, tmp.y, depth),
                    new Vector3(tmp2.x, tmp2.y, depth),
                    new Vector3(tmp3.x, tmp3.y, depth),
                    new Vector3(tmp4.x, tmp4.y, depth),
                };

                // back
                Vector3[] tpt = new Vector3[4]
                {
                    new Vector3(tmp2.x, tmp2.y, y),
                    new Vector3(tmp.x,  tmp.y, y),
                    new Vector3(tmp4.x, tmp4.y, y),
                    new Vector3(tmp3.x, tmp3.y, y)
                };

                v.AddRange(tpb);
                v.AddRange(tpt);
            }

            for(int i = 0; i < v.Count; i++)
                v[i] = rotation * v[i];

            mesh.GeometryWithPoints(v.ToArray());

            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

    [CustomPropertyDrawer(typeof(Arch))]
    public class ArchDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static GUIContent m_Content = new GUIContent();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Arch Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                m_Content.text = "Thickness";
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Thickness"), m_Content);
                m_Content.text = "Sides Count";
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), m_Content);
                m_Content.text = "Arch Circumference";
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_ArchDegrees"), m_Content);
                m_Content.text = "End Caps";
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_EndCaps"), m_Content);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
