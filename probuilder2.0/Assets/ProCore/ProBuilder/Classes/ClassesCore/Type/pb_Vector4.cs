using UnityEngine;
using System.Collections;

[System.Serializable()]
public class pb_Vector4
{
	public float x, y, z, w;

	public static implicit operator Vector4(pb_Vector4 v) 
	{
		return new Vector4(v.x, v.y, v.z, v.w);
	}

	public static implicit operator pb_Vector4(Vector4 v)
	{
		return new pb_Vector4(v);
	}

	public static implicit operator Quaternion(pb_Vector4 v) 
	{
		return new Quaternion(v.x, v.y, v.z, v.w);
	}

	public static implicit operator pb_Vector4(Quaternion v)
	{
		return new pb_Vector4(v);
	}

	public pb_Vector4() {}

	public pb_Vector4(Vector4 v)
	{
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
		this.w = v.w;
	}

	public pb_Vector4(Quaternion v)
	{
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
		this.w = v.w;
	}
}