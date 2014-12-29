/*
 *	UV Settings for ProBuilder Objects
 */
using UnityEngine;
using System.Runtime.Serialization;

[System.Serializable]
/**
 *	\brief Container for UV mapping parameters per face.
 */
public class pb_UV : ISerializable {

#region ENUM

	/**
	 * Defines the anchor point of UV calculations.
	 */
	public enum Justify {
		Right,
		Left,
		Top,
		Center,
		Bottom,
		None
	}

	/**
	 * Fill mode.
	 */
	public enum Fill {
		Fit,
		Tile,
		Stretch
	}

#endregion

#region MEMBERS

	public bool 			useWorldSpace;		///< If true, UV coordinates are calculated using world points instead of local.
	public bool 			flipU;				///< If true, the U value will be inverted.
	public bool 			flipV;				///< If true, the V value will be inverted.
	public bool 			swapUV;				///< If true, U and V values will switched.
	public Fill 			fill;				///< Which Fill mode to use. 
	public Vector2			scale;				///< The scale to be applied to U and V coordinates.
	public Vector2			offset;				///< The offset to be applied to the UV coordinates.
	public float 			rotation;			///< Rotates UV coordinates.
	public Justify 			justify;			///< Aligns UVs to the edges or center.
	public Vector2			localPivot;			///< The center point of the mapped UVs prior to offset application.
	public Vector2			localSize;			///< The size of the mapped UVs prior to modifications.

	// OnSerialize
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("useWorldSpace", 		useWorldSpace,			typeof(bool));
		info.AddValue("flipU", 				flipU,					typeof(bool));
		info.AddValue("flipV", 				flipV,					typeof(bool));
		info.AddValue("swapUV", 			swapUV,					typeof(bool));
		info.AddValue("fill", 				fill,					typeof(Fill));
		info.AddValue("scale", 				(pb_Vector2)scale,		typeof(pb_Vector2));
		info.AddValue("offset", 			(pb_Vector2)offset,		typeof(pb_Vector2));
		info.AddValue("rotation", 			rotation,				typeof(float));
		info.AddValue("justify", 			justify,				typeof(Justify));
		info.AddValue("localPivot", 		(pb_Vector2)localPivot,	typeof(pb_Vector2));
		info.AddValue("localSize", 			(pb_Vector2)localSize,	typeof(pb_Vector2));
	}

	// The pb_SerializedMesh constructor is used to deserialize values. 
	public pb_UV(SerializationInfo info, StreamingContext context)
	{
		this.useWorldSpace			= (bool)		info.GetValue("useWorldSpace", 	typeof(bool));
		this.flipU					= (bool)		info.GetValue("flipU", 			typeof(bool));
		this.flipV					= (bool)		info.GetValue("flipV", 			typeof(bool));
		this.swapUV					= (bool)		info.GetValue("swapUV", 		typeof(bool));
		this.fill					= (Fill)		info.GetValue("fill", 			typeof(Fill));
		this.scale					= (pb_Vector2) 	info.GetValue("scale", 			typeof(pb_Vector2));
		this.offset					= (pb_Vector2) 	info.GetValue("offset", 		typeof(pb_Vector2));
		this.rotation				= (float)		info.GetValue("rotation", 		typeof(float));
		this.justify				= (Justify)		info.GetValue("justify", 		typeof(Justify));
		this.localPivot				= (pb_Vector2)	info.GetValue("localPivot", 	typeof(pb_Vector2));
		this.localSize				= (pb_Vector2)	info.GetValue("localSize", 		typeof(pb_Vector2));
	}
#endregion

#region INITIALIZATION

	public pb_UV()
	{
		useWorldSpace = false;
		justify = Justify.None;
		flipU = false;
		flipV = false;
		swapUV = false;
		fill = Fill.Tile;
		scale = new Vector2(1f, 1f);
		offset = new Vector2(0f, 0f);
		rotation = 0f;
	}

	public pb_UV(pb_UV uvs)
	{
		useWorldSpace = uvs.useWorldSpace;
		flipU = uvs.flipU;
		flipV = uvs.flipV;
		swapUV = uvs.swapUV;
		fill = uvs.fill;
		scale = uvs.scale;
		offset = uvs.offset;
		rotation = uvs.rotation;
		justify = uvs.justify;
	}

	public pb_UV(
		// ProjectionAxis	_projectionAxis,
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
		this.justify 			= Justify.None;
		this.flipU 				= false;
		this.flipV 				= false;
		this.swapUV 			= false;
		this.fill 				= Fill.Tile;
		this.scale 				= new Vector2(1f, 1f);
		this.offset 			= new Vector2(0f, 0f);
		this.rotation 			= 0f;
	}

#endregion

#region CONSTANT
	
	public static pb_UV LightmapUVSettings = new pb_UV(
		true,						// useWorldSpace -- we want to retain relative scale
		false,						// flipU			
		false,						// flipV			
		false,						// swapUV			
		Fill.Fit,					// fill			
		new Vector2(1f, 1f),		// scale			
		new Vector2(0f, 0f),		// offset			
		0f,							// rotation		
		Justify.None);				// justify			

#endregion

#region DEBUG

	public override string ToString()
	{
		string str = 
			"Use World Space: " + useWorldSpace + "\n" +
			"Flip U: " + flipU + "\n" +
			"Flip V: " + flipV + "\n" +
			"Swap UV: " + swapUV + "\n" +
			"Fill Mode: " + fill + "\n" +
			"Justification: " + justify + "\n" +
			"Scale: " + scale + "\n" +
			"Offset: " + offset + "\n" +
			"Rotation: " + rotation + "\n" +
			"Pivot: " + localPivot + "\n";
		return str;
	}

	public string ToString(string delim)
	{
		string str = delim.Replace("\n", "") + 
			"Use World Space: " + useWorldSpace + delim +
			"Flip U: " + flipU + delim +
			"Flip V: " + flipV + delim +
			"Swap UV: " + swapUV + delim +
			"Fill Mode: " + fill + delim +
			"Justification: " + justify + delim +
			"Scale: " + scale + delim +
			"Offset: " + offset + delim +
			"Rotation: " + rotation + delim +
			"Pivot: " + localPivot;
		return str;
	}

#endregion

}
