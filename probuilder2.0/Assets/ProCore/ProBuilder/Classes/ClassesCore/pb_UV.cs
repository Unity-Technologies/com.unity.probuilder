// Disable warnings for Justify enum
#pragma warning disable 0618

/*
 *	UV Settings for ProBuilder Objects
 */
using UnityEngine;
using System.Runtime.Serialization;

/**
 *	\brief Container for UV mapping parameters per face.
 */
[System.Serializable]
public class pb_UV
{
	/**
	 * Defines the anchor point of UV calculations.
	 */
	[System.Obsolete("See pb_UV.Anchor")]
	public enum Justify {
		Right,
		Left,
		Top,
		Center,
		Bottom,
		None
	}

	/**
	 * The origin point of UVs.
	 */
	public enum Anchor
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight
	}

	/**
	 * Fill mode.
	 */
	public enum Fill {
		Fit,
		Tile,
		Stretch
	}

	public bool 			useWorldSpace;		///< If true, UV coordinates are calculated using world points instead of local.
	public bool 			flipU;				///< If true, the U value will be inverted.
	public bool 			flipV;				///< If true, the V value will be inverted.
	public bool 			swapUV;				///< If true, U and V values will switched.
	public Fill 			fill;				///< Which Fill mode to use.
	public Vector2			scale;				///< The scale to be applied to U and V coordinates.
	public Vector2			offset;				///< The offset to be applied to the UV coordinates.
	public float 			rotation;			///< Rotates UV coordinates.
	[System.Obsolete("Please use pb_UV.anchor.")]
	public Justify 			justify;			///< Aligns UVs to the edges or center.
	public Vector2			localPivot;			///< The center point of the mapped UVs prior to offset application.
	[System.Obsolete("localPivot and localSize are no longer stored.")]
	public Vector2			localSize;			///< The size of the mapped UVs prior to modifications.
	public Anchor 			anchor;				///< Where the UV coordinate origin is.

	public pb_UV()
	{
		this.useWorldSpace = false;
		this.flipU = false;
		this.flipV = false;
		this.swapUV = false;
		this.fill = Fill.Tile;
		this.scale = new Vector2(1f, 1f);
		this.offset = new Vector2(0f, 0f);
		this.rotation = 0f;
		this.anchor = Anchor.LowerLeft;
	}

	public pb_UV(pb_UV uvs)
	{
		this.useWorldSpace = uvs.useWorldSpace;
		this.flipU = uvs.flipU;
		this.flipV = uvs.flipV;
		this.swapUV = uvs.swapUV;
		this.fill = uvs.fill;
		this.scale = uvs.scale;
		this.offset = uvs.offset;
		this.rotation = uvs.rotation;
		this.anchor = uvs.anchor;
	}

	[System.Obsolete("Please use constructor with pb_UV.Anchor parameter.")]
	public pb_UV(
		// ProjectionAxis	 _projectionAxis,
		bool 			_useWorldSpace,
		bool 			_flipU,
		bool 			_flipV,
		bool 			_swapUV,
		Fill 			_fill,
		Vector2 		_scale,
		Vector2 		_offset,
		float 			_rotation,
		Justify 		_justify
		)
	{
		this.useWorldSpace		= _useWorldSpace;
		this.flipU				= _flipU;
		this.flipV				= _flipV;
		this.swapUV				= _swapUV;
		this.fill				= _fill;
		this.scale				= _scale;
		this.offset				= _offset;
		this.rotation			= _rotation;
		this.justify			= _justify;
	}

	public void Reset()
	{
		this.useWorldSpace 		= false;
		this.flipU 				= false;
		this.flipV 				= false;
		this.swapUV 			= false;
		this.fill 				= Fill.Tile;
		this.scale 				= new Vector2(1f, 1f);
		this.offset 			= new Vector2(0f, 0f);
		this.rotation 			= 0f;
		this.anchor 			= Anchor.LowerLeft;
	}

	public override string ToString()
	{
		string str =
			"Use World Space: " + useWorldSpace + "\n" +
			"Flip U: " + flipU + "\n" +
			"Flip V: " + flipV + "\n" +
			"Swap UV: " + swapUV + "\n" +
			"Fill Mode: " + fill + "\n" +
			"Anchor: " + anchor + "\n" +
			"Scale: " + scale + "\n" +
			"Offset: " + offset + "\n" +
			"Rotation: " + rotation + "\n" +
			"Pivot: " + localPivot + "\n";
		return str;
	}
}
