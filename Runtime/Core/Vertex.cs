using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Holds information about a single vertex, and provides methods for averaging between many.
	/// <remarks>A vertex is only required to contain position, all other values are optional.</remarks>
	/// </summary>
	/// <inheritdoc cref="IEquatable{T}"/>
	public class Vertex : IEquatable<Vertex>
	{
		public Vector3 position { get; set; }
		public Color color { get; set; }
		public Vector3 normal { get; set; }
		public Vector4 tangent { get; set; }
		public Vector2 uv0 { get; set; }
		public Vector2 uv2 { get; set; }
		public Vector4 uv3 { get; set; }
		public Vector4 uv4 { get; set; }

		public bool hasPosition { get; set; }
		public bool hasColor { get; set; }
		public bool hasNormal { get; set; }
		public bool hasTangent { get; set; }
		public bool hasUv0 { get; set; }
		public bool hasUv2 { get; set; }
		public bool hasUv3 { get; set; }
		public bool hasUv4 { get; set; }

		public Vertex(bool hasAllValues = false)
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

		public override bool Equals(object obj)
		{
			return obj is Vertex && Equals((Vertex)obj);
		}

		public bool Equals(Vertex other)
		{
			if (ReferenceEquals(other, null))
				return false;

			return ProBuilderMath.Approx3(position, other.position) &&
				ProBuilderMath.ApproxC(color, other.color) &&
				ProBuilderMath.Approx3(normal, other.normal) &&
				ProBuilderMath.Approx4(tangent, other.tangent) &&
				ProBuilderMath.Approx2(uv0, other.uv0) &&
				ProBuilderMath.Approx2(uv2, other.uv2) &&
				ProBuilderMath.Approx4(uv3, other.uv3) &&
				ProBuilderMath.Approx4(uv4, other.uv4);
		}

		// GetHashCode creates a new hashcode from position, uv0, and normal since those are the values most likely to be different.
		public override int GetHashCode()
		{
			// 783 is 27 * 29
			unchecked
			{
				int hash = 783 + VectorHash.GetHashCode(position);
				hash = hash * 29 + VectorHash.GetHashCode(uv0);
				hash = hash * 31 + VectorHash.GetHashCode(normal);
				return hash;
			}
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="vertex"></param>
		public Vertex(Vertex vertex)
		{
			if (vertex == null)
				throw new ArgumentNullException("vertex");

			position = vertex.position;
			hasPosition = vertex.hasPosition;
			color = vertex.color;
			hasColor = vertex.hasColor;
			uv0 = vertex.uv0;
			hasUv0 = vertex.hasUv0;
			normal = vertex.normal;
			hasNormal = vertex.hasNormal;
			tangent = vertex.tangent;
			hasTangent = vertex.hasTangent;
			uv2 = vertex.uv2;
			hasUv2 = vertex.hasUv2;
			uv3 = vertex.uv3;
			hasUv3 = vertex.hasUv3;
			uv4 = vertex.uv4;
			hasUv4 = vertex.hasUv4;
		}

        public static bool operator ==(Vertex a, Vertex b)
        {
            if(object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

		/// <summary>
		/// Addition operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vertex operator +(Vertex a, Vertex b)
		{
			Vertex v = new Vertex(a);
			v.Add(b);
			return v;
		}
          
		/// <summary>
		/// In-place addition.
		/// </summary>
		/// <param name="b"></param>
		public void Add(Vertex b)
		{
			if (b == null)
				throw new ArgumentNullException("b");

			position += b.position;
			color += b.color;
			normal += b.normal;
			tangent += b.tangent;
			uv0 += b.uv0;
			uv2 += b.uv2;
			uv3 += b.uv3;
            uv4 += b.uv4;
		}

		/// <summary>
		/// Subtraction operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vertex operator -(Vertex a, Vertex b)
		{
			Vertex v = new Vertex(a);
			v.Subtract(b);
			return v;
		}

        /// <summary>
        /// Subtract two vertices and return the result.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex Subtract(Vertex a, Vertex b)
        {
            var c = new Vertex(a);
            c.Subtract(b);
            return c;
        }

        /// <summary>
        /// In-place subtraction.
        /// </summary>
        /// <param name="b"></param>
        public void Subtract(Vertex b)
		{
			if (b == null)
				throw new ArgumentNullException("b");

			position -= b.position;
			color -= b.color;
			normal -= b.normal;
			tangent -= b.tangent;
			uv0 -= b.uv0;
			uv2 -= b.uv2;
			uv3 -= b.uv3;
			uv4 -= b.uv4;
		}

		/// <summary>
		/// Multiplication operator overload passes * float to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Vertex operator *(Vertex a, float value)
		{
			Vertex v = new Vertex(a);
			v.Multiply(value);
			return v;
		}

		/// <summary>
		/// In place multiplication.
		/// </summary>
		/// <param name="value"></param>
		public void Multiply(float value)
		{
			position *= value;
			color *= value;
			normal *= value;
			tangent *= value;
			uv0 *= value;
			uv2 *= value;
			uv3 *= value;
			uv4 *= value;
		}

		/// <summary>
		/// Division operator overload passes on to each vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Vertex operator /(Vertex a, float value)
		{
			Vertex v = new Vertex(a);
			v.Divide(value);
			return v;
		}

		/// <summary>
		/// In place division.
		/// </summary>
		/// <param name="value"></param>
		public void Divide(float value)
		{
			position /= value;
			color /= value;
			normal /= value;
			tangent /= value;
			uv0 /= value;
			uv2 /= value;
			uv3 /= value;
			uv4 /= value;
		}

		/// <summary>
		/// Normalize vector values in place.
		/// </summary>
		public void Normalize()
		{
			position.Normalize();
			Vector4 color4 = color;
			color4.Normalize();
			color = color4;
			normal.Normalize();
			tangent.Normalize();
			uv0.Normalize();
			uv2.Normalize();
			uv3.Normalize();
			uv4.Normalize();
		}

		public override string ToString()
		{
			return position.ToString();
		}

		public string ToString(string args)
		{
			StringBuilder sb = new StringBuilder();
			if (hasPosition) sb.AppendLine("position: " + position.ToString(args));
			if (hasColor) sb.AppendLine("color: " + color.ToString(args));
			if (hasNormal) sb.AppendLine("normal: " + normal.ToString(args));
			if (hasTangent) sb.AppendLine("tangent: " + tangent.ToString(args));
			if (hasUv0) sb.AppendLine("uv0: " + uv0.ToString(args));
			if (hasUv2) sb.AppendLine("uv2: " + uv2.ToString(args));
			if (hasUv3) sb.AppendLine("uv3: " + uv3.ToString(args));
			if (hasUv4) sb.AppendLine("uv4: " + uv4.ToString(args));
			return sb.ToString();
		}

		/// <summary>
		/// Creates a new array of pb_Vertex with the provide pb_Object data.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="indexes"></param>
		/// <returns></returns>
		public static Vertex[] GetVertices(ProBuilderMesh mesh, IList<int> indexes = null)
		{
			if (mesh == null)
				throw new ArgumentNullException("mesh");

			int meshVertexCount = mesh.vertexCount;
			int vertexCount = indexes != null ? indexes.Count : mesh.vertexCount;

			Vertex[] v = new Vertex[vertexCount];

			Vector3[] positions = mesh.positionsInternal;
			Color[] colors = mesh.colorsInternal;
			Vector2[] uv0s = mesh.texturesInternal;

			Vector3[] normals = mesh.mesh.normals;
			Vector4[] tangents = mesh.mesh.tangents;
			Vector2[] uv2s = mesh.mesh.uv2;

			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();

			mesh.GetUVs(2, uv3s);
			mesh.GetUVs(3, uv4s);

			bool _hasPositions = positions != null && positions.Count() == meshVertexCount;
			bool _hasColors = colors != null && colors.Count() == meshVertexCount;
			bool _hasNormals = normals != null && normals.Count() == meshVertexCount;
			bool _hasTangents = tangents != null && tangents.Count() == meshVertexCount;
			bool _hasUv0 = uv0s != null && uv0s.Count() == meshVertexCount;
			bool _hasUv2 = uv2s != null && uv2s.Count() == meshVertexCount;
			bool _hasUv3 = uv3s != null && uv3s.Count() == meshVertexCount;
			bool _hasUv4 = uv4s != null && uv4s.Count() == meshVertexCount;

			for (int i = 0; i < vertexCount; i++)
			{
				v[i] = new Vertex();
				int ind = indexes == null ? i : indexes[i];

				if (_hasPositions)
				{
					v[i].hasPosition = true;
					v[i].position = positions[ind];
				}

				if (_hasColors)
				{
					v[i].hasColor = true;
					v[i].color = colors[ind];
				}

				if (_hasNormals)
				{
					v[i].hasNormal = true;
					v[i].normal = normals[ind];
				}

				if (_hasTangents)
				{
					v[i].hasTangent = true;
					v[i].tangent = tangents[ind];
				}

				if (_hasUv0)
				{
					v[i].hasUv0 = true;
					v[i].uv0 = uv0s[ind];
				}

				if (_hasUv2)
				{
					v[i].hasUv2 = true;
					v[i].uv2 = uv2s[ind];
				}

				if (_hasUv3)
				{
					v[i].hasUv3 = true;
					v[i].uv3 = uv3s[ind];
				}

				if (_hasUv4)
				{
					v[i].hasUv4 = true;
					v[i].uv4 = uv4s[ind];
				}
			}

			return v;
		}

		/// <summary>
		/// Creates a new array of pb_Vertex with the provide pb_Object data.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Vertex[] GetVertices(Mesh m)
		{
			if (m == null)
				return null;

			int vertexCount = m.vertexCount;
			Vertex[] v = new Vertex[vertexCount];

			Vector3[] positions = m.vertices;
			Color[] colors = m.colors;
			Vector3[] normals = m.normals;
			Vector4[] tangents = m.tangents;
			Vector2[] uv0s = m.uv;
			Vector2[] uv2s = m.uv2;
#if !UNITY_4_7 && !UNITY_5_0
			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();
			m.GetUVs(2, uv3s);
			m.GetUVs(3, uv4s);
#endif

			bool _hasPositions = positions != null && positions.Count() == vertexCount;
			bool _hasColors = colors != null && colors.Count() == vertexCount;
			bool _hasNormals = normals != null && normals.Count() == vertexCount;
			bool _hasTangents = tangents != null && tangents.Count() == vertexCount;
			bool _hasUv0 = uv0s != null && uv0s.Count() == vertexCount;
			bool _hasUv2 = uv2s != null && uv2s.Count() == vertexCount;
#if !UNITY_4_7 && !UNITY_5_0
			bool _hasUv3 = uv3s != null && uv3s.Count() == vertexCount;
			bool _hasUv4 = uv4s != null && uv4s.Count() == vertexCount;
#endif

			for (int i = 0; i < vertexCount; i++)
			{
				v[i] = new Vertex();

				if (_hasPositions)
				{
					v[i].hasPosition = true;
					v[i].position = positions[i];
				}

				if (_hasColors)
				{
					v[i].hasColor = true;
					v[i].color = colors[i];
				}

				if (_hasNormals)
				{
					v[i].hasNormal = true;
					v[i].normal = normals[i];
				}

				if (_hasTangents)
				{
					v[i].hasTangent = true;
					v[i].tangent = tangents[i];
				}

				if (_hasUv0)
				{
					v[i].hasUv0 = true;
					v[i].uv0 = uv0s[i];
				}

				if (_hasUv2)
				{
					v[i].hasUv2 = true;
					v[i].uv2 = uv2s[i];
				}
#if !UNITY_4_7 && !UNITY_5_0
				if (_hasUv3)
				{
					v[i].hasUv3 = true;
					v[i].uv3 = uv3s[i];
				}

				if (_hasUv4)
				{
					v[i].hasUv4 = true;
					v[i].uv4 = uv4s[i];
				}
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
		public static void GetArrays(
			IList<Vertex> vertices,
			out Vector3[] position,
			out Color[] color,
			out Vector2[] uv0,
			out Vector3[] normal,
			out Vector4[] tangent,
			out Vector2[] uv2,
			out List<Vector4> uv3,
			out List<Vector4> uv4)
		{
			GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, Attributes.All);
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
		public static void GetArrays(
			IList<Vertex> vertices,
			out Vector3[] position,
			out Color[] color,
			out Vector2[] uv0,
			out Vector3[] normal,
			out Vector4[] tangent,
			out Vector2[] uv2,
			out List<Vector4> uv3,
			out List<Vector4> uv4,
			Attributes attributes)
		{
			if (vertices == null)
				throw new ArgumentNullException("vertices");

			int vc = vertices.Count;
			var first = vertices[0];

			bool hasPosition = ((attributes & Attributes.Position) == Attributes.Position) && first.hasPosition;
			bool hasColor = ((attributes & Attributes.Color) == Attributes.Color) && first.hasColor;
			bool hasUv0 = ((attributes & Attributes.UV0) == Attributes.UV0) && first.hasUv0;
			bool hasNormal = ((attributes & Attributes.Normal) == Attributes.Normal) && first.hasNormal;
			bool hasTangent = ((attributes & Attributes.Tangent) == Attributes.Tangent) && first.hasTangent;
			bool hasUv2 = ((attributes & Attributes.UV1) == Attributes.UV1) && first.hasUv2;
			bool hasUv3 = ((attributes & Attributes.UV2) == Attributes.UV2) && first.hasUv3;
			bool hasUv4 = ((attributes & Attributes.UV3) == Attributes.UV3) && first.hasUv4;

			position = hasPosition ? new Vector3[vc] : null;
			color = hasColor ? new Color[vc] : null;
			uv0 = hasUv0 ? new Vector2[vc] : null;
			normal = hasNormal ? new Vector3[vc] : null;
			tangent = hasTangent ? new Vector4[vc] : null;
			uv2 = hasUv2 ? new Vector2[vc] : null;
			uv3 = hasUv3 ? new List<Vector4>(vc) : null;
			uv4 = hasUv4 ? new List<Vector4>(vc) : null;

			for (int i = 0; i < vc; i++)
			{
				if (hasPosition) position[i] = vertices[i].position;
				if (hasColor) color[i] = vertices[i].color;
				if (hasUv0) uv0[i] = vertices[i].uv0;
				if (hasNormal) normal[i] = vertices[i].normal;
				if (hasTangent) tangent[i] = vertices[i].tangent;
				if (hasUv2) uv2[i] = vertices[i].uv2;
				if (hasUv3) uv3.Add(vertices[i].uv3);
				if (hasUv4) uv4.Add(vertices[i].uv4);
			}
		}

		/// <summary>
		/// Replace mesh values with vertex array. Mesh is cleared during this function, so be sure to set the triangles after calling.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="vertices"></param>
		public static void SetMesh(Mesh mesh, IList<Vertex> vertices)
		{
			if (mesh == null)
				throw new ArgumentNullException("mesh");

			if (vertices == null)
				throw new ArgumentNullException("vertices");

			Vector3[] positions = null;
			Color[] colors = null;
			Vector2[] uv0s = null;
			Vector3[] normals = null;
			Vector4[] tangents = null;
			Vector2[] uv2s = null;
			List<Vector4> uv3s = null;
			List<Vector4> uv4s = null;

			GetArrays(vertices, out positions,
				out colors,
				out uv0s,
				out normals,
				out tangents,
				out uv2s,
				out uv3s,
				out uv4s);

			mesh.Clear();

			Vertex first = vertices[0];

			if (first.hasPosition) mesh.vertices = positions;
			if (first.hasColor) mesh.colors = colors;
			if (first.hasUv0) mesh.uv = uv0s;
			if (first.hasNormal) mesh.normals = normals;
			if (first.hasTangent) mesh.tangents = tangents;
			if (first.hasUv2) mesh.uv2 = uv2s;
#if !UNITY_4_7 && !UNITY_5_0
			if (first.hasUv3)
				if (uv3s != null)
					mesh.SetUVs(2, uv3s);
			if (first.hasUv4)
				if (uv4s != null)
					mesh.SetUVs(3, uv4s);
#endif
		}

		/// <summary>
		/// Average all vertices to a single vertex.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indexes"></param>
		/// <returns></returns>
		public static Vertex Average(IList<Vertex> vertices, IList<int> indexes = null)
		{
			if (vertices == null)
				throw new ArgumentNullException("vertices");

			Vertex v = new Vertex();

			int vertexCount = indexes != null ? indexes.Count : vertices.Count;

			int normalCount = 0,
				tangentCount = 0,
				uv2Count = 0,
				uv3Count = 0,
				uv4Count = 0;

			for (int i = 0; i < vertexCount; i++)
			{
				int index = indexes == null ? i : indexes[i];

				v.position += vertices[index].position;
				v.color += vertices[index].color;
				v.uv0 += vertices[index].uv0;

				if (vertices[index].hasNormal)
				{
					normalCount++;
					v.normal += vertices[index].normal;
				}

				if (vertices[index].hasTangent)
				{
					tangentCount++;
					v.tangent += vertices[index].tangent;
				}

				if (vertices[index].hasUv2)
				{
					uv2Count++;
					v.uv2 += vertices[index].uv2;
				}

				if (vertices[index].hasUv3)
				{
					uv3Count++;
					v.uv3 += vertices[index].uv3;
				}

				if (vertices[index].hasUv4)
				{
					uv4Count++;
					v.uv4 += vertices[index].uv4;
				}
			}

			v.position *= (1f / vertexCount);
			v.color *= (1f / vertexCount);
			v.uv0 *= (1f / vertexCount);

			v.normal *= (1f / normalCount);
			v.tangent *= (1f / tangentCount);
			v.uv2 *= (1f / uv2Count);
			v.uv3 *= (1f / uv3Count);
			v.uv4 *= (1f / uv4Count);

			return v;
		}

		/// <summary>
		/// Returns a new vertex mixed between x and y.  1 is fully y, 0 is fully x.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Vertex Mix(Vertex x, Vertex y, float a)
		{
			if (x == null || y == null)
				throw new ArgumentNullException("x", "Mix does accept null vertices.");

			float i = 1f - a;

			Vertex v = new Vertex();

			v.position = x.position * i + y.position * a;
			v.color = x.color * i + y.color * a;
			v.uv0 = x.uv0 * i + y.uv0 * a;

			if (x.hasNormal && y.hasNormal)
				v.normal = x.normal * i + y.normal * a;
			else if (x.hasNormal)
				v.normal = x.normal;
			else if (y.hasNormal)
				v.normal = y.normal;

			if (x.hasTangent && y.hasTangent)
				v.tangent = x.tangent * i + y.tangent * a;
			else if (x.hasTangent)
				v.tangent = x.tangent;
			else if (y.hasTangent)
				v.tangent = y.tangent;

			if (x.hasUv2 && y.hasUv2)
				v.uv2 = x.uv2 * i + y.uv2 * a;
			else if (x.hasUv2)
				v.uv2 = x.uv2;
			else if (y.hasUv2)
				v.uv2 = y.uv2;

			if (x.hasUv3 && y.hasUv3)
				v.uv3 = x.uv3 * i + y.uv3 * a;
			else if (x.hasUv3)
				v.uv3 = x.uv3;
			else if (y.hasUv3)
				v.uv3 = y.uv3;

			if (x.hasUv4 && y.hasUv4)
				v.uv4 = x.uv4 * i + y.uv4 * a;
			else if (x.hasUv4)
				v.uv4 = x.uv4;
			else if (y.hasUv4)
				v.uv4 = y.uv4;

			return v;
		}
	}
}
