using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;
using System.Linq;

namespace ProBuilder2.Common
{
	public class pb_Bounds2D
	{

#region Members

		public Vector2 center;
		[SerializeField] private Vector2 _size;
		public Vector2 size { get { return _size; } set {  _size = value; extents = value/2f; } }
		public Vector2 extents { get; private set; }

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
#endregion

#region Constructor

		/**
		 * Basic constructor.
		 */
		public pb_Bounds2D(Vector2 center, Vector2 size)
		{
			this.center = center;
			this.size = size;
		}

		/**
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(IList<Vector2> points, IList<int> indices)
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
					xMin = Mathf.Min(xMin, points[indices[i]].x);
					yMin = Mathf.Min(yMin, points[indices[i]].y);

					xMax = Mathf.Max(xMax, points[indices[i]].x);
					yMax = Mathf.Max(yMax, points[indices[i]].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector2(xMax-xMin, yMax-yMin);
		}

		/**
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(IList<Vector4> points, IList<int> indices)
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
					xMin = Mathf.Min(xMin, points[indices[i]].x);
					yMin = Mathf.Min(yMin, points[indices[i]].y);

					xMax = Mathf.Max(xMax, points[indices[i]].x);
					yMax = Mathf.Max(yMax, points[indices[i]].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector2(xMax-xMin, yMax-yMin);
		}

		/**
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(IList<Vector2> points, int length = -1)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;
		
			if(points.Count > 0)
			{
				xMin = points[0].x;
				yMin = points[0].y;
				xMax = xMin;
				yMax = yMin;

				int c = length > -1 ? length : points.Count;

				for(int i = 1; i < c; i++)
				{
					xMin = Mathf.Min(xMin, points[i].x);
					yMin = Mathf.Min(yMin, points[i].y);

					xMax = Mathf.Max(xMax, points[i].x);
					yMax = Mathf.Max(yMax, points[i].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector2(xMax-xMin, yMax-yMin);
		}

		/**
		 * Create bounds from a set of 4d points (uses only x,y)
		 */
		public pb_Bounds2D(IList<Vector4> points, int length = -1)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;
		
			if(points.Count > 0)
			{
				xMin = points[0].x;
				yMin = points[0].y;
				xMax = xMin;
				yMax = yMin;

				int c = length > -1 ? length : points.Count;

				for(int i = 1; i < c; i++)
				{
					xMin = Mathf.Min(xMin, points[i].x);
					yMin = Mathf.Min(yMin, points[i].y);

					xMax = Mathf.Max(xMax, points[i].x);
					yMax = Mathf.Max(yMax, points[i].y);
				}
			}

			this.center = new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
			this.size = new Vector2(xMax-xMin, yMax-yMin);
		}
#endregion

#region Public Methods

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
#endregion

#region Static

		/**
		 * Returns the center of the bounding box of points.  Optional parameter @length limits the 
		 * bounds calculations to only the points up to length in array.
		 */
		[System.Obsolete("warning slow cast")]
		public static Vector2 Center(IList<Vector2> points, int length = -1)
		{
			return (Vector2) Center(points.Cast<Vector4>().ToList(), length);
		}

		public static Vector2 Center(IList<Vector4> points, int length = -1)
		{
			if(length < 0)
				length = points.Count;
				
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;
		
			if(points.Count > 0)
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

			return new Vector4( (xMin+xMax)/2f, (yMin+yMax)/2f );
		}
#endregion

#region Override

		public override string ToString()
		{
			return "[cen: " + center + " size: " + size + "]";
		}
#endregion
	}
}
