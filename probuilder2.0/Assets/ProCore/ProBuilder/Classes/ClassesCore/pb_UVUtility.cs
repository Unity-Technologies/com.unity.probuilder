using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 *	\brief Responsible for mapping UV coordinates.
 *	Generally should only be called by #pb_Object
 *	after setting #pb_UV parameters.
 */
namespace ProBuilder2.Common {

public class pb_UVUtility
{

#region Map

	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings, Vector3 normal)
	{
		profiler.BeginSample("Project");
		Vector2[] uvs = pb_Projection.PlanarProject(verts, normal);
		profiler.EndSample();
		profiler.BeginSample("Apply Settings");
		uvs = ApplyUVSettings(uvs, uvSettings);
		profiler.EndSample();
		return uvs;
	}

	private static pb_Bounds2D bounds = new pb_Bounds2D();

	private static Vector2[] ApplyUVSettings(Vector2[] uvs, pb_UV uvSettings)
	{
		int len = uvs.Length;

		profiler.BeginSample("FillMode");
		switch(uvSettings.fill)
		{
			case pb_UV.Fill.Tile:
				break;
			case pb_UV.Fill.Fit:
				uvs = NormalizeUVs(uvs);
				break;
			case pb_UV.Fill.Stretch:
				uvs = StretchUVs(uvs);
				break;
		}
		profiler.EndSample();

		profiler.BeginSample("ApplyUVAnchor");

		if(!uvSettings.useWorldSpace)
			ApplyUVAnchor(uvs, uvSettings.anchor);
		profiler.EndSample();

		profiler.BeginSample("Scale/Rotate");
		
		// Apply transform last, so that fill and justify don't override it.
		if( uvSettings.scale.x != 1f || 
			uvSettings.scale.y != 1f ||
			uvSettings.rotation != 0f)
		{
			bounds.SetWithPoints(uvs);

			for(int i = 0; i < len; i++)
			{
				uvs[i] = uvs[i].ScaleAroundPoint(bounds.center, uvSettings.scale);
				uvs[i] = uvs[i].RotateAroundPoint(bounds.center, uvSettings.rotation);
			}
		}
		profiler.EndSample();

		profiler.BeginSample("Flip");
		for(int i = 0; i < len; i++)
		{
			float u = uvs[i].x, v = uvs[i].y;

			if(uvSettings.flipU)
				u = -u;

			if(uvSettings.flipV)
				v = -v;

			if(!uvSettings.swapUV)
				uvs[i] = new Vector2(u, v);
			else
				uvs[i] = new Vector2(v, u);
		}
		profiler.EndSample();

		profiler.BeginSample("Set Bounds");
		bounds.SetWithPoints(uvs);

		uvSettings.localPivot = bounds.center;
		uvSettings.localSize = bounds.size;
		profiler.EndSample();

		profiler.BeginSample("Translate");
		for(int i = 0; i < uvs.Length; i++)
			uvs[i] -= uvSettings.offset;
		profiler.EndSample();

		return uvs;
	}
#endregion

#region UTILITY

	private static Vector2[] StretchUVs(Vector2[] uvs)
	{
		Vector2 scale = pb_Math.LargestVector2(uvs) - pb_Math.SmallestVector2(uvs);

		for(int i = 0; i < uvs.Length; i++)
			uvs[i] = new Vector2(uvs[i].x/scale.x, uvs[i].y/scale.y);

		return uvs;
	}

	/*
	 *	Returns normalized UV values for a mesh uvs (0,0) - (1,1)
	 */
	private static Vector2[] NormalizeUVs(Vector2[] uvs)
	{
		/*
		 *	how this works -
		 *		- shift uv coordinates such that the lowest value x and y coordinates are zero
		 *		- scale non-zeroed coordinates uniformly to normalized values (0,0) - (1,1)
		 */

		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = pb_Math.SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		float scale = pb_Math.LargestValue( pb_Math.LargestVector2(uvs) );

		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] /= scale;
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

	private static void ApplyUVAnchor(Vector2[] uvs, pb_UV.Anchor anchor)
	{
		Vector2 scoot = Vector2.zero;

		Vector2 min = pb_Math.SmallestVector2(uvs);
		Vector2 max = pb_Math.LargestVector2(uvs);

		if(	anchor == pb_UV.Anchor.UpperLeft || anchor == pb_UV.Anchor.MiddleLeft || anchor == pb_UV.Anchor.LowerLeft )
			scoot.x = min.x;
		else
		if(	anchor == pb_UV.Anchor.UpperRight || anchor == pb_UV.Anchor.MiddleRight || anchor == pb_UV.Anchor.LowerRight )
			scoot.x = max.x - 1f;
		else
			scoot.x = (min.x + ((max.x - min.x) * .5f)) - .5f;

		if( anchor == pb_UV.Anchor.UpperLeft || anchor == pb_UV.Anchor.UpperCenter || anchor == pb_UV.Anchor.UpperRight)
			scoot.y = max.y - 1f;
		else
		if( anchor == pb_UV.Anchor.MiddleLeft || anchor == pb_UV.Anchor.MiddleCenter || anchor == pb_UV.Anchor.MiddleRight)
			scoot.y = (min.y + ((max.y - min.y) * .5f)) - .5f;
		else
			scoot.y = min.y;

		for(int i = 0; i < uvs.Length; i++)
			uvs[i] -= scoot;
	}
#endregion
}
}
