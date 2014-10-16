using UnityEngine;
using System.Collections;

[System.Serializable()]
public class pb_Vector2
{
	public float x, y;

	public static implicit operator Vector2(pb_Vector2 v) 
	{
		return new Vector2(v.x, v.y);
	}

	public static implicit operator pb_Vector2(Vector2 v)
	{
		return new pb_Vector2(v);
	}

	public pb_Vector2(Vector2 v)
	{
		this.x = v.x;
		this.y = v.y;
	}
}