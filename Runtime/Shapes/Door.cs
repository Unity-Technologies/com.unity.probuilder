using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [door](../manual/Door.html) shape.
    /// </summary>
    [Shape("Door")]
    public class Door : Shape
    {
        /// <summary>
        /// Sets the height of the top of the door frame in meters.
        /// The default value is 0.5. The minimum value is 0.01.
        /// </summary>
        [Min(0.01f)]
        [SerializeField]
        float m_DoorHeight = .5f;

        /// <summary>
        /// Sets the width of the door frame on the sides in meters.
        /// The default value is 0.75. The minimum value is 0.01.
        /// </summary>
        [Min(0.01f)]
        [SerializeField]
        float m_LegWidth = .75f;

        internal override void SetParametersToBuiltInShape()
        {
            m_DoorHeight = 0.5f;
            m_LegWidth = 0.75f;
        }
        
        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Door)
            {
                m_DoorHeight = ( (Door) shape ).m_DoorHeight;
                m_LegWidth = ( (Door) shape ).m_LegWidth;
            }
        }

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var upDir = Vector3.Scale(rotation * Vector3.up, size) ;
            var rightDir = Vector3.Scale(rotation * Vector3.right, size) ;
            var forwardDir = Vector3.Scale(rotation * Vector3.forward, size) ;

            float totalWidth = rightDir.magnitude;
            float totalHeight = upDir.magnitude;
            float depth = forwardDir.magnitude;

            float xLegCoord = totalWidth / 2f;
            var legWidth = xLegCoord - m_LegWidth > 0 ? xLegCoord - m_LegWidth : 0.001f;
            var ledgeHeight = (totalHeight - m_DoorHeight * 2f) > 0 ? totalHeight - m_DoorHeight * 2f : 0.001f;

            var baseY = -totalHeight;
            var front = depth / 2f;
            // 8---9---10--11
            // |           |
            // 4   5---6   7
            // |   |   |   |
            // 0   1   2   3
            Vector3[] template = new Vector3[12]
            {
                new Vector3(-xLegCoord, baseY, front),           // 0
                new Vector3(-legWidth, baseY, front),            // 1
                new Vector3(legWidth, baseY, front),             // 2
                new Vector3(xLegCoord, baseY, front),            // 3
                new Vector3(-xLegCoord, ledgeHeight, front),  // 4
                new Vector3(-legWidth, ledgeHeight, front),   // 5
                new Vector3(legWidth, ledgeHeight, front),    // 6
                new Vector3(xLegCoord, ledgeHeight, front),   // 7
                new Vector3(-xLegCoord, totalHeight, front),  // 8
                new Vector3(-legWidth, totalHeight, front),   // 9
                new Vector3(legWidth, totalHeight, front),    // 10
                new Vector3(xLegCoord, totalHeight, front)    // 11
            };

            List<Vector3> points = new List<Vector3>();

            points.Add(template[4]);
            points.Add(template[0]);
            points.Add(template[5]);
            points.Add(template[1]);

            points.Add(template[2]);
            points.Add(template[3]);
            points.Add(template[6]);
            points.Add(template[7]);

            points.Add(template[4]);
            points.Add(template[5]);
            points.Add(template[8]);
            points.Add(template[9]);

            points.Add(template[10]);
            points.Add(template[6]);
            points.Add(template[11]);
            points.Add(template[7]);

            points.Add(template[5]);
            points.Add(template[6]);
            points.Add(template[9]);
            points.Add(template[10]);

            List<Vector3> reverse = new List<Vector3>();

            for (int i = 0; i < points.Count; i += 4)
            {
                reverse.Add(points[i + 0] - Vector3.forward * depth);
                reverse.Add(points[i + 2] - Vector3.forward * depth);
                reverse.Add(points[i + 1] - Vector3.forward * depth);
                reverse.Add(points[i + 3] - Vector3.forward * depth);
            }

            points.AddRange(reverse);

            points.Add(template[6]);
            points.Add(template[5]);
            points.Add(template[6] - Vector3.forward * depth);
            points.Add(template[5] - Vector3.forward * depth);

            points.Add(template[2] - Vector3.forward * depth);
            points.Add(template[2]);
            points.Add(template[6] - Vector3.forward * depth);
            points.Add(template[6]);

            points.Add(template[1]);
            points.Add(template[1] - Vector3.forward * depth);
            points.Add(template[5]);
            points.Add(template[5] - Vector3.forward * depth);

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < points.Count; i++)
                 points[i] = Vector3.Scale(rotation * points[i], sizeSigns);

            mesh.GeometryWithPoints(points.ToArray());

            var sizeSign = sizeSigns.x * sizeSigns.y * sizeSigns.z;
            if(sizeSign < 0)
            {
                var faces = mesh.facesInternal;
                foreach(var face in faces)
                    face.Reverse();
            }

            return mesh.mesh.bounds;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Door))]
    public class DoorDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_HeightContent = new GUIContent("Pediment Height", L10n.Tr("Set the height of the door's top."));
        static readonly GUIContent k_SideContent = new GUIContent("Side Width", L10n.Tr("Set the width of the door's sides."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Door Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_DoorHeight"), k_HeightContent);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_LegWidth"), k_SideContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
