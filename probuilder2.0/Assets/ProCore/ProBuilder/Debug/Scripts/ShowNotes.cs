using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShowNotes : MonoBehaviour {

	[TextAreaAttribute]
	public string Notes = "";
	bool mouseOver = false;
	static GUIStyle LabelStyle;
	string stats = "";

	Mesh boundsMesh;
	static Material BoundsMaterial;

	void Awake()
	{
		if( LabelStyle == null )
		{
			LabelStyle = new GUIStyle();
			LabelStyle.normal.textColor = Color.white;
			LabelStyle.fontStyle = FontStyle.Bold;
			LabelStyle.wordWrap = true;
		}

		if(BoundsMaterial == null)
		{
			BoundsMaterial = new Material(Shader.Find("Custom/UnlitVertexColor"));
		}
	}

	void Start()
	{
		stats = "\nStatically Batched: " + gameObject.GetComponent<MeshRenderer>().isPartOfStaticBatch;
		boundsMesh = DrawBounds();
	}

	void OnMouseEnter()
	{
		mouseOver = true;
	}

	void OnMouseExit()
	{
		mouseOver = false;
	}

	Rect al = new Rect(0,0,200,100);
	void OnGUI()
	{
		if(mouseOver)
		{
			Vector2 mpos = Input.mousePosition;

			al.x = mpos.x;
			al.y = (Screen.height-mpos.y)+22;

			GUI.Box(al, Notes + stats, LabelStyle);
		}
	}

	void Update()
	{
		Graphics.DrawMesh(boundsMesh, Vector3.zero, Quaternion.identity, BoundsMaterial, 0);
	}

	// REnder
	Mesh DrawBounds()
	{
		Bounds b = gameObject.GetComponent<MeshRenderer>().bounds;
		Vector3 cen = b.center;
		Vector3 ext = b.extents;

		// Draw Wireframe
		List<Vector3> v = new List<Vector3>();

		v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y, -ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y,  ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y, -ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y,  ext.z, .2f) );

		v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y, -ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y,  ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y, -ext.z, .2f) );
		v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y,  ext.z, .2f) );

		Vector2[] u = new Vector2[48];
		int[] t = new int[48];
		Color[] c = new Color[48];

		for(int i = 0; i < 48; i++)
		{
			t[i] = i;
			u[i] = Vector2.zero;
			c[i] = Color.white;
			c[i].a = .5f;
		}

		// for(int i = 0; i < 48; i+=2)
		// {
		// 		// To render axis colors
		// 		Vector3 dir = (v[i+1] - v[i]).normalized;
		// 		c[i+0] = new Color( Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z), .8f);
		// 		c[i+1] = new Color( Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z), .8f);

		// }

		Mesh m = new Mesh();
		m.vertices = v.ToArray();
		m.subMeshCount = 1;
		m.SetIndices(t, MeshTopology.Lines, 0);
		m.uv = u;
		m.normals = v.ToArray();
		m.colors = c; 

		return m;
	}

	private Vector3[] DrawBoundsEdge(Vector3 center, float x, float y, float z, float size)
	{
		Vector3 p = center;
		Vector3[] v = new Vector3[6];

		p.x += x;
		p.y += y;
		p.z += z;

		v[0] = p;
		v[1] = (p + ( -(x/Mathf.Abs(x)) * Vector3.right 	* Mathf.Min(size, Mathf.Abs(x))));

		v[2] = p;
		v[3] = (p + ( -(y/Mathf.Abs(y)) * Vector3.up 		* Mathf.Min(size, Mathf.Abs(y))));

		v[4] = p;
		v[5] = (p + ( -(z/Mathf.Abs(z)) * Vector3.forward 	* Mathf.Min(size, Mathf.Abs(z))));

		return v;
	}
}
