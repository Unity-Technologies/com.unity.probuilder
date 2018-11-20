using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Subdivide a ProBuilder mesh.
    /// </summary>
    static class Subdivision
    {
        /// <summary>
        /// Subdivide all faces on the mesh.
        /// </summary>
        /// <remarks>More accurately, this inserts a vertex at the center of each face and connects each edge at it's center.</remarks>
        /// <param name="pb"></param>
        /// <returns></returns>
        public static ActionResult Subdivide(this ProBuilderMesh pb)
        {
            return pb.Subdivide(pb.facesInternal) != null ? new ActionResult(ActionResult.Status.Success, "Subdivide") : new ActionResult(ActionResult.Status.Failure, "Subdivide Failed");
        }

        /// <summary>
        /// Subdivide a mesh, optionally restricting to the specified faces.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="faces">The faces to be affected by subdivision.</param>
        /// <returns>The faces created as a result of the subdivision.</returns>
        public static Face[] Subdivide(this ProBuilderMesh pb, IList<Face> faces)
        {
            return ConnectElements.Connect(pb, faces);
        }
    }
}
