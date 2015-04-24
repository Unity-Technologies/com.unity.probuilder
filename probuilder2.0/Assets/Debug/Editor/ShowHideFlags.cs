using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

public class ShowHideFlags : Editor
{
	[MenuItem("Tools/Show HideFalgs")]
	static void Indot()
	{
		foreach(Transform t in Selection.transforms)
			Debug.Log(t.gameObject.hideFlags);
	}

	[MenuItem("Tools/Print Snap Settings")]
	public static void dflkajsdkflj()
	{
		string txt = "Snap Enabled: ";
		txt += pb_ProGrids_Interface.SnapEnabled();
		txt += "\nAxis Constraints: " + pb_ProGrids_Interface.UseAxisConstraints();
		txt += "\nSnap Value: " + pb_ProGrids_Interface.SnapValue();

		Debug.Log(txt);
	}

	[MenuItem("Tools/Show Prefab Info")]
	static void Indoadfadsfadsft()
	{
		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			if(	(PrefabUtility.GetPrefabType(pb.gameObject) == PrefabType.PrefabInstance ||
					 PrefabUtility.GetPrefabType(pb.gameObject) == PrefabType.Prefab ) )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				foreach(PropertyModification pm in PrefabUtility.GetPropertyModifications(pb))
				{
					sb.AppendLine(	(pm.objectReference != null ? pm.objectReference.name : "") + ": " +
									(pm.propertyPath != null ? pm.propertyPath : "") + ", " +
									(pm.target != null ? pm.target.name : ""));
				}

				Debug.Log("Name: " + pb.name + "\n" + sb.ToString());
			}
		}
	}

	[MenuItem("Tools/ProBuilder/Subdivide Triangles")]
	public static void Subdivid()
	{
		Undo.RecordObjects(Selection.transforms.GetComponents<pb_Object>(), "Subdivide");

		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			Vector3[] v = SubdivideIcosahedron(pb.vertices);
			pb.SetVertices(v);
			pb.SetColors(new Color[v.Length]);
			pb.SetUV(new Vector2[v.Length]);
			pb_Face[] f = new pb_Face[v.Length/3];
			for(int i = 0; i < v.Length; i+=3)
				f[i/3] = new pb_Face( new int[] { i, i+1, i+2 } );
			pb.SetFaces(f);
			pb.GeometryWithVerticesFaces(v, f);
		}
	}
		static Vector3[] SubdivideIcosahedron(Vector3[] vertices)
	{
		Vector3[] v = new Vector3[vertices.Length * 4];

		int index = 0;

		Vector3 p0 = Vector3.zero,	//	    5
				p1 = Vector3.zero,	//    3   4
				p2 = Vector3.zero,	//	0,  1,  2
				p3 = Vector3.zero,
				p4 = Vector3.zero,
				p5 = Vector3.zero;

		for(int i = 0; i < vertices.Length; i+=3)
		{
			p0 = vertices[i+0];
			p2 = vertices[i+2];
			p5 = vertices[i+1];
			p1 = PointOnSphere( (p0 + p2) * .5f );
			p3 = PointOnSphere( (p0 + p5) * .5f );
			p4 = PointOnSphere( (p2 + p5) * .5f );

			v[index++] = p0;
			v[index++] = p1;
			v[index++] = p3;

			v[index++] = p1;
			v[index++] = p2;
			v[index++] = p4;

			v[index++] = p1;
			v[index++] = p4;
			v[index++] = p3;

			v[index++] = p3;
			v[index++] = p4;
			v[index++] = p5;
		}

		return v;
	}
	 	
	static Vector3 PointOnSphere(Vector3 v)
	{
		float len = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
		return v / len;
	}
}
