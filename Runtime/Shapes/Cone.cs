using System.Collections.Generic;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Cone")]
    public class Cone : Shape
    {
        [Range(3,64)]
        [SerializeField]
        internal int m_NumberOfSides = 6;

        float m_Radius = 0;

        public override void UpdateBounds(ProBuilderMesh mesh)
        {
            Vector3 boxSize = mesh.mesh.bounds.size;
            boxSize.x = boxSize.z = m_Radius * 2f;
            m_ShapeBox.size = boxSize;
        }

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 meshSize, Quaternion rotation)
        {
            meshSize = Math.Abs(meshSize);

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
                f.Add(face);
                sideFaces.Add(face);
                f.Add(new Face(new int[3] { i + 3, i + 4, i + 5 }));
            }

            for(int i = 0; i < v.Count; i++)
                v[i] = rotation * v[i];

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

            m_ShapeBox.center = Vector3.zero;
            Vector3 boxSize = mesh.mesh.bounds.size;
            boxSize.x = boxSize.z = m_Radius * 2f;
            m_ShapeBox.size = boxSize;
        }
    }

    [CustomPropertyDrawer(typeof(Cone))]
    public class ConeDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        readonly GUIContent m_Content = new GUIContent("Sides Count");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Cone Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_NumberOfSides"), m_Content);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }

}
