using UnityEngine;
using System.Collections;

[System.Serializable()]
public class pb_Color
{
	public float r, g, b, a;

	public static implicit operator Color(pb_Color c) 
	{
		return new Color(c.r, c.g, c.b, c.a);
	}

	public static implicit operator pb_Color(Color c)
	{
		return new pb_Color(c);
	}

	public pb_Color(Color c)
	{
		this.r = c.r;
		this.g = c.g;
		this.b = c.b;
		this.a = c.a;
	}

	public pb_Color(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}
}