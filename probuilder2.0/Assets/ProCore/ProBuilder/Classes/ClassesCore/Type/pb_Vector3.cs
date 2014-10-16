using UnityEngine;
using System.Collections;

[System.Serializable()]
public class pb_Vector3
{
	public float x, y, z;

	public static implicit operator Vector3(pb_Vector3 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public static implicit operator pb_Vector3(Vector3 v)
	{
		return new pb_Vector3(v);
	}

	public pb_Vector3(Vector3 v)
	{
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
	}
}