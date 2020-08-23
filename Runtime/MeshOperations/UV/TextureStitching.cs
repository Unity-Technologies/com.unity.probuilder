using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    static partial class UVEditing
	{
        /// <summary>
        /// Provided two faces, this method will attempt to project @f2 and align its size, rotation, and position to match
        /// the shared edge on f1.  Returns true on success, false otherwise.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool AutoStitch(ProBuilderMesh mesh, Face f1, Face f2, int channel)
        {
            var wings = WingedEdge.GetWingedEdges(mesh, new [] { f1, f2 });

            var sharedEdge = wings.FirstOrDefault(x => x.face == f1 && x.opposite != null && x.opposite.face == f2);

            if (sharedEdge == null)
                return false;

            if (f1.manualUV)
                f2.manualUV = true;

            f1.textureGroup = -1;
            f2.textureGroup = -1;

            Projection.PlanarProject(mesh, f2);

            if (AlignEdges(mesh, f2, sharedEdge.edge.local, sharedEdge.opposite.edge.local, channel))
            {
                if (!f2.manualUV)
                    UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(mesh, new [] { f2 });

                return true;
            }

            return false;
        }

        /// <summary>
        /// move the UVs to where the edges passed meet
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faceToMove"></param>
        /// <param name="edgeToAlignTo"></param>
        /// <param name="edgeToBeAligned"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        static bool AlignEdges(ProBuilderMesh mesh, Face faceToMove, Edge edgeToAlignTo, Edge edgeToBeAligned, int channel)
        {
            Vector2[] uvs = GetUVs(mesh, channel);
            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;

            // Match each edge vertex to the other
            int[] matchX = new int[2] { edgeToAlignTo.a, -1 };
            int[] matchY = new int[2] { edgeToAlignTo.b, -1 };

            int siIndex = mesh.GetSharedVertexHandle(edgeToAlignTo.a);

            if (siIndex < 0)
                return false;

            if (sharedIndexes[siIndex].Contains(edgeToBeAligned.a))
            {
                matchX[1] = edgeToBeAligned.a;
                matchY[1] = edgeToBeAligned.b;
            }
            else
            {
                matchX[1] = edgeToBeAligned.b;
                matchY[1] = edgeToBeAligned.a;
            }

            // scale face 2 to match the edge size of f1
            float dist_e1 = Vector2.Distance(uvs[edgeToAlignTo.a], uvs[edgeToAlignTo.b]);
            float dist_e2 = Vector2.Distance(uvs[edgeToBeAligned.a], uvs[edgeToBeAligned.b]);

            float scale = dist_e1 / dist_e2;

            // doesn't matter what point we scale around because we'll move it in the next step anyways
            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] = uvs[i].ScaleAroundPoint(Vector2.zero, Vector2.one * scale);

            // Figure out where the center of each edge is so that we can move the f2 edge to match f1's origin
            Vector2 f1_center = (uvs[edgeToAlignTo.a] + uvs[edgeToAlignTo.b]) / 2f;
            Vector2 f2_center = (uvs[edgeToBeAligned.a] + uvs[edgeToBeAligned.b]) / 2f;

            Vector2 diff = f1_center - f2_center;

            // Move f2 face to where it's matching edge center is on top of f1's center
            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] += diff;

            // Now that the edge's centers are matching, rotate f2 to match f1's angle
            Vector2 angle1 = uvs[matchY[0]] - uvs[matchX[0]];
            Vector2 angle2 = uvs[matchY[1]] - uvs[matchX[1]];

            float angle = Vector2.Angle(angle1, angle2);
            if (Vector3.Cross(angle1, angle2).z < 0)
                angle = 360f - angle;

            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, angle);

            float error = Mathf.Abs(Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]])) + Mathf.Abs(Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]));

            // now check that the matched UVs are on top of one another if the error allowance is greater than some small value
            if (error > .02f)
            {
                // first try rotating 180 degrees
                foreach (int i in faceToMove.distinctIndexesInternal)
                    uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, 180f);

                float e2 = Mathf.Abs(Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]])) + Mathf.Abs(Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]));
                if (e2 < error)
                    error = e2;
                else
                {
                    // flip 'em back around
                    foreach (int i in faceToMove.distinctIndexesInternal)
                        uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, 180f);
                }
            }

            // If successfully aligned, merge the sharedIndexesUV
            SplitUVs(mesh, faceToMove.distinctIndexesInternal);

            mesh.SetTexturesCoincident(matchX);
            mesh.SetTexturesCoincident(matchY);
            ApplyUVs(mesh, uvs, channel);

            return true;
        }
	}
}
