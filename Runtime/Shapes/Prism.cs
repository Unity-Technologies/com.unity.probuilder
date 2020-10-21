namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Prism")]
    public class Prism : Shape
    {
        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var baseY = new Vector3(0, size.y / 2f, 0);
            size.y *= 2f;

            Vector3[] template = new Vector3[6]
            {
                Vector3.Scale(new Vector3(-.5f, 0f, -.5f),  size) - baseY,
                Vector3.Scale(new Vector3(.5f, 0f, -.5f),   size) - baseY,
                Vector3.Scale(new Vector3(0f, .5f, -.5f),   size) - baseY,
                Vector3.Scale(new Vector3(-.5f, 0f, .5f),   size) - baseY,
                Vector3.Scale(new Vector3(0.5f, 0f, .5f),   size) - baseY,
                Vector3.Scale(new Vector3(0f, .5f, .5f),    size) - baseY
            };

            Vector3[] v = new Vector3[18]
            {
                template[0],    // 0    front
                template[1],    // 1
                template[2],    // 2

                template[1],    // 3    right side
                template[4],    // 4
                template[2],    // 5
                template[5],    // 6

                template[4],    // 7    back side
                template[3],    // 8
                template[5],    // 9

                template[3],    // 10   left side
                template[0],    // 11
                template[5],    // 12
                template[2],    // 13

                template[0],    // 14   // bottom
                template[1],    // 15
                template[3],    // 16
                template[4]     // 17
            };

            Face[] f = new Face[5]
            {
                new Face(new int[3] {2, 1, 0}),          // x
                new Face(new int[6] {5, 4, 3, 5, 6, 4}), // x
                new Face(new int[3] {9, 8, 7}),
                new Face(new int[6] {12, 11, 10, 12, 13, 11}),
                new Face(new int[6] {14, 15, 16, 15, 17, 16})
            };

            mesh.RebuildWithPositionsAndFaces(v, f);
        }
    }
}
