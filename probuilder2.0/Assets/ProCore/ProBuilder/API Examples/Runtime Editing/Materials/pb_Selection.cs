using UnityEngine;
using System.Collections;
using ProBuilder2.Common;

public class pb_Selection
{
	public pb_Object pb;	///< This is the currently selected ProBuilder object.	
	public pb_Face face;	///< Keep a reference to the currently selected face.
	
	public pb_Selection(pb_Object _pb, pb_Face _face)
	{
		pb = _pb;
		face = _face;
	}

	public bool HasObject()
	{
		return pb != null;
	}

	public bool IsValid()
	{
		return pb != null && face != null;
	}

	public bool Equals(pb_Selection sel)
	{
		return (pb == sel.pb && face == sel.face);
	}

	public void Destroy()
	{
		if(pb != null)
			GameObject.Destroy(pb.gameObject);
	}

	public override string ToString()
	{
		return "pb_Object: " + pb == null ? "Null" : pb.name +
			"\npb_Face: " + ( (face == null) ? "Null" : face.ToString() );
	}
}