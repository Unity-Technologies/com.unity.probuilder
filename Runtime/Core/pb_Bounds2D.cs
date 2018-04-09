using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// Internal bounds class.
	/// </summary>
	class pb_Bounds2D
	{
		public Vector2 center = Vector2.zero;
		[SerializeField] private Vector2 _size = Vector2.zero;
		[SerializeField] private Vector2 _extents = Vector2.zero;

		public Vector2 size
		{
			get
			{
				return _size;
			}

			set
			{
				_size = value;

				_extents.x = _size.x * .5f;
				_extents.y = _size.y * .5f;
			}
		}

		public Vector2 extents { get { return _extents; } }

		/**
		 * Returns an array of Vector2[] points for each corner, in the order right to left, top to bottom.
		 * 	0 -- > 1
		 *   	/
		 *    /
		 *	2 -- > 3
		 */
		public Vector2[] corners {
			get {
				return new Vector2[] {
					new Vector2(center.x - extents.x, center.y + extents.y),
					new Vector2(center.x + extents.x, center.y + extents.y),
					new Vector2(center.x - extents.x, center.y - extents.y),
					new Vector2(center.x + extents.x, center.y - extents.y)
				};
			}
		}

		/**
		 * Basic constructor.
		 */
		public pb_Bounds2D()
		{}

		public pb_Bounds2D(Vector2 center, Vector2 size)
		{
			this.center = center;
			this.size = size;
		}

		/**
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(Vector2[] points)
		{
			SetWithPoints(points);
		}

		/**
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(Vector2[] points, int[] indices)
		{
			SetWithPoints(points, indices);
		}

		/// <summary>
		/// Create bounds from a set of 2d points.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="edges"></param>
		public pb_Bounds2D(Vector2[] points, pb_Edge[] edges)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			if(points.Length > 0 && edges.Length > 0)
			{
				xMin = points[edges[0].x].x;
				yMin = points[edges[0].x].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 0; i < edges.Length; i++)
				{
					xMin = Mathf.Min(xMin, points[edges[i].x].x);
					xMin = Mathf.Min(xMin, points[edges[i].y].x);
					yMin = Mathf.Min(yMin, points[edges[i].x].y);
					yMin = Mathf.Min(yMin, points[edges[i].y].y);

					xMax = Mathf.Max(xMax, points[edges[i].x].x);
					xMax = Mathf.Max(xMax, points[edges[i].y].x);
					yMax = Mathf.Max(yMax, points[edges[i].x].y);
					yMax = Mathf.Max(yMax, points[edges[i].y].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector3(xMax-xMin, yMax-yMin);
		}

		/// <summary>
		/// Create bounds from a set of 3d points cast to 2d.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="edges"></param>
		internal pb_Bounds2D(Vector3[] points, pb_Edge[] edges)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			if(points.Length > 0 && edges.Length > 0)
			{
				xMin = points[edges[0].x].x;
				yMin = points[edges[0].x].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 0; i < edges.Length; i++)
				{
					xMin = Mathf.Min(xMin, points[edges[i].x].x);
					xMin = Mathf.Min(xMin, points[edges[i].y].x);
					yMin = Mathf.Min(yMin, points[edges[i].x].y);
					yMin = Mathf.Min(yMin, points[edges[i].y].y);

					xMax = Mathf.Max(xMax, points[edges[i].x].x);
					xMax = Mathf.Max(xMax, points[edges[i].y].x);
					yMax = Mathf.Max(yMax, points[edges[i].x].y);
					yMax = Mathf.Max(yMax, points[edges[i].y].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector3(xMax-xMin, yMax-yMin);
		}

		public pb_Bounds2D(Vector2[] points, int length)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			if(points.Length > 0)
			{
				xMin = points[0].x;
				yMin = points[0].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 1; i < length; i++)
				{
					xMin = Mathf.Min(xMin, points[i].x);
					yMin = Mathf.Min(yMin, points[i].y);

					xMax = Mathf.Max(xMax, points[i].x);
					yMax = Mathf.Max(yMax, points[i].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector3(xMax-xMin, yMax-yMin);
		}

		/**
		 * Returns true if the point is contained within the bounds.  False otherwise.
		 */
		public bool ContainsPoint(Vector2 point)
		{
			return !(	point.x > center.x + extents.x ||
						point.x < center.x - extents.x ||
						point.y > center.y + extents.y ||
						point.y < center.y - extents.y);
		}

		/**
		 * Returns true if any part of the line segment is contained within this bounding box.
		 */
		public bool IntersectsLineSegment(Vector2 lineStart, Vector2 lineEnd)
		{
			if( ContainsPoint(lineStart) || ContainsPoint(lineEnd) )
			{
				return true;
			}
			else
			{
				Vector2[] aabb = corners;
				return( pb_Math.GetLineSegmentIntersect(aabb[0], aabb[1], lineStart, lineEnd) ||
						pb_Math.GetLineSegmentIntersect(aabb[1], aabb[3], lineStart, lineEnd) ||
						pb_Math.GetLineSegmentIntersect(aabb[3], aabb[2], lineStart, lineEnd) ||
						pb_Math.GetLineSegmentIntersect(aabb[2], aabb[0], lineStart, lineEnd) );
			}
		}

		/**
		 * Returns true if bounds overlap.
		 */
		public bool Intersects(pb_Bounds2D bounds)
		{
			Vector2 dist = this.center - bounds.center;
			Vector2 size = this.size + bounds.size;

			return  Mathf.Abs(dist.x) * 2f < size.x &&
					Mathf.Abs(dist.y) * 2f < size.y;
		}

		/**
		 * Returns true if bounds overlaps rect.
		 */
		public bool Intersects(Rect rect)
		{
			Vector2 dist = this.center - rect.center;
			Vector2 size = this.size + rect.size;

			return  Mathf.Abs(dist.x) * 2f < size.x &&
					Mathf.Abs(dist.y) * 2f < size.y;
		}

		/**
		 *	Set this bounds center and size to encapsulate points.
		 */
		public void SetWithPoints(IList<Vector2> points)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			int len = points.Count;

			if(len > 0)
			{
				xMin = points[0].x;
				yMin = points[0].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 1; i < len; i++)
				{
					float x = points[i].x;
					float y = points[i].y;

					if(x < xMin) xMin = x;
					if(x > xMax) xMax = x;

					if(y < yMin) yMin = y;
					if(y > yMax) yMax = y;
				}
			}

			center.x = (xMin+xMax) / 2f;
			center.y = (yMin+yMax) / 2f;

			_size.x = xMax - xMin;
			_size.y = yMax - yMin;

			_extents.x = _size.x * .5f;
			_extents.y = _size.y * .5f;
		}

		/**
		 *	Set this bounds center and size to encapsulate points.
		 */
		public void SetWithPoints(IList<Vector2> points, IList<int> indices)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			if(points.Count > 0 && indices.Count > 0)
			{
				xMin = points[indices[0]].x;
				yMin = points[indices[0]].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 1; i < indices.Count; i++)
				{
					float x = points[indices[i]].x;
					float y = points[indices[i]].y;

					if(x < xMin) xMin = x;
					if(x > xMax) xMax = x;

					if(y < yMin) yMin = y;
					if(y > yMax) yMax = y;
				}
			}

			center.x = (xMin+xMax) / 2f;
			center.y = (yMin+yMax) / 2f;

			_size.x = xMax - xMin;
			_size.y = yMax - yMin;

			_extents.x = _size.x * .5f;
			_extents.y = _size.y * .5f;
		}

		/**
		 * Returns the center of the bounding box of points.  Optional parameter @length limits the
		 * bounds calculations to only the points up to length in array.
		 */
		public static Vector2 Center(Vector2[] points, int length = -1)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			int size = length < 1 ? points.Length : length;

			xMin = points[0].x;
			yMin = points[0].y;
			xMax = xMin;
			yMax = yMin;

			for(int i = 1; i < size; i++)
			{
				float x = points[i].x;
				float y = points[i].y;

				if(x < xMin) xMin = x;
				if(x > xMax) xMax = x;

				if(y < yMin) yMin = y;
				if(y > yMax) yMax = y;
			}

			return new Vector2( (xMin + xMax) / 2f, (yMin + yMax) / 2f );
		}

		/**
		 * Returns the center of the bounding box of points.  Optional parameter @length limits the
		 * bounds calculations to only the points up to length in array.
		 */
		public static Vector2 Center(Vector2[] points, int[] indices)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;

			int size = indices.Length;

			xMin = points[indices[0]].x;
			yMin = points[indices[0]].y;
			xMax = xMin;
			yMax = yMin;

			for(int i = 1; i < size; i++)
			{
				float x = points[indices[i]].x;
				float y = points[indices[i]].y;

				if(x < xMin) xMin = x;
				if(x > xMax) xMax = x;

				if(y < yMin) yMin = y;
				if(y > yMax) yMax = y;
			}

			return new Vector2( (xMin + xMax) / 2f, (yMin + yMax) / 2f );
		}

		public override string ToString()
		{
			return "[cen: " + center + " size: " + size + "]";
		}
	}
}
