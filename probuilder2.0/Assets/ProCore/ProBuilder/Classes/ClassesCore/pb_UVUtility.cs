using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	static class pb_UVUtility
	{
		private static Vector2 tvec2 = Vector2.zero;

		public static void PlanarMap2(Vector3[] verts, Vector2[] uvs, int[] indices, pb_UV uvSettings, Vector3 normal)
		{
			ProjectionAxis projectionAxis = pb_Projection.VectorToProjectionAxis(normal);

			pb_Projection.PlanarProject(verts, uvs, indices, normal, projectionAxis);

			ApplyUVSettings(uvs, indices, uvSettings);
		}

		private static void ApplyUVSettings(Vector2[] uvs, int[] indices, pb_UV uvSettings)
		{
			int len = indices.Length;

			switch(uvSettings.fill)
			{
				case pb_UV.Fill.Tile:
					break;
				case pb_UV.Fill.Fit:
					uvs = NormalizeUVs(uvs, indices);
					break;
				case pb_UV.Fill.Stretch:
					uvs = StretchUVs(uvs, indices);
					break;
			}

			if(!uvSettings.useWorldSpace && uvSettings.anchor != pb_UV.Anchor.None)
				ApplyUVAnchor(uvs, indices, uvSettings.anchor);

			// Apply transform last, so that fill and justify don't override it.
			if( uvSettings.scale.x != 1f ||
				uvSettings.scale.y != 1f ||
				uvSettings.rotation != 0f)
			{
				Vector2 center = pb_Bounds2D.Center(uvs, indices);

				for(int i = 0; i < len; i++)
				{
					uvs[indices[i]] = uvs[indices[i]].ScaleAroundPoint(center, uvSettings.scale);
					uvs[indices[i]] = uvs[indices[i]].RotateAroundPoint(center, uvSettings.rotation);
				}
			}


			if(uvSettings.flipU || uvSettings.flipV || uvSettings.swapUV)
			{
				for(int i = 0; i < len; i++)
				{
					float 	u = uvs[indices[i]].x,
							v = uvs[indices[i]].y;

					if(uvSettings.flipU)
						u = -u;

					if(uvSettings.flipV)
						v = -v;

					if(!uvSettings.swapUV)
					{
						uvs[indices[i]].x = u;
						uvs[indices[i]].y = v;
					}
					else
					{
						uvs[indices[i]].x = v;
						uvs[indices[i]].y = u;
					}
				}
			}


			uvSettings.localPivot = pb_Bounds2D.Center(uvs, indices);

			for(int i = 0; i < indices.Length; i++)
			{
				uvs[indices[i]].x -= uvSettings.offset.x;
				uvs[indices[i]].y -= uvSettings.offset.y;
			}
		}

		private static Vector2[] StretchUVs(Vector2[] uvs, int[] indices)
		{
			Vector2 scale = pb_Math.LargestVector2(uvs, indices) - pb_Math.SmallestVector2(uvs, indices);

			for(int i = 0; i < indices.Length; i++)
			{
				uvs[i].x = uvs[indices[i]].x / scale.x;
				uvs[i].y = uvs[indices[i]].y / scale.y;
			}

			return uvs;
		}

		/*
		 *	Returns normalized UV values for a mesh uvs (0,0) - (1,1)
		 */
		private static Vector2[] NormalizeUVs(Vector2[] uvs, int[] indices)
		{
			/*
			 *	how this works -
			 *		- shift uv coordinates such that the lowest value x and y coordinates are zero
			 *		- scale non-zeroed coordinates uniformly to normalized values (0,0) - (1,1)
			 */

			int len = indices.Length;

			// shift UVs to zeroed coordinates
			Vector2 smallestVector2 = pb_Math.SmallestVector2(uvs, indices);

			int i;

			for(i = 0; i < len; i++)
			{
				uvs[indices[i]].x -= smallestVector2.x;
				uvs[indices[i]].y -= smallestVector2.y;
			}

			float scale = pb_Math.LargestValue( pb_Math.LargestVector2(uvs, indices) );

			for(i = 0; i < len; i++)
			{
				uvs[indices[i]].x /= scale;
				uvs[indices[i]].y /= scale;
			}

			return uvs;
		}

		[System.Obsolete("See ApplyAnchor().")]
		private static Vector2[] JustifyUVs(Vector2[] uvs, pb_UV.Justify j)
		{
			Vector2 amt = new Vector2(0f, 0f);

			switch(j)
			{
				case pb_UV.Justify.Left:
					amt = new Vector2(pb_Math.SmallestVector2(uvs).x, 0f);
					break;
				case pb_UV.Justify.Right:
					amt = new Vector2(pb_Math.LargestVector2(uvs).x - 1f, 0f);
					break;
				case pb_UV.Justify.Top:
					amt = new Vector2(0f, pb_Math.LargestVector2(uvs).y - 1f);
					break;
				case pb_UV.Justify.Bottom:
					amt = new Vector2(0f, pb_Math.SmallestVector2(uvs).y);
					break;
				case pb_UV.Justify.Center:
					amt = pb_Math.Average(uvs) - (new Vector2(.5f, .5f));
					break;
			}

			for(int i = 0; i < uvs.Length; i++)
				uvs[i] -= amt;

			return uvs;
		}

		private static void ApplyUVAnchor(Vector2[] uvs, int[] indices, pb_UV.Anchor anchor)
		{
			tvec2.x = 0f;
			tvec2.y = 0f;

			Vector2 min = pb_Math.SmallestVector2(uvs, indices);
			Vector2 max = pb_Math.LargestVector2(uvs, indices);

			if(	anchor == pb_UV.Anchor.UpperLeft || anchor == pb_UV.Anchor.MiddleLeft || anchor == pb_UV.Anchor.LowerLeft )
				tvec2.x = min.x;
			else
			if(	anchor == pb_UV.Anchor.UpperRight || anchor == pb_UV.Anchor.MiddleRight || anchor == pb_UV.Anchor.LowerRight )
				tvec2.x = max.x - 1f;
			else
				tvec2.x = (min.x + ((max.x - min.x) * .5f)) - .5f;

			if( anchor == pb_UV.Anchor.UpperLeft || anchor == pb_UV.Anchor.UpperCenter || anchor == pb_UV.Anchor.UpperRight)
				tvec2.y = max.y - 1f;
			else
			if( anchor == pb_UV.Anchor.MiddleLeft || anchor == pb_UV.Anchor.MiddleCenter || anchor == pb_UV.Anchor.MiddleRight)
				tvec2.y = (min.y + ((max.y - min.y) * .5f)) - .5f;
			else
				tvec2.y = min.y;

			int len = indices.Length;

			for(int i = 0; i < len; i++)
			{
				uvs[indices[i]].x -= tvec2.x;
				uvs[indices[i]].y -= tvec2.y;
			}
		}
	}
}
