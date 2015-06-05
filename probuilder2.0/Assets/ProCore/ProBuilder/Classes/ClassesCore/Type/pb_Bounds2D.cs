using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;

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
		public pb_Bounds2D(Vector2[] points)
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

				for(int i = 1; i < points.Length; i++)
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
		 * Create bounds from a set of 2d points.
		 */
		public pb_Bounds2D(Vector2[] points, int[] indices)
		{
			float 	xMin = 0f,
					xMax = 0f,
					yMin = 0f,
					yMax = 0f;
		
			if(points.Length > 0 && indices.Length > 0)
			{
				xMin = points[indices[0]].x;
				yMin = points[indices[0]].y;
				xMax = xMin;
				yMax = yMin;

				for(int i = 1; i < indices.Length; i++)
				{
					xMin = Mathf.Min(xMin, points[indices[i]].x);
					yMin = Mathf.Min(yMin, points[indices[i]].y);

					xMax = Mathf.Max(xMax, points[indices[i]].x);
					yMax = Mathf.Max(yMax, points[indices[i]].y);
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
		public static Vector2 Center(List<Vector2> points) { return Center(points.ToArray()); }
		public static Vector2 Center(Vector2[] points) { return Center(points, points.Length); }
		public static Vector2 Center(Vector2[] points, int length)
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

			return new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f );
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