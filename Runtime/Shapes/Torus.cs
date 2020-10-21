using System.Collections.Generic;

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
        float m_InnerRadius = 30;

        [Range(0, 360)]
        [SerializeField]
        float m_HorizontalCircumference = 360;

        [Range(0, 360)]
        [SerializeField]
        float m_VerticalCircumference = 360;

        [SerializeField]
        bool m_Smooth = true;

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var outerRadius = System.Math.Min(size.x, size.z);
            int clampedRows = (int)Mathf.Clamp(m_Rows + 1, 4, 128);
            int clampedColumns = (int)Mathf.Clamp(m_Columns + 1, 4, 128);
            float clampedRadius = Mathf.Clamp(m_InnerRadius, .01f, 2048f);
            float clampedTubeRadius = Mathf.Clamp(outerRadius, .01f, clampedRadius - .001f);
            clampedRadius -= clampedTubeRadius;
            float clampedHorizontalCircumference = Mathf.Clamp(m_HorizontalCircumference, .01f, 360f);
            float clampedVerticalCircumference = Mathf.Clamp(m_VerticalCircumference, .01f, 360f);

            List<Vector3> vertices = new List<Vector3>();

            int col = clampedColumns - 1;

            Vector3[] cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, Quaternion.Euler(Vector3.up * 0f * clampedHorizontalCircumference), clampedRadius);

            for (int i = 1; i < clampedColumns; i++)
            {
                vertices.AddRange(cir);
                Quaternion rotation = Quaternion.Euler(Vector3.up * ((i / (float)col) * clampedHorizontalCircumference));
                cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, rotation, clampedRadius);
                vertices.AddRange(cir);
            }

            // List<int> ind = new List<int>();
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

            mesh.RebuildWithPositionsAndFaces(vertices, faces);
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

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 2; i < segments; i++)
            {
                float rad = ((i / seg) * circumference) * Mathf.Deg2Rad;
                sb.AppendLine(rad.ToString());

                v[n + 0] = v[n - 1];
                v[n + 1] = rotation * (new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f) + Vector3.right * offset);

                n += 2;
            }

            return v;
        }
    }
}
