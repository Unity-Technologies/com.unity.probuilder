using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// The results of a raycast hit.
    /// </summary>
    sealed class RaycastHit
    {
        public float distance;
        public Vector3 point;
        public Vector3 normal;
        public int face;

        public RaycastHit(
            float distance,
            Vector3 point,
            Vector3 normal,
            int face)
        {
            this.distance = distance;
            this.point = point;
            this.normal = normal;
            this.face = face;
        }
    }
}
