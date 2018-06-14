using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	static class UnwrappingUtility
	{
		static Vector2 s_TempVector2 = Vector2.zero;

		public static void PlanarMap2(Vector3[] verts, Vector2[] uvs, int[] indexes, AutoUnwrapSettings uvSettings, Vector3 normal)
		{
			ProjectionAxis projectionAxis = Projection.VectorToProjectionAxis(normal);

			Projection.PlanarProject(verts, uvs, indexes, normal, projectionAxis);

			ApplyUVSettings(uvs, indexes, uvSettings);
		}

		static void ApplyUVSettings(Vector2[] uvs, int[] indexes, AutoUnwrapSettings uvSettings)
		{
			int len = indexes.Length;

			switch(uvSettings.fill)
			{
				case AutoUnwrapSettings.Fill.Tile:
					break;
				case AutoUnwrapSettings.Fill.Fit:
					uvs = NormalizeUVs(uvs, indexes);
					break;
				case AutoUnwrapSettings.Fill.Stretch:
					uvs = StretchUVs(uvs, indexes);
					break;
			}

			if(!uvSettings.useWorldSpace && uvSettings.anchor != AutoUnwrapSettings.Anchor.None)
				ApplyUVAnchor(uvs, indexes, uvSettings.anchor);

			// Apply transform last, so that fill and justify don't override it.
			if( uvSettings.scale.x != 1f ||
				uvSettings.scale.y != 1f ||
				uvSettings.rotation != 0f)
			{
				Vector2 center = Bounds2D.Center(uvs, indexes);

				for(int i = 0; i < len; i++)
				{
					uvs[indexes[i]] = uvs[indexes[i]].ScaleAroundPoint(center, uvSettings.scale);
					uvs[indexes[i]] = uvs[indexes[i]].RotateAroundPoint(center, uvSettings.rotation);
				}
			}


			if(uvSettings.flipU || uvSettings.flipV || uvSettings.swapUV)
			{
				for(int i = 0; i < len; i++)
				{
					float 	u = uvs[indexes[i]].x,
							v = uvs[indexes[i]].y;

					if(uvSettings.flipU)
						u = -u;

					if(uvSettings.flipV)
						v = -v;

					if(!uvSettings.swapUV)
					{
						uvs[indexes[i]].x = u;
						uvs[indexes[i]].y = v;
					}
					else
					{
						uvs[indexes[i]].x = v;
						uvs[indexes[i]].y = u;
					}
				}
			}

			for(int i = 0; i < indexes.Length; i++)
			{
				uvs[indexes[i]].x -= uvSettings.offset.x;
				uvs[indexes[i]].y -= uvSettings.offset.y;
			}
		}

		static Vector2[] StretchUVs(Vector2[] uvs, int[] indexes)
		{
			Vector2 scale = Math.LargestVector2(uvs, indexes) - Math.SmallestVector2(uvs, indexes);

			for(int i = 0; i < indexes.Length; i++)
			{
				uvs[i].x = uvs[indexes[i]].x / scale.x;
				uvs[i].y = uvs[indexes[i]].y / scale.y;
			}

			return uvs;
		}

		/// <summary>
		/// Returns normalized UV values for a mesh uvs (0,0) - (1,1)
		/// </summary>
		/// <param name="uvs"></param>
		/// <param name="indexes"></param>
		/// <returns></returns>
		static Vector2[] NormalizeUVs(Vector2[] uvs, int[] indexes)
		{
			/*
			 *	how this works -
			 *		- shift uv coordinates such that the lowest value x and y coordinates are zero
			 *		- scale non-zeroed coordinates uniformly to normalized values (0,0) - (1,1)
			 */

			int len = indexes.Length;

			// shift UVs to zeroed coordinates
			Vector2 smallestVector2 = Math.SmallestVector2(uvs, indexes);

			int i;

			for(i = 0; i < len; i++)
			{
				uvs[indexes[i]].x -= smallestVector2.x;
				uvs[indexes[i]].y -= smallestVector2.y;
			}

			float scale = Math.LargestValue( Math.LargestVector2(uvs, indexes) );

			for(i = 0; i < len; i++)
			{
				uvs[indexes[i]].x /= scale;
				uvs[indexes[i]].y /= scale;
			}

			return uvs;
		}

		static void ApplyUVAnchor(Vector2[] uvs, int[] indexes, AutoUnwrapSettings.Anchor anchor)
		{
			s_TempVector2.x = 0f;
			s_TempVector2.y = 0f;

			Vector2 min = Math.SmallestVector2(uvs, indexes);
			Vector2 max = Math.LargestVector2(uvs, indexes);

			if(	anchor == AutoUnwrapSettings.Anchor.UpperLeft || anchor == AutoUnwrapSettings.Anchor.MiddleLeft || anchor == AutoUnwrapSettings.Anchor.LowerLeft )
				s_TempVector2.x = min.x;
			else
			if(	anchor == AutoUnwrapSettings.Anchor.UpperRight || anchor == AutoUnwrapSettings.Anchor.MiddleRight || anchor == AutoUnwrapSettings.Anchor.LowerRight )
				s_TempVector2.x = max.x - 1f;
			else
				s_TempVector2.x = (min.x + ((max.x - min.x) * .5f)) - .5f;

			if( anchor == AutoUnwrapSettings.Anchor.UpperLeft || anchor == AutoUnwrapSettings.Anchor.UpperCenter || anchor == AutoUnwrapSettings.Anchor.UpperRight)
				s_TempVector2.y = max.y - 1f;
			else
			if( anchor == AutoUnwrapSettings.Anchor.MiddleLeft || anchor == AutoUnwrapSettings.Anchor.MiddleCenter || anchor == AutoUnwrapSettings.Anchor.MiddleRight)
				s_TempVector2.y = (min.y + ((max.y - min.y) * .5f)) - .5f;
			else
				s_TempVector2.y = min.y;

			int len = indexes.Length;

			for(int i = 0; i < len; i++)
			{
				uvs[indexes[i]].x -= s_TempVector2.x;
				uvs[indexes[i]].y -= s_TempVector2.y;
			}
		}
	}
}
