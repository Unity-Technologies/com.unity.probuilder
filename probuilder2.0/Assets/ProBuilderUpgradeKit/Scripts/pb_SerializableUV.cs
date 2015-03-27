using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{
	/**
	 * A serializable storage class for pb_Face.
	 */
	[System.Serializable()]
	public class pb_SerializableUV : ISerializable
	{
		public bool 			useWorldSpace;		///< If true, UV coordinates are calculated using world points instead of local.
		public bool 			flipU;				///< If true, the U value will be inverted.
		public bool 			flipV;				///< If true, the V value will be inverted.
		public bool 			swapUV;				///< If true, U and V values will switched.
		public pb_UV.Fill		fill;				///< Which Fill mode to use. 
		public Vector2			scale;				///< The scale to be applied to U and V coordinates.
		public Vector2			offset;				///< The offset to be applied to the UV coordinates.
		public float 			rotation;			///< Rotates UV coordinates.
		public pb_UV.Justify	justify;			///< Aligns UVs to the edges or center.

		public pb_SerializableUV() {}

		public pb_SerializableUV(pb_UV uv)
		{
			this.useWorldSpace		= uv.useWorldSpace;
			this.flipU				= uv.flipU;
			this.flipV				= uv.flipV;
			this.swapUV				= uv.swapUV;
			this.fill				= uv.fill;
			this.scale				= uv.scale;
			this.offset				= uv.offset;
			this.rotation			= uv.rotation;
			this.justify			= uv.justify;
		}

		// OnSerialize
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("useWorldSpace", 		useWorldSpace,			typeof(bool));
			info.AddValue("flipU", 				flipU,					typeof(bool));
			info.AddValue("flipV", 				flipV,					typeof(bool));
			info.AddValue("swapUV", 			swapUV,					typeof(bool));
			info.AddValue("fill", 				fill,					typeof(pb_UV.Fill));
			info.AddValue("scale", 				(pb_Vector2)scale,		typeof(pb_Vector2));
			info.AddValue("offset", 			(pb_Vector2)offset,		typeof(pb_Vector2));
			info.AddValue("rotation", 			rotation,				typeof(float));
			info.AddValue("justify", 			justify,				typeof(pb_UV.Justify));
		}

		// The pb_SerializedMesh constructor is used to deserialize values. 
		public pb_SerializableUV(SerializationInfo info, StreamingContext context)
		{
			this.useWorldSpace		= (bool)			info.GetValue("useWorldSpace", 	typeof(bool));
			this.flipU				= (bool)			info.GetValue("flipU", 			typeof(bool));
			this.flipV				= (bool)			info.GetValue("flipV", 			typeof(bool));
			this.swapUV				= (bool)			info.GetValue("swapUV", 		typeof(bool));
			this.fill				= (pb_UV.Fill)		info.GetValue("fill", 			typeof(pb_UV.Fill));
			this.scale				= (pb_Vector2) 		info.GetValue("scale", 			typeof(pb_Vector2));
			this.offset				= (pb_Vector2) 		info.GetValue("offset", 		typeof(pb_Vector2));
			this.rotation			= (float)			info.GetValue("rotation", 		typeof(float));
			this.justify			= (pb_UV.Justify)	info.GetValue("justify", 		typeof(pb_UV.Justify));
		}

		public static explicit operator pb_UV(pb_SerializableUV serialized)
		{
			return new pb_UV(
					serialized.useWorldSpace,
					serialized.flipU,
					serialized.flipV,
					serialized.swapUV,
					serialized.fill,
					serialized.scale,
					serialized.offset,
					serialized.rotation,
					serialized.justify
				);
		}
	}
}