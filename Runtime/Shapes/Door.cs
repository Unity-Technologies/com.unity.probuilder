#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    public class Door : Shape
    {
        [Min(0.01f)]
        [SerializeField]
        float m_DoorHeight = .5f;

        [Min(0.01f)]
        [SerializeField]
        float m_LegWidth = .75f;

        public override void RebuildMesh(ProBuilderMesh mesh)
        {
#if UNITY_EDITOR
            EditorPrefs.SetFloat("ShapeBuilder.Door.m_AxisDivisions", m_DoorHeight);
            EditorPrefs.SetFloat("ShapeBuilder.Door.m_HeightCuts", m_LegWidth);
#endif

            float totalWidth = m_Size.x;
            float totalHeight = m_Size.y;
            float depth = m_Size.z;

            float xLegCoord = totalWidth / 2f;
            var legWidth = xLegCoord - this.m_LegWidth;
            var ledgeHeight = totalHeight - m_DoorHeight;

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

            points.Add(template[0]);
            points.Add(template[1]);
            points.Add(template[4]);
            points.Add(template[5]);

            points.Add(template[2]);
            points.Add(template[3]);
            points.Add(template[6]);
            points.Add(template[7]);

            points.Add(template[4]);
            points.Add(template[5]);
            points.Add(template[8]);
            points.Add(template[9]);

            points.Add(template[6]);
            points.Add(template[7]);
            points.Add(template[10]);
            points.Add(template[11]);

            points.Add(template[5]);
            points.Add(template[6]);
            points.Add(template[9]);
            points.Add(template[10]);

            List<Vector3> reverse = new List<Vector3>();

            for (int i = 0; i < points.Count; i += 4)
            {
                reverse.Add(points[i + 1] - Vector3.forward * depth);
                reverse.Add(points[i + 0] - Vector3.forward * depth);
                reverse.Add(points[i + 3] - Vector3.forward * depth);
                reverse.Add(points[i + 2] - Vector3.forward * depth);
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


            mesh.GeometryWithPoints(points.ToArray());
        }
    }
}
