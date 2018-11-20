using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// How bezier handles behave when being manipulated in the scene view.
    /// </summary>
    enum BezierTangentMode
    {
        Free,
        Aligned,
        Mirrored
    }

    enum BezierTangentDirection
    {
        In,
        Out
    }

    /// <summary>
    /// A bezier knot.
    /// </summary>
    [System.Serializable]
    struct BezierPoint
    {
        public Vector3 position;
        public Vector3 tangentIn;
        public Vector3 tangentOut;
        public Quaternion rotation;

        public BezierPoint(Vector3 position, Vector3 tangentIn, Vector3 tangentOut, Quaternion rotation)
        {
            this.position = position;
            this.tangentIn = tangentIn;
            this.tangentOut = tangentOut;
            this.rotation = rotation;
        }

        public void EnforceTangentMode(BezierTangentDirection master, BezierTangentMode mode)
        {
            if (mode == BezierTangentMode.Aligned)
            {
                if (master == BezierTangentDirection.In)
                    tangentOut = position + (tangentOut - position).normalized * (tangentIn - position).magnitude;
                else
                    tangentIn = position + (tangentIn - position).normalized * (tangentOut - position).magnitude;
            }
            else if (mode == BezierTangentMode.Mirrored)
            {
                if (master == BezierTangentDirection.In)
                    tangentOut = position - (tangentIn - position);
                else
                    tangentIn = position - (tangentOut - position);
            }
        }

        /// <summary>
        /// Set the position while also moving tangent points.
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            Vector3 delta = position - this.position;
            this.position = position;
            this.tangentIn += delta;
            this.tangentOut += delta;
        }

        public void SetTangentIn(Vector3 tangent, BezierTangentMode mode)
        {
            this.tangentIn = tangent;
            EnforceTangentMode(BezierTangentDirection.In, mode);
        }

        public void SetTangentOut(Vector3 tangent, BezierTangentMode mode)
        {
            this.tangentOut = tangent;
            EnforceTangentMode(BezierTangentDirection.Out, mode);
        }

        public static Vector3 QuadraticPosition(BezierPoint a, BezierPoint b, float t)
        {
            float x = (1f - t) * (1f - t) * a.position.x + 2f * (1f - t) * t * a.tangentOut.x + t * t * b.position.x;
            float y = (1f - t) * (1f - t) * a.position.y + 2f * (1f - t) * t * a.tangentOut.y + t * t * b.position.y;
            float z = (1f - t) * (1f - t) * a.position.z + 2f * (1f - t) * t * a.tangentOut.z + t * t * b.position.z;
            return new Vector3(x, y, z);
        }

        public static Vector3 CubicPosition(BezierPoint a, BezierPoint b, float t)
        {
            t = Mathf.Clamp01(t);

            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * a.position +
                3f * oneMinusT * oneMinusT * t * a.tangentOut +
                3f * oneMinusT * t * t * b.tangentIn +
                t * t * t * b.position;
        }

        public static Vector3 GetLookDirection(IList<BezierPoint> points, int index, int previous, int next)
        {
            if (previous < 0)
            {
                return (points[index].position - QuadraticPosition(points[index], points[next], .1f)).normalized;
            }
            else if (next < 0)
            {
                return (QuadraticPosition(points[index], points[previous], .1f) - points[index].position).normalized;
            }
            else if (next > -1 && previous > -1)
            {
                Vector3 a = (QuadraticPosition(points[index], points[previous], .1f) - points[index].position).normalized;
                Vector3 b = (QuadraticPosition(points[index], points[next], .1f) - points[index].position).normalized;
                return ((a + b) * .5f).normalized;
            }
            else
            {
                return Vector3.forward;
            }
        }
    }
}
