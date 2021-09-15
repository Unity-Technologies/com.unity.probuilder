namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Defines the location of the pivot point on a ProBuilderMesh object.
    /// </summary>
    public enum PivotPoint
    {
        /// <summary>
        /// Transforms are applied from the center point of the selection bounding box.
        /// Corresponds with <see cref="UnityEditor.PivotMode.Center"/>.
        /// </summary>
        Center,

        /// <summary>
        /// Transforms are applied from the origin of each selection group.
        /// </summary>
        IndividualOrigins,

        /// <summary>
        /// Transforms are applied from the active selection center.
        /// </summary>
        ActiveElement,

//      /// <summary>
//      /// Transforms are applied from a user-defined pivot point.
//      /// </summary>
//      Custom
    }
}
