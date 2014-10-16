/**
 *	Unity Example script - found in the docs or Unify Wiki.
 */

using UnityEngine;
using System.Collections;

public class pb_MeshCombineUtility {
	
	public struct MeshInstance
	{
		public Mesh      mesh;
		public int       subMeshIndex;            
		public Matrix4x4 transform;
	}
	
	/// Usually rendering with triangle strips is faster.
	/// However when combining objects with very low triangle counts, it can be faster to use triangles.
	/// Best is to try out which value is faster in practice.

	/// This option has a far longer preprocessing time at startup but leads to better runtime performance.
	public static void CombineMeshFilters(MeshFilter[] mf, Transform t, bool generateTriangleStrips) {

		MeshFilter[] filters  = mf;
		Matrix4x4 myTransform = t.worldToLocalMatrix;
		Hashtable materialToMesh= new Hashtable();

		for (int i=0;i<filters.Length;i++) {
			MeshFilter filter = (MeshFilter)filters[i];
			Renderer curRenderer  = filters[i].GetComponent<Renderer>();
			pb_MeshCombineUtility.MeshInstance instance = new pb_MeshCombineUtility.MeshInstance ();
			instance.mesh = filter.sharedMesh;
			if (curRenderer != null && curRenderer.enabled && instance.mesh != null) {
				instance.transform = myTransform * filter.transform.localToWorldMatrix;
				
				Material[] materials = curRenderer.sharedMaterials;
				for (int m=0;m<materials.Length;m++) {
					instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
	
					ArrayList objects = (ArrayList)materialToMesh[materials[m]];
					if (objects != null) {
						objects.Add(instance);
					}
					else
					{
						objects = new ArrayList ();
						objects.Add(instance);
						materialToMesh.Add(materials[m], objects);
					}
				}
				
				curRenderer.enabled = false;
			}
		}
	
		foreach (DictionaryEntry de  in materialToMesh) {
			ArrayList elements = (ArrayList)de.Value;
			pb_MeshCombineUtility.MeshInstance[] instances = (pb_MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(pb_MeshCombineUtility.MeshInstance));

			// We have a maximum of one material, so just attach the mesh to our own game object
			if (materialToMesh.Count == 1)
			{
				// Make sure we have a mesh filter & renderer
				if (t.GetComponent(typeof(MeshFilter)) == null)
					t.gameObject.AddComponent(typeof(MeshFilter));
				if (!t.GetComponent<MeshRenderer>())
					t.gameObject.AddComponent<MeshRenderer>();
	
				MeshFilter filter = (MeshFilter)t.GetComponent(typeof(MeshFilter));
				filter.mesh = pb_MeshCombineUtility.Combine(instances, generateTriangleStrips);
				t.GetComponent<Renderer>().material = (Material)de.Key;
				t.GetComponent<Renderer>().enabled = true;
			}
			// We have multiple materials to take care of, build one mesh / gameobject for each material
			// and parent it to this object
			else
			{
				GameObject go = new GameObject("Combined mesh");
				go.transform.parent = t.transform;
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localPosition = Vector3.zero;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent<MeshRenderer>();
				go.GetComponent<Renderer>().material = (Material)de.Key;
				MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
				filter.mesh = pb_MeshCombineUtility.Combine(instances, generateTriangleStrips);
			}
		}	
	}	

	public static Mesh Combine (MeshInstance[] combines, bool generateStrips)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		int stripCount = 0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				vertexCount += combine.mesh.vertexCount;
				
				if (generateStrips)
				{
					// SUBOPTIMAL FOR PERFORMANCE
					int curStripCount = combine.mesh.GetTriangles(combine.subMeshIndex).Length;
					if (curStripCount != 0)
					{
						if( stripCount != 0 )
						{
							if ((stripCount & 1) == 1 )
								stripCount += 3;
							else
								stripCount += 2;
						}
						stripCount += curStripCount;
					}
					else
					{
						generateStrips = false;
					}
				}
			}
		}
		
		// Precomputed how many triangles we need instead
		if (!generateStrips)
		{
			foreach( MeshInstance combine in combines )
			{
				if (combine.mesh)
				{
					triangleCount += combine.mesh.GetTriangles(combine.subMeshIndex).Length;
				}
			}
		}
		
		Vector3[] vertices = new Vector3[vertexCount] ;
		Vector3[] normals = new Vector3[vertexCount] ;
		Vector4[] tangents = new Vector4[vertexCount] ;
		Vector2[] uv = new Vector2[vertexCount];
		Vector2[] uv1 = new Vector2[vertexCount];
		Color[] colors = new Color[vertexCount];
		
		int[] triangles = new int[triangleCount];
		int[] strip = new int[stripCount];
		
		int offset;
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.vertices, vertices, ref offset, combine.transform);
		}

		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				Matrix4x4 invTranspose = combine.transform;
				invTranspose = invTranspose.inverse.transpose;
				CopyNormal(combine.mesh.vertexCount, combine.mesh.normals, normals, ref offset, invTranspose);
			}
				
		}
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				Matrix4x4 invTranspose = combine.transform;
				invTranspose = invTranspose.inverse.transpose;
				CopyTangents(combine.mesh.vertexCount, combine.mesh.tangents, tangents, ref offset, invTranspose);
			}
				
		}
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.uv, uv, ref offset);
		}
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.uv2, uv1, ref offset);
		}
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				CopyColors(combine.mesh.vertexCount, combine.mesh.colors, colors, ref offset);
		}
		
		int triangleOffset=0;
		int stripOffset=0;
		int vertexOffset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				if (generateStrips)
				{
					int[] inputstrip = combine.mesh.GetTriangles(combine.subMeshIndex);
					if (stripOffset != 0)
					{
						if ((stripOffset & 1) == 1)
						{
							strip[stripOffset+0] = strip[stripOffset-1];
							strip[stripOffset+1] = inputstrip[0] + vertexOffset;
							strip[stripOffset+2] = inputstrip[0] + vertexOffset;
							stripOffset+=3;
						}
						else
						{
							strip[stripOffset+0] = strip[stripOffset-1];
							strip[stripOffset+1] = inputstrip[0] + vertexOffset;
							stripOffset+=2;
						}
					}
					
					for (int i=0;i<inputstrip.Length;i++)
					{
						strip[i+stripOffset] = inputstrip[i] + vertexOffset;
					}
					stripOffset += inputstrip.Length;
				}
				else
				{
					int[]  inputtriangles = combine.mesh.GetTriangles(combine.subMeshIndex);
					for (int i=0;i<inputtriangles.Length;i++)
					{
						triangles[i+triangleOffset] = inputtriangles[i] + vertexOffset;
					}
					triangleOffset += inputtriangles.Length;
				}
				
				vertexOffset += combine.mesh.vertexCount;
			}
		}
		
		Mesh mesh = new Mesh();
		mesh.name = "Combined Mesh";
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.colors = colors;
		mesh.uv = uv;
		mesh.uv2 = uv1;
		mesh.tangents = tangents;
		if (generateStrips)
			mesh.SetTriangles(strip, 0);
		else
			mesh.triangles = triangles;
		
		return mesh;
	}
	
	static void Copy (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyPoint(src[i]);
		offset += vertexcount;
	}

	static void CopyNormal (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyVector(src[i]).normalized;
		offset += vertexcount;
	}

	static void Copy (int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
		offset += vertexcount;
	}

	static void CopyColors (int vertexcount, Color[] src, Color[] dst, ref int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
		offset += vertexcount;
	}
	
	static void CopyTangents (int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
		{
			Vector4 p4 = src[i];
			Vector3 p = new Vector3(p4.x, p4.y, p4.z);
			p = transform.MultiplyVector(p).normalized;
			dst[i+offset] = new Vector4(p.x, p.y, p.z, p4.w);
		}
			
		offset += vertexcount;
	}
}
