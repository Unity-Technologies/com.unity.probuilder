using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    public class Arch : Shape
    {
        [Min(0.01f)]
        [SerializeField]
        float thickness = .1f;

        [Range(3, 200)]
        [SerializeField]
        int numberOfSides = 6;

        [Range(0, 360)]
        [SerializeField]
        float archDegrees = 180;

        [SerializeField]
        bool endCaps = true;

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var radialCuts = numberOfSides;
            var angle = archDegrees;
            var width = thickness;
            var radius = size.y;
            var depth = size.z;
            Vector2[] templateOut = new Vector2[radialCuts];
            Vector2[] templateIn = new Vector2[radialCuts];

            for (int i = 0; i < radialCuts; i++)
            {
                templateOut[i] = Math.PointInCircumference(radius, i * (angle / (radialCuts - 1)), Vector2.zero) + new Vector2(0, -radius/2f);
                templateIn[i] = Math.PointInCircumference(radius - width, i * (angle / (radialCuts - 1)), Vector2.zero) + new Vector2(0, -radius/2f);
            }

            List<Vector3> v = new List<Vector3>();

            Vector2 tmp, tmp2, tmp3, tmp4;

            float y = -depth;

            for (int n = 0; n < radialCuts - 1; n++)
            {
                // outside faces
                tmp = templateOut[n];
                tmp2 = n < (radialCuts - 1) ? templateOut[n + 1] : templateOut[n];

                Vector3[] qvo = new Vector3[4]
                {
                    new Vector3(tmp.x, tmp.y, y),
                    new Vector3(tmp2.x, tmp2.y, y),
                    new Vector3(tmp.x, tmp.y, depth),
                    new Vector3(tmp2.x, tmp2.y, depth)
                };

                // inside faces
                tmp = templateIn[n];
                tmp2 = n < (radialCuts - 1) ? templateIn[n + 1] : templateIn[n];

                Vector3[] qvi = new Vector3[4]
                {
                    new Vector3(tmp2.x, tmp2.y, y),
                    new Vector3(tmp.x, tmp.y, y),
                    new Vector3(tmp2.x, tmp2.y, depth),
                    new Vector3(tmp.x, tmp.y, depth)
                };

                v.AddRange(qvo);

                if (n != radialCuts - 1)
                    v.AddRange(qvi);

                // left side bottom face
                if (angle < 360f && endCaps)
                {
                    if (n == 0)
                    {
                        v.AddRange(
                            new Vector3[4]
                        {
                            new Vector3(templateOut[n].x, templateOut[n].y, depth),
                            new Vector3(templateIn[n].x, templateIn[n].y, depth),
                            new Vector3(templateOut[n].x, templateOut[n].y, y),
                            new Vector3(templateIn[n].x, templateIn[n].y, y)
                        });
                    }

                    // ride side bottom face
                    if (n == radialCuts - 2)
                    {
                        v.AddRange(
                            new Vector3[4]
                        {
                            new Vector3(templateIn[n + 1].x, templateIn[n + 1].y, depth),
                            new Vector3(templateOut[n + 1].x, templateOut[n + 1].y, depth),
                            new Vector3(templateIn[n + 1].x, templateIn[n + 1].y, y),
                            new Vector3(templateOut[n + 1].x, templateOut[n + 1].y, y)
                        });
                    }
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

            mesh.GeometryWithPoints(v.ToArray());
        }
    }
}
