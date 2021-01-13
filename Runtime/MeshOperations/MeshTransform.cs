namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Functions for manipulating the transform of a mesh.
    /// </summary>
    public static class MeshTransform
    {
        internal static void SetPivot(this ProBuilderMesh mesh, PivotLocation pivotType, Vector3? pivotPosition = null)
        {
            if(mesh.vertexCount == 0)
                return;

            switch (pivotType)
            {
                case PivotLocation.Center:
                    mesh.CenterPivot(null);
                    break;

                case PivotLocation.FirstCorner:
                    mesh.SetPivot(pivotPosition == null ? Vector3.zero : (Vector3)pivotPosition);
                    break;
            }
        }

        /// <summary>
        /// Center the mesh pivot at the average of a set of vertices.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="indexes">The indexes of the positions to average to find the new pivot.</param>
        public static void CenterPivot(this ProBuilderMesh mesh, int[] indexes)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            Vector3 center = Vector3.zero;

            if (indexes != null && indexes.Length > 0)
            {
                Vector3[] positions = mesh.positionsInternal;

                if (positions == null || positions.Length < 3)
                    return;

                foreach (int i in indexes)
                    center += positions[i];

                center = mesh.transform.TransformPoint(center / (float)indexes.Length);
            }
            else
            {
                center = mesh.transform.TransformPoint(mesh.mesh.bounds.center);
            }

            Vector3 dir = (mesh.transform.position - center);

            mesh.transform.position = center;

            mesh.ToMesh();
            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, dir);
            mesh.Refresh();
        }

        /// <summary>
        /// Set the pivot point of a mesh in world space. The Transform component position property is set to worldPosition, while the mesh geometry does not move.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="worldPosition">The new pivot position in world space.</param>
        public static void SetPivot(this ProBuilderMesh mesh, Vector3 worldPosition)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            Vector3 offset = mesh.transform.position - worldPosition;
            mesh.transform.position = worldPosition;
            mesh.ToMesh();
            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, offset);
            mesh.Refresh();
        }

        /// <summary>
        /// Scale vertices and set transform.localScale to Vector3.one.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        public static void FreezeScaleTransform(this ProBuilderMesh mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            Vector3[] v = mesh.positionsInternal;

            for (var i = 0; i < v.Length; i++)
                v[i] = Vector3.Scale(v[i], mesh.transform.localScale);

            mesh.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
