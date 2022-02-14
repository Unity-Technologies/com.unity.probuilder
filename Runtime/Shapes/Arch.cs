using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [arch](../manual/Arch.html) shape.
    /// </summary>
    [Shape("Arch")]
    public class Arch : Shape
    {
        /// <summary>
        /// The thickness of the arch in meters. The larger the thickness, the smaller the opening becomes.
        /// The default value is 0.1. The minimum value is 0.01.
        /// </summary>
        [Min(0.01f)]
        [SerializeField]
        float m_Thickness = .1f;

        /// <summary>
        /// The number of sides for the arch. The more sides you use (relative to the size of the Radius), the smoother the arch becomes.
        /// The default value is 5. Valid values range from 3 to 200.
        /// </summary>
        [Range(3, 200)]
        [SerializeField]
        int m_NumberOfSides = 5;

        /// <summary>
        /// The circumference of the arch in degrees.
        /// The default value is 180. Valid values range from 1 to 360.
        /// </summary>
        [Range(1, 360)]
        [SerializeField]
        float m_ArchDegrees = 180;

        /// <summary>
        /// True to create faces for the ends of the arch (default).
        /// You can set this value to false as an optimization strategy.
        /// </summary>
        [SerializeField]
        bool m_EndCaps = true;

        /// <summary>
        /// True to smooth the edges of the polygons (default).
        /// </summary>
        [SerializeField]
        bool m_Smooth = true;

        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Arch)
            {
                Arch arch = ( (Arch) shape );
                m_Thickness = arch.m_Thickness;
                m_NumberOfSides = arch.m_NumberOfSides;
                m_ArchDegrees = arch.m_ArchDegrees;
                m_EndCaps = arch.m_EndCaps;
                m_Smooth = arch.m_Smooth;
            }
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

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var upDir = Vector3.Scale(rotation * Vector3.up, size) ;
            var rightDir = Vector3.Scale(rotation * Vector3.right, size) ;
            var forwardDir = Vector3.Scale(rotation * Vector3.forward, size) ;

            var xRadius = rightDir.magnitude / 2f;
            var yRadius = upDir.magnitude;
            var depth = forwardDir.magnitude / 2f;

            var radialCuts = m_NumberOfSides + 1;
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
            int smoothedFaceCount = 0;
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

                // left side bottom face
                if(angle < 360f && m_EndCaps)
                {
                    if(n == 0)
                        v.AddRange(GetFace(templateOut[n], templateIn[n], depth));
                }

                v.AddRange(qvo);
                v.AddRange(qvi);
                smoothedFaceCount += 2;

                if(angle < 360f && m_EndCaps)
                {
                    // right side bottom face
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

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < v.Count; i++)
                v[i] = Vector3.Scale(rotation * v[i], sizeSigns);

            mesh.GeometryWithPoints(v.ToArray());

            if(m_Smooth)
            {
                for(int i = ( angle < 360f && m_EndCaps ) ? 1 : 0; i < smoothedFaceCount; i++)
                    mesh.facesInternal[i].smoothingGroup = 1;
            }

            var sizeSign = sizeSigns.x * sizeSigns.y * sizeSigns.z;
            if(sizeSign < 0)
            {
                var faces = mesh.facesInternal;
                foreach(var face in faces)
                    face.Reverse();
            }

            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
            mesh.Refresh();

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Arch))]
    public class ArchDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_ThicknessContent = new GUIContent("Thickness", L10n.Tr("Thickness of the arch borders. Larger value creates a smaller opening."));
        static readonly GUIContent k_SidesContent = new GUIContent("Sides Count", L10n.Tr("Number of sides of the arch."));
        static readonly GUIContent k_CircumferenceContent = new GUIContent("Arch Circumference", L10n.Tr("Circumference of the arch in degrees."));
        static readonly GUIContent k_EndCapsContent = new GUIContent("End Caps", L10n.Tr("Whether to generate faces for the ends of the arch."));
        static readonly GUIContent k_SmoothContent = new GUIContent("Smooth", L10n.Tr("Whether to smooth the edges of the arch."));

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Arch Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Thickness"), k_ThicknessContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), k_SidesContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_ArchDegrees"), k_CircumferenceContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_EndCaps"), k_EndCapsContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smooth"), k_SmoothContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
