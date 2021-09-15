namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Functions for manipulating the transform of a mesh.
    /// </summary>
    public static class MeshTransform
    {
        /// <summary>
        /// Set the pivot point for a mesh to either the center, or a corner point of the bounding box.
        /// </summary>
        /// <param name="mesh">The <see cref="ProBuilderMesh"/> to adjust vertices for a new pivot point.</param>
        /// <param name="pivotLocation">The new pivot point is either the center of the mesh bounding box, or
        /// the bounds center - extents.</param>
        internal static void SetPivot(this ProBuilderMesh mesh, PivotLocation pivotLocation)
        {
            var bounds = mesh.GetBounds();
            var pivot = pivotLocation == PivotLocation.Center ? bounds.center : bounds.center - bounds.extents;
            SetPivot(mesh, mesh.transform.TransformPoint(pivot));
        }

        /// <summary>
        /// Centers the mesh pivot at the average of a set of vertex positions.
        ///
        /// This is the equivalent of the [Center Pivot](../manual/CenterPivot.html) action.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="indexes">The array of indices representing the positions to average in order to find the new pivot.</param>
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
        /// Sets the pivot point of a mesh in world space. The Transform component position property is set to worldPosition,
        /// while the mesh geometry does not move.
        ///
        /// This is the equivalent of the [Set Pivot (Vertices)](../manual/Vert_SetPivot.html) action.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="worldPosition">The new pivot position in world space.</param>
        public static void SetPivot(this ProBuilderMesh mesh, Vector3 worldPosition)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            var transform = mesh.transform;
            Vector3 offset = transform.position - worldPosition;
            transform.position = worldPosition;

            mesh.ToMesh();
            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, offset);
            mesh.Refresh();
        }

        /// <summary>
        /// Scales vertices and sets `transform.localScale` to `Vector3.one`.
        ///
        /// This is the equivalent of the [Freeze Transform](../manual/Freeze_Transform.html) action.
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
