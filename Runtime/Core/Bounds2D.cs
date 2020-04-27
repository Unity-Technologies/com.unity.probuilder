using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Internal bounds class.
    /// </summary>
    sealed class Bounds2D
    {
        public Vector2 center = Vector2.zero;
        [SerializeField] Vector2 m_Size = Vector2.zero;
        [SerializeField] Vector2 m_Extents = Vector2.zero;

        public Vector2 size
        {
            get { return m_Size; }

            set
            {
                m_Size = value;

                m_Extents.x = m_Size.x * .5f;
                m_Extents.y = m_Size.y * .5f;
            }
        }

        public Vector2 extents
        {
            get { return m_Extents; }
        }

        /// <summary>
        /// Returns an array of Vector2[] points for each corner, in the order right to left, top to bottom.
        /// </summary>
        public Vector2[] corners
        {
            get
            {
                return new Vector2[] {
                    new Vector2(center.x - extents.x, center.y + extents.y),
                    new Vector2(center.x + extents.x, center.y + extents.y),
                    new Vector2(center.x - extents.x, center.y - extents.y),
                    new Vector2(center.x + extents.x, center.y - extents.y)
                };
            }
        }

        public Bounds2D()
        {}

        public Bounds2D(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = size;
        }

        /// <summary>
        /// Create bounds from a set of 2d points.
        /// </summary>
        /// <param name="points"></param>
        public Bounds2D(IList<Vector2> points)
        {
            SetWithPoints(points);
        }

        /// <summary>
        /// Create bounds from a set of 2d points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="indexes"></param>
        public Bounds2D(IList<Vector2> points, IList<int> indexes)
        {
            SetWithPoints(points, indexes);
        }

        /// <summary>
        /// Create bounds from a set of 3d points cast to 2d.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="edges"></param>
        internal Bounds2D(Vector3[] points, Edge[] edges)
        {
            float   xMin = 0f,
                                    xMax = 0f,
                                    yMin = 0f,
                                    yMax = 0f;

            if (points.Length > 0 && edges.Length > 0)
            {
                xMin = points[edges[0].a].x;
                yMin = points[edges[0].a].y;
                xMax = xMin;
                yMax = yMin;

                for (int i = 0; i < edges.Length; i++)
                {
                    xMin = Mathf.Min(xMin, points[edges[i].a].x);
                    xMin = Mathf.Min(xMin, points[edges[i].b].x);
                    yMin = Mathf.Min(yMin, points[edges[i].a].y);
                    yMin = Mathf.Min(yMin, points[edges[i].b].y);

                    xMax = Mathf.Max(xMax, points[edges[i].a].x);
                    xMax = Mathf.Max(xMax, points[edges[i].b].x);
                    yMax = Mathf.Max(yMax, points[edges[i].a].y);
                    yMax = Mathf.Max(yMax, points[edges[i].b].y);
                }
            }

            this.center = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
            this.size = new Vector3(xMax - xMin, yMax - yMin);
        }

        public Bounds2D(Vector2[] points, int length)
        {
            float   xMin = 0f,
                                  xMax = 0f,
                                  yMin = 0f,
                                  yMax = 0f;

            if (points.Length > 0)
            {
                xMin = points[0].x;
                yMin = points[0].y;
                xMax = xMin;
                yMax = yMin;

                for (int i = 1; i < length; i++)
                {
                    xMin = Mathf.Min(xMin, points[i].x);
                    yMin = Mathf.Min(yMin, points[i].y);

                    xMax = Mathf.Max(xMax, points[i].x);
                    yMax = Mathf.Max(yMax, points[i].y);
                }
            }

            this.center = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
            this.size = new Vector3(xMax - xMin, yMax - yMin);
        }

        /// <summary>
        /// Returns true if the point is contained within the bounds.  False otherwise.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point)
        {
            return !(point.x > center.x + extents.x ||
                     point.x < center.x - extents.x ||
                     point.y > center.y + extents.y ||
                     point.y < center.y - extents.y);
        }

        /// <summary>
        /// Returns true if any part of the line segment is contained within this bounding box.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public bool IntersectsLineSegment(Vector2 lineStart, Vector2 lineEnd)
        {
            if (ContainsPoint(lineStart) || ContainsPoint(lineEnd))
            {
                return true;
            }
            else
            {
                Vector2[] aabb = corners;
                return (Math.GetLineSegmentIntersect(aabb[0], aabb[1], lineStart, lineEnd) ||
                        Math.GetLineSegmentIntersect(aabb[1], aabb[3], lineStart, lineEnd) ||
                        Math.GetLineSegmentIntersect(aabb[3], aabb[2], lineStart, lineEnd) ||
                        Math.GetLineSegmentIntersect(aabb[2], aabb[0], lineStart, lineEnd));
            }
        }

        /// <summary>
        /// Returns true if bounds overlap.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public bool Intersects(Bounds2D bounds)
        {
            Vector2 dist = this.center - bounds.center;
            Vector2 size = this.size + bounds.size;

            return Mathf.Abs(dist.x) * 2f < size.x &&
                Mathf.Abs(dist.y) * 2f < size.y;
        }

        /// <summary>
        /// Check if this rect is intersected by another.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>True if bounds overlaps rect.</returns>
        public bool Intersects(Rect rect)
        {
            Vector2 dist = this.center - rect.center;
            Vector2 size = this.size + rect.size;

            return Mathf.Abs(dist.x) * 2f < size.x &&
                Mathf.Abs(dist.y) * 2f < size.y;
        }

        /// <summary>
        /// Set this bounds center and size to encapsulate points.
        /// </summary>
        /// <param name="points"></param>
        public void SetWithPoints(IList<Vector2> points)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            int len = points.Count;

            if (len > 0)
            {
                xMin = points[0].x;
                yMin = points[0].y;
                xMax = xMin;
                yMax = yMin;

                for (int i = 1; i < len; i++)
                {
                    float x = points[i].x;
                    float y = points[i].y;

                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;

                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }

            center.x = (xMin + xMax) / 2f;
            center.y = (yMin + yMax) / 2f;

            m_Size.x = xMax - xMin;
            m_Size.y = yMax - yMin;

            m_Extents.x = m_Size.x * .5f;
            m_Extents.y = m_Size.y * .5f;
        }

        /// <summary>
        /// Set this bounds center and size to encapsulate points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="indexes"></param>
        public void SetWithPoints(IList<Vector2> points, IList<int> indexes)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            if (points.Count > 0 && indexes.Count > 0)
            {
                xMin = points[indexes[0]].x;
                yMin = points[indexes[0]].y;
                xMax = xMin;
                yMax = yMin;

                for (int i = 1; i < indexes.Count; i++)
                {
                    float x = points[indexes[i]].x;
                    float y = points[indexes[i]].y;

                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;

                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }

            center.x = (xMin + xMax) / 2f;
            center.y = (yMin + yMax) / 2f;

            m_Size.x = xMax - xMin;
            m_Size.y = yMax - yMin;

            m_Extents.x = m_Size.x * .5f;
            m_Extents.y = m_Size.y * .5f;
        }

        /// <summary>
        /// Returns the center of the bounding box of points. Optional parameter @length limits the bounds calculations
        /// to only the points up to length in array.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Vector2 Center(IList<Vector2> points)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            int size = points.Count;

            xMin = points[0].x;
            yMin = points[0].y;
            xMax = xMin;
            yMax = yMin;

            for (int i = 1; i < size; i++)
            {
                float x = points[i].x;
                float y = points[i].y;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;

                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            return new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
        }

        public static Vector2 Center(IList<Vector2> points, IList<int> indexes)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            int size = indexes.Count;

            xMin = points[indexes[0]].x;
            yMin = points[indexes[0]].y;
            xMax = xMin;
            yMax = yMin;

            for (int i = 1; i < size; i++)
            {
                float x = points[indexes[i]].x;
                float y = points[indexes[i]].y;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;

                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            return new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
        }

        public static Vector2 Size(IList<Vector2> points, IList<int> indexes)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            int size = indexes.Count;

            xMin = points[indexes[0]].x;
            yMin = points[indexes[0]].y;
            xMax = xMin;
            yMax = yMin;

            for (int i = 1; i < size; i++)
            {
                float x = points[indexes[i]].x;
                float y = points[indexes[i]].y;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;

                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            return new Vector2(xMax - xMin, yMax - yMin);
        }

        internal static Vector2 Center(IList<Vector4> points, IEnumerable<int> indexes)
        {
            float   xMin = 0f,
                    xMax = 0f,
                    yMin = 0f,
                    yMax = 0f;

            if (indexes.Any())
            {
                var first = indexes.First();

                xMin = points[first].x;
                yMin = points[first].y;
                xMax = xMin;
                yMax = yMin;

                foreach (var index in indexes)
                {
                    float x = points[index].x;
                    float y = points[index].y;

                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;

                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }
            return new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
        }

        public override string ToString()
        {
            return "[cen: " + center + " size: " + size + "]";
        }
    }
}
