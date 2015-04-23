using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;

/**
 *	\brief Responsible for mapping UV coordinates.  
 *	Generally should only be called by #pb_Object 
 *	after setting #pb_UV parameters.
 */
namespace ProBuilder2.Common {

public class pb_UVUtility
{

#region Map

	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings) { return PlanarMap(verts, uvSettings, null); }
	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings, Vector3? nrm)
	{
		Vector2[] uvs = pb_Math.PlanarProject(verts, nrm == null ? pb_Math.Normal(verts[0], verts[1], verts[2]) : (Vector3)nrm);
		
		uvs = ApplyUVSettings(uvs, uvSettings);
		return uvs;
	}
	
	private static Vector2[] ApplyUVSettings(Vector2[] uvs, pb_UV uvSettings)
	{

		int len = uvs.Length;

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

		// if(uvSettings.justify != pb_UV.Justify.None)
		// 	uvs = JustifyUVs(uvs, uvSettings.justify);

		// Apply transform last, so that fill and justify don't override it.
		pb_Bounds2D bounds = new pb_Bounds2D(uvs);

		if(!uvSettings.useWorldSpace)
			for(int i = 0; i < uvs.Length; i++)
				uvs[i] -= (bounds.center - bounds.extents);

		bounds = new pb_Bounds2D(uvs);

		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = uvs[i].ScaleAroundPoint(bounds.center, uvSettings.scale);			
			uvs[i] = uvs[i].RotateAroundPoint(bounds.center, uvSettings.rotation);
		}


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
		
		bounds = new pb_Bounds2D(uvs);

		uvSettings.localPivot = bounds.center;// uvSettings.useWorldSpace ? bounds.center : bounds.extents;
		uvSettings.localSize = bounds.size;

		for(int i = 0; i < uvs.Length; i++)
			uvs[i] -= uvSettings.offset;

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
#endregion
}
}