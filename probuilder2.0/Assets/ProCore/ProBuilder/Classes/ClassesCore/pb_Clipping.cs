using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Rect to line segment clipping implemention.
	/// </summary>
	/// <remarks>
	/// https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
	/// </remarks>
	static class pb_Clipping
	{
		[System.Flags]
		enum OutCode
		{
			INSIDE = 0, // 0000
			LEFT = 1, 	// 0001
			RIGHT = 2, 	// 0010
			BOTTOM = 4,	// 0100
			TOP = 8, 	// 1000
		}

		// Compute the bit code for a point (x, y) using the clip rectangle
		// bounded diagonally by (xmin, ymin), and (xmax, ymax)

		static OutCode ComputeOutCode(Rect rect, float x, float y)
		{
			OutCode code = OutCode.INSIDE; // initialised as being inside of [[clip window]]

			if (x < rect.xMin) // to the left of clip window
				code |= OutCode.LEFT;
			else if (x > rect.xMax) // to the right of clip window
				code |= OutCode.RIGHT;
			if (y < rect.yMin) // below the clip window
				code |= OutCode.BOTTOM;
			else if (y > rect.yMax) // above the clip window
				code |= OutCode.TOP;

			return code;
		}

		// Cohen–Sutherland clipping algorithm clips a line from
		// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
		// diagonal from (xmin, ymin) to (xmax, ymax).
		internal static bool RectContainsLineSegment(Rect rect, float x0, float y0, float x1, float y1)
		{
			// compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
			OutCode outcode0 = ComputeOutCode(rect, x0, y0);
			OutCode outcode1 = ComputeOutCode(rect, x1, y1);

			bool accept = false;

			while (true)
			{
				if ((outcode0 | outcode1) == OutCode.INSIDE)
				{
					// bitwise OR is 0: both points inside window; trivially accept and exit loop
					accept = true;
					break;
				}
				else if ((outcode0 & outcode1) != OutCode.INSIDE)
				{
					// bitwise AND is not 0: both points share an outside zone (LEFT, RIGHT, TOP,
					// or BOTTOM), so both must be outside window; exit loop (accept is false)
					break;
				}
				else
				{
					// failed both tests, so calculate the line segment to clip
					// from an outside point to an intersection with clip edge
					float x = 0f, y = 0f;

					// At least one endpoint is outside the clip rectangle; pick it.
					OutCode outcodeOut = outcode0 != OutCode.INSIDE ? outcode0 : outcode1;

					// Now find the intersection point;
					// use formulas:
					//   slope = (y1 - y0) / (x1 - x0)
					//   x = x0 + (1 / slope) * (ym - y0), where ym is ymin or ymax
					//   y = y0 + slope * (xm - x0), where xm is xmin or xmax
					// No need to worry about divide-by-zero because, in each case, the
					// outcode bit being tested guarantees the denominator is non-zero
					if ((outcodeOut & OutCode.TOP) == OutCode.TOP)
					{
						// point is above the clip window
						x = x0 + (x1 - x0) * (rect.yMax - y0) / (y1 - y0);
						y = rect.yMax;
					}
					else if ((outcodeOut & OutCode.BOTTOM) == OutCode.BOTTOM)
					{
						// point is below the clip window
						x = x0 + (x1 - x0) * (rect.yMin - y0) / (y1 - y0);
						y = rect.yMin;
					}
					else if ((outcodeOut & OutCode.RIGHT) == OutCode.RIGHT)
					{
						// point is to the right of clip window
						y = y0 + (y1 - y0) * (rect.xMax - x0) / (x1 - x0);
						x = rect.xMax;
					}
					else if ((outcodeOut & OutCode.LEFT) == OutCode.LEFT)
					{
						// point is to the left of clip window
						y = y0 + (y1 - y0) * (rect.xMin - x0) / (x1 - x0);
						x = rect.xMin;
					}

					// Now we move outside point to intersection point to clip
					// and get ready for next pass.
					if (outcodeOut == outcode0)
					{
						x0 = x;
						y0 = y;
						outcode0 = ComputeOutCode(rect, x0, y0);
					}
					else
					{
						x1 = x;
						y1 = y;
						outcode1 = ComputeOutCode(rect, x1, y1);
					}
				}
			}

			return accept;
		}
	}
}