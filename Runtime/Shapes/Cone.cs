using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [cone](../manual/Cone.html) shape.
    /// </summary>
    [Shape("Cone")]
    public class Cone : Shape
    {
        /// <summary>
        /// Sets the number of sides for the cone. The more sides you use, the smoother the sides of the cone become.
        /// The default value is 6. Valid values range from 3 to 64.
        /// </summary>
        [Range(3,64)]
        [SerializeField]
        internal int m_NumberOfSides = 6;

        float m_Radius = 0;

        /// <summary>
        /// Determines whether to smooth the edges of the polygons.
        /// This is enabled by default.
        /// </summary>
        [SerializeField]
        bool m_Smooth = true;

        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Cone)
            {
                Cone cone = (Cone) shape;
                m_NumberOfSides = cone.m_NumberOfSides;
                m_Radius = cone.m_Radius;
                m_Smooth = cone.m_Smooth;
            }
        }

        /// <inheritdoc/>
        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            var upLocalAxis = rotation * Vector3.up;
            upLocalAxis = Math.Abs(upLocalAxis);

            Vector3 boxSize = mesh.mesh.bounds.size;
            boxSize.x = Mathf.Lerp(m_Radius * 2f, boxSize.x, upLocalAxis.x);
            boxSize.y = Mathf.Lerp(m_Radius * 2f, boxSize.y, upLocalAxis.y);
            boxSize.z = Mathf.Lerp(m_Radius * 2f, boxSize.z, upLocalAxis.z);
            bounds.size = boxSize;

            return bounds;
        }

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(size);

            m_Radius = System.Math.Min(meshSize.x, meshSize.z);
            var height = meshSize.y;

            var subdivAxis = m_NumberOfSides;
            // template is outer ring - radius refers to outer ring always
            Vector3[] template = new Vector3[subdivAxis];

            for (int i = 0; i < subdivAxis; i++)
            {
                Vector2 ct = Math.PointInCircumference(m_Radius, i * (360f / subdivAxis), Vector2.zero);
                template[i] = new Vector3(ct.x, -height / 2f, ct.y);
            }

            List<Vector3> v = new List<Vector3>();
            List<Face> f = new List<Face>();

            // build sides
            for (int i = 0; i < subdivAxis; i++)
            {
                // side face
                v.Add(template[i]);
                v.Add((i < subdivAxis - 1) ? template[i + 1] : template[0]);
                v.Add(Vector3.up * height / 2f);

                // bottom face
                v.Add(template[i]);
                v.Add((i < subdivAxis - 1) ? template[i + 1] : template[0]);
                v.Add(Vector3.down * height / 2f);
            }

            List<Face> sideFaces = new List<Face>();
            for (int i = 0; i < subdivAxis * 6; i += 6)
            {
                Face face = new Face(new int[3] { i + 2, i + 1, i + 0 });
                face.smoothingGroup = m_Smooth ? 1 : 0;
                f.Add(face);
                sideFaces.Add(face);
                f.Add(new Face(new int[3] { i + 3, i + 4, i + 5 }));
            }

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < v.Count; i++)
                v[i] = Vector3.Scale(rotation * v[i], sizeSigns);

            var sizeSign = Mathf.Sign(size.x) * Mathf.Sign(size.y) * Mathf.Sign(size.z);
            if(sizeSign < 0)
            {
                foreach(var face in f)
                    face.Reverse();
            }

            mesh.RebuildWithPositionsAndFaces(v, f);

            mesh.unwrapParameters = new UnwrapParameters()
            {
                packMargin = 30f
            };

            // Set the UVs manually for the side faces, so that they are uniform.
            // Calculate the UVs for the first face, then set the others to the same.
            var firstFace = sideFaces[0];
            var uv = firstFace.uv;
            uv.anchor = AutoUnwrapSettings.Anchor.LowerLeft;
            firstFace.uv = uv;
            firstFace.manualUV = true;
            // Always use up vector for projection of side faces.
            // Otherwise the lines in the PB texture end up crooked.
            UvUnwrapping.Unwrap(mesh, firstFace, projection: Vector3.up);
            for (int i = 1; i < sideFaces.Count; i++)
            {
                var sideFace = sideFaces[i];
                sideFace.manualUV = true;
                UvUnwrapping.CopyUVs(mesh, firstFace, sideFace);
            }
            mesh.RefreshUV(sideFaces);

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Cone))]
    public class ConeDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_SidesContent = new GUIContent("Sides Count", L10n.Tr("Number of sides of the cone."));
        static readonly GUIContent k_SmoothContent = new GUIContent("Smooth", L10n.Tr("Whether to smooth the edges of the arch."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Cone Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), k_SidesContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Smooth"), k_SmoothContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
