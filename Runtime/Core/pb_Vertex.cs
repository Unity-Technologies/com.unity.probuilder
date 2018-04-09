using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// Holds information about a single vertex, and provides methods for averaging between many.
	/// <remarks>A vertex is only required to contain position, all other values are optional.</remarks>
	/// </summary>
	public class pb_Vertex : System.IEquatable<pb_Vertex>
	{
		public Vector3 position;
		public Color color;
		public Vector3 normal;
		public Vector4 tangent;
		public Vector2 uv0;
		public Vector2 uv2;
		public Vector4 uv3;
		public Vector4 uv4;

		public bool hasPosition	= false;
		public bool hasColor	= false;
		public bool hasNormal	= false;
		public bool hasTangent	= false;
		public bool hasUv0		= false;
		public bool hasUv2		= false;
		public bool hasUv3		= false;
		public bool hasUv4		= false;

		public pb_Vertex(bool hasAllValues = false)
		{
			hasPosition = hasAllValues;
			hasColor = hasAllValues;
			hasNormal = hasAllValues;
			hasTangent = hasAllValues;
			hasUv0 = hasAllValues;
			hasUv2 = hasAllValues;
			hasUv3 = hasAllValues;
			hasUv4 = hasAllValues;
		}

		public override bool Equals(object other)
		{
			return other is pb_Vertex && this.Equals(other as pb_Vertex);
		}

		public bool Equals(pb_Vertex other)
		{
			if(other == null)
				return false;

			return 	pb_Math.Approx3(position, other.position) &&
					pb_Math.ApproxC(color, other.color) &&
					pb_Math.Approx3(normal, other.normal) &&
					pb_Math.Approx4(tangent, other.tangent) &&
					pb_Math.Approx2(uv0, other.uv0) &&
					pb_Math.Approx2(uv2, other.uv2) &&
					pb_Math.Approx4(uv3, other.uv3) &&
					pb_Math.Approx4(uv4, other.uv4);
		}

		// GetHashCode creates a new hashcode from position, uv0, and normal since those are the values most likely to be different.
		public override int GetHashCode()
		{
			// 783 is 27 * 29
			unchecked
			{
				int hash = 783 + pb_Vector.GetHashCode(position);
				hash = hash * 29 + pb_Vector.GetHashCode(uv0);
				hash = hash * 31 + pb_Vector.GetHashCode(normal);
				return hash;
			}
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="v"></param>
		public pb_Vertex(pb_Vertex v)
		{
			this.position 	= v.position;
			this.hasPosition = v.hasPosition;
			this.color 		= v.color;
			this.hasColor 	= v.hasColor;
			this.uv0 		= v.uv0;
			this.hasUv0 	= v.hasUv0;
			this.normal 	= v.normal;
			this.hasNormal 	= v.hasNormal;
			this.tangent 	= v.tangent;
			this.hasTangent = v.hasTangent;
			this.uv2 		= v.uv2;
			this.hasUv2 	= v.hasUv2;
			this.uv3 		= v.uv3;
			this.hasUv3 	= v.hasUv3;
			this.uv4 		= v.uv4;
			this.hasUv4 	= v.hasUv4;
		}

		/// <summary>
		/// Addition operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static pb_Vertex operator +(pb_Vertex a, pb_Vertex b)
		{
			pb_Vertex v = new pb_Vertex(a);
			v.Add(b);
			return v;
		}

		/// <summary>
		/// In-place addition.
		/// </summary>
		/// <param name="b"></param>
		public void Add(pb_Vertex b)
		{
			this.position	+= b.position;
			this.color		+= b.color;
			this.normal		+= b.normal;
			this.tangent	+= b.tangent;
			this.uv0		+= b.uv0;
			this.uv2		+= b.uv2;
			this.uv3		+= b.uv3;
			this.uv4		+= b.uv4;
		}

		/// <summary>
		/// Subtraction operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static pb_Vertex operator -(pb_Vertex a, pb_Vertex b)
		{
			pb_Vertex v = new pb_Vertex(a);
			v.Subtract(b);
			return v;
		}

		/// <summary>
		/// In-place subtraction.
		/// </summary>
		/// <param name="b"></param>
		public void Subtract(pb_Vertex b)
		{
			this.position	-= b.position;
			this.color		-= b.color;
			this.normal		-= b.normal;
			this.tangent	-= b.tangent;
			this.uv0		-= b.uv0;
			this.uv2		-= b.uv2;
			this.uv3		-= b.uv3;
			this.uv4		-= b.uv4;
		}

		/// <summary>
		/// Multiplication operator overload passes * float to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static pb_Vertex operator *(pb_Vertex a, float value)
		{
			pb_Vertex v = new pb_Vertex(a);
			v.Multiply(value);
			return v;
		}

		/// <summary>
		/// In place multiplication.
		/// </summary>
		/// <param name="value"></param>
		public void Multiply(float value)
		{
			this.position	*= value;
			this.color		*= value;
			this.normal		*= value;
			this.tangent	*= value;
			this.uv0		*= value;
			this.uv2		*= value;
			this.uv3		*= value;
			this.uv4		*= value;
		}

		/// <summary>
		/// Division operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static pb_Vertex operator /(pb_Vertex a, float value)
		{
			pb_Vertex v = new pb_Vertex(a);
			v.Divide(value);
			return v;
		}

		/// <summary>
		/// In place division.
		/// </summary>
		/// <param name="value"></param>
		public void Divide(float value)
		{
			this.position	/= value;
			this.color		/= value;
			this.normal		/= value;
			this.tangent	/= value;
			this.uv0		/= value;
			this.uv2		/= value;
			this.uv3		/= value;
			this.uv4		/= value;
		}

		/// <summary>
		/// Normalize vector values in place.
		/// </summary>
		public void Normalize()
		{
			position.Normalize();
			Vector4 color4 = (Vector4) color;
			color4.Normalize();
			color.r = color4.x;
			color.g = color4.y;
			color.b = color4.z;
			color.a = color4.w;
			normal.Normalize();
			tangent.Normalize();
			uv0.Normalize();
			uv2.Normalize();
			uv3.Normalize();
			uv4.Normalize();
		}

		public override string ToString()
		{
			return this.position.ToString();
		}

		/// <summary>
		/// Creates a new array of pb_Vertex with the provide pb_Object data.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public static pb_Vertex[] GetVertices(pb_Object pb, IList<int> indices = null)
		{
			int meshVertexCount = pb.vertexCount;
			int vertexCount = indices != null ? indices.Count : pb.vertexCount;

			pb_Vertex[] v = new pb_Vertex[vertexCount];

			Vector3[] positions = pb.vertices;
			Color[] colors 		= pb.colors;
			Vector2[] uv0s 		= pb.uv;

			Vector3[] normals 	= pb.msh.normals;
			Vector4[] tangents 	= pb.msh.tangents;
			Vector2[] uv2s 		= pb.msh.uv2;

			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();

			pb.GetUVs(2, uv3s);
			pb.GetUVs(3, uv4s);

			bool _hasPositions	= positions != null && positions.Count() == meshVertexCount;
			bool _hasColors		= colors != null 	&& colors.Count() == meshVertexCount;
			bool _hasNormals	= normals != null 	&& normals.Count() == meshVertexCount;
			bool _hasTangents	= tangents != null 	&& tangents.Count() == meshVertexCount;
			bool _hasUv0		= uv0s != null 		&& uv0s.Count() == meshVertexCount;
			bool _hasUv2		= uv2s != null 		&& uv2s.Count() == meshVertexCount;
			bool _hasUv3		= uv3s != null 		&& uv3s.Count() == meshVertexCount;
			bool _hasUv4		= uv4s != null 		&& uv4s.Count() == meshVertexCount;

			for(int i = 0; i < vertexCount; i++)
			{
				v[i] = new pb_Vertex();
				int ind = indices == null ? i : indices[i];

				if( _hasPositions )	{ v[i].hasPosition = true; 	v[i].position 	= positions[ind];	 }
				if( _hasColors ) 	{ v[i].hasColor = true;	 	v[i].color 		= colors[ind];		 }
				if( _hasNormals ) 	{ v[i].hasNormal = true;	v[i].normal 	= normals[ind];		 }
				if( _hasTangents ) 	{ v[i].hasTangent = true;	v[i].tangent 	= tangents[ind];	 }
				if( _hasUv0 ) 		{ v[i].hasUv0 = true;		v[i].uv0 		= uv0s[ind];		 }
				if( _hasUv2 ) 		{ v[i].hasUv2 = true;		v[i].uv2 		= uv2s[ind];		 }
				if( _hasUv3 ) 		{ v[i].hasUv3 = true;		v[i].uv3 		= uv3s[ind];		 }
				if( _hasUv4 ) 		{ v[i].hasUv4 = true;		v[i].uv4 		= uv4s[ind];		 }
			}

			return v;
		}

		/// <summary>
		/// Creates a new array of pb_Vertex with the provide pb_Object data.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static pb_Vertex[] GetVertices(Mesh m)
		{
			if(m == null)
				return null;

			int vertexCount = m.vertexCount;
			pb_Vertex[] v = new pb_Vertex[vertexCount];

			Vector3[] positions = m.vertices;
			Color[] colors 		= m.colors;
			Vector3[] normals 	= m.normals;
			Vector4[] tangents 	= m.tangents;
			Vector2[] uv0s 		= m.uv;
			Vector2[] uv2s 		= m.uv2;
#if !UNITY_4_7 && !UNITY_5_0
			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();
			m.GetUVs(2, uv3s);
			m.GetUVs(3, uv4s);
#endif

			bool _hasPositions	= positions != null && positions.Count() == vertexCount;
			bool _hasColors		= colors != null 	&& colors.Count() == vertexCount;
			bool _hasNormals	= normals != null 	&& normals.Count() == vertexCount;
			bool _hasTangents	= tangents != null 	&& tangents.Count() == vertexCount;
			bool _hasUv0		= uv0s != null 		&& uv0s.Count() == vertexCount;
			bool _hasUv2		= uv2s != null 		&& uv2s.Count() == vertexCount;
#if !UNITY_4_7 && !UNITY_5_0
			bool _hasUv3		= uv3s != null 		&& uv3s.Count() == vertexCount;
			bool _hasUv4		= uv4s != null 		&& uv4s.Count() == vertexCount;
#endif

			for(int i = 0; i < vertexCount; i++)
			{
				v[i] = new pb_Vertex();

				if( _hasPositions )	{ v[i].hasPosition = true; v[i].position = positions[i]; }
				if( _hasColors ) 	{ v[i].hasColor = true; v[i].color = colors[i]; }
				if( _hasNormals ) 	{ v[i].hasNormal = true; v[i].normal = normals[i]; }
				if( _hasTangents ) 	{ v[i].hasTangent = true; v[i].tangent = tangents[i]; }
				if( _hasUv0 ) 		{ v[i].hasUv0 = true; v[i].uv0 = uv0s[i]; }
				if( _hasUv2 ) 		{ v[i].hasUv2 = true; v[i].uv2 = uv2s[i]; }
#if !UNITY_4_7 && !UNITY_5_0
				if( _hasUv3 ) 		{ v[i].hasUv3 = true; v[i].uv3 = uv3s[i]; }
				if( _hasUv4 ) 		{ v[i].hasUv4 = true; v[i].uv4 = uv4s[i]; }
#endif
			}

			return v;
		}

		/// <summary>
		/// Allocate and fill all mesh arrays.  This method will fill all arrays, regardless of whether
		/// or not real data populates the values (check with hasPosition, hasNormal, etc). If you are using
		/// this function to rebuild a mesh use SetMesh instead, as that method handles setting null
		/// arrays where appropriate for you.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="uv0"></param>
		/// <param name="normal"></param>
		/// <param name="tangent"></param>
		/// <param name="uv2"></param>
		/// <param name="uv3"></param>
		/// <param name="uv4"></param>
		public static void GetArrays(	IList<pb_Vertex> vertices,
										out Vector3[] position,
										out Color[] color,
										out Vector2[] uv0,
										out Vector3[] normal,
										out Vector4[] tangent,
										out Vector2[] uv2,
										out List<Vector4> uv3,
										out List<Vector4> uv4)
		{
			GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, AttributeType.All);
		}

		/// <summary>
		/// Allocate and fill all mesh arrays.  This method will fill all arrays, regardless of whether
		/// or not real data populates the values (check with hasPosition, hasNormal, etc). If you are using
		/// this function to rebuild a mesh use SetMesh instead, as that method handles setting null
		/// arrays where appropriate for you.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="uv0"></param>
		/// <param name="normal"></param>
		/// <param name="tangent"></param>
		/// <param name="uv2"></param>
		/// <param name="uv3"></param>
		/// <param name="uv4"></param>
		/// <param name="attributes"></param>
		public static void GetArrays(	IList<pb_Vertex> vertices,
										out Vector3[] position,
										out Color[] color,
										out Vector2[] uv0,
										out Vector3[] normal,
										out Vector4[] tangent,
										out Vector2[] uv2,
										out List<Vector4> uv3,
										out List<Vector4> uv4,
										AttributeType attributes)
		{
			int vc = vertices.Count;

			bool hasPosition = ((attributes & AttributeType.Position) == AttributeType.Position);
			bool hasColor = ((attributes & AttributeType.Color) == AttributeType.Color);
			bool hasUv0 = ((attributes & AttributeType.UV0) == AttributeType.UV0);
			bool hasNormal = ((attributes & AttributeType.Normal) == AttributeType.Normal);
			bool hasTangent = ((attributes & AttributeType.Tangent) == AttributeType.Tangent);
			bool hasUv2 = ((attributes & AttributeType.UV1) == AttributeType.UV1);
			bool hasUv3 = ((attributes & AttributeType.UV2) == AttributeType.UV2);
			bool hasUv4 = ((attributes & AttributeType.UV3) == AttributeType.UV3);

			position 	= hasPosition ? new Vector3[vc] : null;
			color 		= hasColor ? new Color[vc] : null;
			uv0 		= hasUv0 ? new Vector2[vc] : null;
			normal 		= hasNormal ? new Vector3[vc] : null;
			tangent 	= hasTangent ? new Vector4[vc] : null;
			uv2 		= hasUv2 ? new Vector2[vc] : null;
			uv3 		= hasUv3 ? new List<Vector4>(vc) : null;
			uv4 		= hasUv4 ? new List<Vector4>(vc) : null;

			for(int i = 0; i < vc; i++)
			{
				if(hasPosition) 	position[i] = vertices[i].position;
				if(hasColor) 		color[i] 	= vertices[i].color;
				if(hasUv0) 			uv0[i] 		= vertices[i].uv0;
				if(hasNormal) 		normal[i] 	= vertices[i].normal;
				if(hasTangent) 		tangent[i] 	= vertices[i].tangent;
				if(hasUv2) 			uv2[i] 		= vertices[i].uv2;
				if(hasUv3) 			uv3.Add(vertices[i].uv3);
				if(hasUv4) 			uv4.Add(vertices[i].uv4);
			}
		}

		/// <summary>
		/// Replace mesh values with vertex array. Mesh is cleared during this function, so be sure to set the triangles after calling.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="vertices"></param>
		public static void SetMesh(Mesh m, IList<pb_Vertex> vertices)
		{
			Vector3[] positions	= null;
 			Color[] colors		= null;
 			Vector2[] uv0s		= null;
 			Vector3[] normals	= null;
 			Vector4[] tangents	= null;
 			Vector2[] uv2s		= null;
 			List<Vector4> uv3s	= null;
 			List<Vector4> uv4s	= null;

			GetArrays(vertices,	out positions,
								out colors,
								out uv0s,
								out normals,
								out tangents,
								out uv2s,
								out uv3s,
								out uv4s);

			m.Clear();

			pb_Vertex first = vertices[0];

			if(first.hasPosition)	m.vertices = positions;
			if(first.hasColor)		m.colors = colors;
			if(first.hasUv0)		m.uv = uv0s;
			if(first.hasNormal)		m.normals = normals;
			if(first.hasTangent)	m.tangents = tangents;
			if(first.hasUv2)		m.uv2 = uv2s;
#if !UNITY_4_7 && !UNITY_5_0
			if(first.hasUv3)		if(uv3s != null) m.SetUVs(2, uv3s);
			if(first.hasUv4)		if(uv4s != null) m.SetUVs(3, uv4s);
#endif
		}

		/// <summary>
		/// Average all vertices to a single vertex.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public static pb_Vertex Average(IList<pb_Vertex> vertices, IList<int> indices = null)
		{
			pb_Vertex v = new pb_Vertex();

			int vertexCount = indices != null ? indices.Count : vertices.Count;

			int normalCount = 0,
				tangentCount = 0,
				uv2Count = 0,
				uv3Count = 0,
				uv4Count = 0;

			for(int i = 0; i < vertexCount; i++)
			{
				int index = indices == null ? i : indices[i];

				v.position 	+= vertices[index].position;
				v.color 	+= vertices[index].color;
				v.uv0 		+= vertices[index].uv0;

				if(vertices[index].hasNormal) {
					normalCount++;
					v.normal += vertices[index].normal;
				}

				if(vertices[index].hasTangent) {
					tangentCount++;
					v.tangent += vertices[index].tangent;
				}

				if(vertices[index].hasUv2) {
					uv2Count++;
					v.uv2 += vertices[index].uv2;
				}

				if(vertices[index].hasUv3) {
					uv3Count++;
					v.uv3 += vertices[index].uv3;
				}

				if(vertices[index].hasUv4) {
					uv4Count++;
					v.uv4 += vertices[index].uv4;
				}
			}

			v.position 	*= (1f/vertexCount);
			v.color 	*= (1f/vertexCount);
			v.uv0 		*= (1f/vertexCount);

			v.normal 	*= (1f/normalCount);
			v.tangent 	*= (1f/tangentCount);
			v.uv2 		*= (1f/uv2Count);
			v.uv3 		*= (1f/uv3Count);
			v.uv4 		*= (1f/uv4Count);

			return v;
		}

		/// <summary>
		/// Returns a new vertex mixed between x and y.  1 is fully y, 0 is fully x.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static pb_Vertex Mix(pb_Vertex x, pb_Vertex y, float a)
		{
			float i = 1f - a;

			pb_Vertex v = new pb_Vertex();

			v.position 	= x.position * i + y.position * a;
			v.color 	= x.color * i + y.color * a;
			v.uv0 		= x.uv0 * i + y.uv0 * a;

			if(x.hasNormal && y.hasNormal)
				v.normal = x.normal * i + y.normal * a;
			else if(x.hasNormal)
				v.normal = x.normal;
			else if(y.hasNormal)
				v.normal = y.normal;

			if(x.hasTangent && y.hasTangent)
				v.tangent = x.tangent * i + y.tangent * a;
			else if(x.hasTangent)
				v.tangent = x.tangent;
			else if(y.hasTangent)
				v.tangent = y.tangent;

			if(x.hasUv2 && y.hasUv2)
				v.uv2 = x.uv2 * i + y.uv2 * a;
			else if(x.hasUv2)
				v.uv2 = x.uv2;
			else if(y.hasUv2)
				v.uv2 = y.uv2;

			if(x.hasUv3 && y.hasUv3)
				v.uv3 = x.uv3 * i + y.uv3 * a;
			else if(x.hasUv3)
				v.uv3 = x.uv3;
			else if(y.hasUv3)
				v.uv3 = y.uv3;

			if(x.hasUv4 && y.hasUv4)
				v.uv4 = x.uv4 * i + y.uv4 * a;
			else if(x.hasUv4)
				v.uv4 = x.uv4;
			else if(y.hasUv4)
				v.uv4 = y.uv4;

			return v;
		}
	}
}
