using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityEngine.ProBuilder
{
#if UNITY_EDITOR
	public sealed partial class ProBuilderMesh : ISerializationCallbackReceiver
#else
	public sealed partial class ProBuilderMesh
#endif
	{
#if UNITY_EDITOR
		public void OnBeforeSerialize() { }

		public void OnAfterDeserialize()
		{
			InvalidateCaches();
		}
#endif

		static HashSet<int> s_CachedHashSet = new HashSet<int>();

		/// <summary>
		/// Reset all the attribute arrays on this object.
		/// </summary>
		public void Clear()
		{
			// various editor tools expect faces & vertices to always be valid.
			// ideally we'd null everything here, but that would break a lot of existing code.
			m_Faces = new Face[0];
			m_Positions = new Vector3[0];
			m_Textures0 = new Vector2[0];
			m_Textures2 = null;
			m_Textures3 = null;
			m_Tangents = null;
			m_SharedVertices = new SharedVertex[0];
			m_SharedTextures = new SharedVertex[0];
			InvalidateSharedVertexLookup();
			InvalidateSharedTextureLookup();
			m_Colors = null;
			ClearSelection();
		}

		void OnDestroy()
		{
			// Time.frameCount is zero when loading scenes in the Editor. It's the only way I could figure to
			// differentiate between OnDestroy invoked from user delete & editor scene loading.
			if (!preserveMeshAssetOnDestroy &&
				Application.isEditor &&
				!Application.isPlaying &&
				Time.frameCount > 0)
			{
				if (meshWillBeDestroyed != null)
					meshWillBeDestroyed(this);
				else
					DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
			}
		}

		internal static ProBuilderMesh CreateInstanceWithPoints(Vector3[] positions)
		{
			if (positions.Length % 4 != 0)
			{
				Log.Warning("Invalid Geometry. Make sure vertices in are pairs of 4 (faces).");
				return null;
			}

			GameObject go = new GameObject();
			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			pb.GeometryWithPoints(positions);

			return pb;
		}

		/// <summary>
		/// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer, then initializes the ProBuilderMesh with a set of positions and faces.
		/// </summary>
		/// <param name="positions">Vertex positions array.</param>
		/// <param name="faces">Faces array.</param>
		/// <returns></returns>
		public static ProBuilderMesh Create(IEnumerable<Vector3> positions, IEnumerable<Face> faces)
		{
			GameObject go = new GameObject();
			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			pb.RebuildWithPositionsAndFaces(positions, faces);
			return pb;
		}

		/// <summary>
		/// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer, then initializes the ProBuilderMesh with a set of positions and faces.
		/// </summary>
		/// <param name="vertices">Vertex positions array.</param>
		/// <param name="faces">Faces array.</param>
		/// <param name="sharedVertices">Optional SharedVertex[] defines coincident vertices.</param>
		/// <param name="sharedTextures">Optional SharedVertex[] defines coincident texture coordinates (UV0).</param>
		/// <returns></returns>
		public static ProBuilderMesh Create(
			IList<Vertex> vertices,
			IList<Face> faces,
			IList<SharedVertex> sharedVertices = null,
			IList<SharedVertex> sharedTextures = null)
		{
			var go = new GameObject();
			var mesh = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			mesh.SetVertices(vertices);
			mesh.faces = faces;
			mesh.sharedVertices = sharedVertices;
			mesh.sharedTextures = sharedTextures != null ? sharedTextures.ToArray() : null;
			mesh.ToMesh();
			mesh.Refresh();
			return mesh;
		}

		void GeometryWithPoints(Vector3[] points)
		{
			// Wrap in faces
			Face[] f = new Face[points.Length / 4];

			for (int i = 0; i < points.Length; i += 4)
			{
				f[i / 4] = new Face(new int[6]
					{
						i + 0, i + 1, i + 2,
						i + 1, i + 3, i + 2
					},
					BuiltinMaterials.defaultMaterial,
					AutoUnwrapSettings.tile,
					0,
					-1,
					-1,
					false);
			}

			Clear();
			positions = points;
			m_Faces = f;
			m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(points);
			InvalidateSharedVertexLookup();
			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Clear all mesh attributes and reinitialize with new positions and face collections.
		/// </summary>
		/// <param name="vertices">Vertex positions array.</param>
		/// <param name="faces">Faces array.</param>
		public void RebuildWithPositionsAndFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
		{
			if (vertices == null)
				throw new ArgumentNullException("vertices");

			Clear();
			m_Positions = vertices.ToArray();
			m_Faces = faces.ToArray();
			m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(m_Positions);
			InvalidateSharedVertexLookup();
			InvalidateSharedTextureLookup();
			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Wraps ToMesh and Refresh in a single call.
		/// </summary>
		/// <seealso cref="ToMesh"/>
		/// <seealso cref="Refresh"/>
		public void Rebuild()
		{
			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Rebuild the mesh positions and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
		/// </summary>
		/// <param name="preferredTopology">Triangles and Quads are supported.</param>
		public void ToMesh(MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			Mesh m = mesh;

			// if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
			if (m != null && m.vertexCount == m_Positions.Length)
				m = mesh;
			else if (m == null)
				m = new Mesh();
			else
				m.Clear();

			m.vertices = m_Positions;
			m.uv2 = null;

			Submesh[] submeshes = Submesh.GetSubmeshes(facesInternal, preferredTopology);
			m.subMeshCount = submeshes.Length;

			for (int i = 0; i < m.subMeshCount; i++)
				m.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, i, false);

			m.name = string.Format("pb_Mesh{0}", id);

			GetComponent<MeshFilter>().sharedMesh = m;
			GetComponent<MeshRenderer>().sharedMaterials = submeshes.Select(x => x.m_Material).ToArray();
		}

		/// <summary>
		/// Deep copy the mesh attribute arrays back to itself. Useful when copy/paste creates duplicate references.
		/// </summary>
		internal void MakeUnique()
		{
			// deep copy arrays of reference types
			sharedVertices = sharedVerticesInternal;
			SetSharedTextures(sharedTextureLookup);
			facesInternal = faces.Select(x => new Face(x)).ToArray();

			// set a new UnityEngine.Mesh instance
			mesh = new Mesh();

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Copy mesh data from another mesh to self.
		/// </summary>
		/// <param name="other"></param>
		public void CopyFrom(ProBuilderMesh other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			Clear();
			positions = other.positions;
			sharedVertices = other.sharedVerticesInternal;
			SetSharedTextures(other.sharedTextureLookup);
			facesInternal = other.faces.Select(x => new Face(x)).ToArray();

			List<Vector4> uvs = new List<Vector4>();

			for (var i = 0; i < k_UVChannelCount; i++)
			{
				other.GetUVs(1, uvs);
				SetUVs(1, uvs);
			}

			tangents = other.tangents;
			colors = other.colors;
			userCollisions = other.userCollisions;
			selectable = other.selectable;
			unwrapParameters = new UnwrapParameters(other.unwrapParameters);
		}

		/// <summary>
		/// Recalculates mesh attributes: normals, collisions, UVs, tangents, and colors.
		/// </summary>
		/// <param name="mask">
		/// Optionally pass a mask to define what components are updated (UV and collisions are expensive to rebuild, and can usually be deferred til completion of task).
		/// </param>
		public void Refresh(RefreshMask mask = RefreshMask.All)
		{
			// Mesh
			if ((mask & RefreshMask.UV) > 0)
				RefreshUV(facesInternal);

			if ((mask & RefreshMask.Colors) > 0)
				RefreshColors();

			if ((mask & RefreshMask.Normals) > 0)
				RefreshNormals();

			if ((mask & RefreshMask.Tangents) > 0)
				RefreshTangents();

			if ((mask & RefreshMask.Collisions) > 0)
				RefreshCollisions();
		}

		void RefreshCollisions()
		{
			Mesh m = mesh;

			m.RecalculateBounds();

			if (!userCollisions && GetComponent<Collider>())
			{
				foreach (Collider c in gameObject.GetComponents<Collider>())
				{
					System.Type t = c.GetType();

					if (t == typeof(BoxCollider))
					{
						((BoxCollider)c).center = m.bounds.center;
						((BoxCollider)c).size = m.bounds.size;
					}
					else if (t == typeof(SphereCollider))
					{
						((SphereCollider)c).center = m.bounds.center;
						((SphereCollider)c).radius = Math.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(CapsuleCollider))
					{
						((CapsuleCollider)c).center = m.bounds.center;
						Vector2 xy = new Vector2(m.bounds.extents.x, m.bounds.extents.z);
						((CapsuleCollider)c).radius = Math.LargestValue(xy);
						((CapsuleCollider)c).height = m.bounds.size.y;
					}
					else if (t == typeof(WheelCollider))
					{
						((WheelCollider)c).center = m.bounds.center;
						((WheelCollider)c).radius = Math.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(MeshCollider))
					{
						gameObject.GetComponent<MeshCollider>().sharedMesh = null; // this is stupid.
						gameObject.GetComponent<MeshCollider>().sharedMesh = m;
					}
				}
			}
		}

		/// <summary>
		/// Returns a new unused texture group id.
		/// Will be greater than or equal to i.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		internal int GetUnusedTextureGroup(int i = 1)
		{
			while (Array.Exists(facesInternal, element => element.textureGroup == i))
				i++;

			return i;
		}

		static bool IsValidTextureGroup(int group)
		{
			return group > 0;
		}

		/// <summary>
		/// Returns a new unused element group.
		/// Will be greater than or equal to i.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		internal int UnusedElementGroup(int i = 1)
		{
			while (Array.Exists(facesInternal, element => element.elementGroup == i))
				i++;

			return i;
		}

		public void RefreshUV(IEnumerable<Face> facesToRefresh)
		{
			// If the UV array has gone out of sync with the positions array, reset all faces to Auto UV so that we can
			// correct the texture array.
			if (!HasArrays(MeshArrays.Texture0))
			{
				m_Textures0 = new Vector2[vertexCount];
				foreach (Face f in facesInternal)
					f.manualUV = false;
				facesToRefresh = facesInternal;
			}

			s_CachedHashSet.Clear();

			foreach (var face in facesToRefresh)
			{
				if (face.manualUV)
					continue;

				int textureGroup = face.textureGroup;

				if (!IsValidTextureGroup(textureGroup))
					UnwrappingUtility.Project(this, face);
				else if (!s_CachedHashSet.Add(textureGroup))
					UnwrappingUtility.ProjectTextureGroup(this, textureGroup, face.uv);
			}

			mesh.uv = m_Textures0;

			if (HasArrays(MeshArrays.Texture2))
				mesh.SetUVs(2, m_Textures2);
			if (HasArrays(MeshArrays.Texture3))
				mesh.SetUVs(3, m_Textures3);
		}

		void RefreshColors()
		{
			Mesh m = GetComponent<MeshFilter>().sharedMesh;
			m.colors = m_Colors;
		}

		/// <summary>
		/// Set the vertex colors for a @"UnityEngine.ProBuilder.Face".
		/// </summary>
		/// <param name="face">The target face.</param>
		/// <param name="color">The color to set this face's referenced vertices to.</param>
		public void SetFaceColor(Face face, Color color)
		{
			if (face == null)
				throw new ArgumentNullException("face");

			if (!HasArrays(MeshArrays.Color))
				m_Colors = ArrayUtility.Fill(Color.white, vertexCount);

			foreach (int i in face.distinctIndexes)
				m_Colors[i] = color;
		}

		void RefreshNormals()
		{
			mesh.normals = CalculateNormals();
		}

		void RefreshTangents()
		{
			CalculateTangents();
			mesh.tangents = m_Tangents;
		}

		void CalculateTangents()
		{
			// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html
			Vector3[] normals = GetNormals();

			int vc = vertexCount;
			Vector3[] tan1 = new Vector3[vc];
			Vector3[] tan2 = new Vector3[vc];

			if (!HasArrays(MeshArrays.Tangent))
				m_Tangents = new Vector4[vc];

			foreach (var face in m_Faces)
			{
				int[] triangles = face.indexesInternal;

				for (int a = 0, c = triangles.Length; a < c; a += 3)
				{
					long i1 = triangles[a + 0];
					long i2 = triangles[a + 1];
					long i3 = triangles[a + 2];

					Vector3 v1 = m_Positions[i1];
					Vector3 v2 = m_Positions[i2];
					Vector3 v3 = m_Positions[i3];

					Vector2 w1 = m_Textures0[i1];
					Vector2 w2 = m_Textures0[i2];
					Vector2 w3 = m_Textures0[i3];

					float x1 = v2.x - v1.x;
					float x2 = v3.x - v1.x;
					float y1 = v2.y - v1.y;
					float y2 = v3.y - v1.y;
					float z1 = v2.z - v1.z;
					float z2 = v3.z - v1.z;

					float s1 = w2.x - w1.x;
					float s2 = w3.x - w1.x;
					float t1 = w2.y - w1.y;
					float t2 = w3.y - w1.y;

					float r = 1.0f / (s1 * t2 - s2 * t1);

					Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
					Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

					tan1[i1] += sdir;
					tan1[i2] += sdir;
					tan1[i3] += sdir;

					tan2[i1] += tdir;
					tan2[i2] += tdir;
					tan2[i3] += tdir;
				}
			}

			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normals[a];
				Vector3 t = tan1[a];

				Vector3.OrthoNormalize(ref n, ref t);

				m_Tangents[a].x = t.x;
				m_Tangents[a].y = t.y;
				m_Tangents[a].z = t.z;
				m_Tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}
		}

		/// <summary>
		/// Calculate mesh normals without taking into account smoothing groups.
		/// </summary>
		/// <returns>A new array of the vertex normals.</returns>
		/// <seealso cref="CalculateNormals"/>
		public Vector3[] CalculateHardNormals()
		{
			Vector3[] perTriangleNormal = new Vector3[vertexCount];
			Vector3[] vertices = positionsInternal;
			Vector3[] normals = new Vector3[vertexCount];
			int[] perTriangleAvg = new int[vertexCount];
			Face[] fces = facesInternal;

			for (int faceIndex = 0, fc = fces.Length; faceIndex < fc; faceIndex++)
			{
				int[] indexes = fces[faceIndex].indexesInternal;

				for (var tri = 0; tri < indexes.Length; tri += 3)
				{
					int a = indexes[tri], b = indexes[tri + 1], c = indexes[tri + 2];

					Vector3 cross = Math.Normal(vertices[a], vertices[b], vertices[c]);
					cross.Normalize();

					perTriangleNormal[a].x += cross.x;
					perTriangleNormal[b].x += cross.x;
					perTriangleNormal[c].x += cross.x;

					perTriangleNormal[a].y += cross.y;
					perTriangleNormal[b].y += cross.y;
					perTriangleNormal[c].y += cross.y;

					perTriangleNormal[a].z += cross.z;
					perTriangleNormal[b].z += cross.z;
					perTriangleNormal[c].z += cross.z;

					perTriangleAvg[a]++;
					perTriangleAvg[b]++;
					perTriangleAvg[c]++;
				}
			}

			for (var i = 0; i < vertexCount; i++)
			{
				normals[i].x = perTriangleNormal[i].x / perTriangleAvg[i];
				normals[i].y = perTriangleNormal[i].y / perTriangleAvg[i];
				normals[i].z = perTriangleNormal[i].z / perTriangleAvg[i];
			}

			return normals;
		}

		/// <summary>
		/// Calculates the normals for a mesh, taking into account smoothing groups.
		/// </summary>
		/// <returns>A Vector3 array of the mesh normals</returns>
		public Vector3[] CalculateNormals()
		{
			Vector3[] normals = CalculateHardNormals();

			// average the soft edge faces
			int vc = vertexCount;
			int[] smoothGroup = new int[vc];
			SharedVertex[] si = sharedVerticesInternal;
			Face[] fcs = facesInternal;
			int smoothGroupMax = 24;

			// Create a lookup of each triangles smoothing group.
			foreach (var face in fcs)
			{
				foreach (int tri in face.distinctIndexesInternal)
				{
					smoothGroup[tri] = face.smoothingGroup;

					if (face.smoothingGroup >= smoothGroupMax)
						smoothGroupMax = face.smoothingGroup + 1;
				}
			}

			Vector3[] averages = new Vector3[smoothGroupMax];
			float[] counts = new float[smoothGroupMax];

			// For each sharedIndexes group (individual vertex), find vertices that are in the same smoothing
			// group and average their normals.
			for (var i = 0; i < si.Length; i++)
			{
				for (var n = 0; n < smoothGroupMax; n++)
				{
					averages[n].x = 0f;
					averages[n].y = 0f;
					averages[n].z = 0f;
					counts[n] = 0f;
				}

				var hold = sharedVertices;
				var tmp = sharedVertexLookup;

				for (var n = 0; n < si[i].Count; n++)
				{
					int index = si[i][n];
					int group = smoothGroup[index];

					// Ideally this should only continue on group == NONE, but historically negative values have also
					// been treated as no smoothing.
					if (group <= Smoothing.smoothingGroupNone ||
						(group > Smoothing.smoothRangeMax && group < Smoothing.hardRangeMax))
						continue;

					averages[group].x += normals[index].x;
					averages[group].y += normals[index].y;
					averages[group].z += normals[index].z;
					counts[group] += 1f;
				}

				for (int n = 0; n < si[i].Count; n++)
				{
					int index = si[i][n];
					int group = smoothGroup[index];

					if (group <= Smoothing.smoothingGroupNone ||
						(group > Smoothing.smoothRangeMax && group < Smoothing.hardRangeMax))
						continue;

					normals[index].x = averages[group].x / counts[group];
					normals[index].y = averages[group].y / counts[group];
					normals[index].z = averages[group].z / counts[group];

					normals[index].Normalize();
				}
			}

			return normals;
		}

		/// <summary>
		/// Find the index of a vertex index (triangle) in an IntArray[]. The index returned is called the common index, or shared index in some cases.
		/// </summary>
		/// <remarks>Aids in removing duplicate vertex indexes.</remarks>
		/// <returns>The common (or shared) index.</returns>
		internal int GetSharedVertexHandle(int vertex)
		{
			int res;

			if (m_SharedVertexLookup.TryGetValue(vertex, out res))
				return res;

			for (int i = 0; i < m_SharedVertices.Length; i++)
			{
				for (int n = 0, c = m_SharedVertices[i].Count; n < c; n++)
					if (m_SharedVertices[i][n] == vertex)
						return i;
			}

			throw new ArgumentOutOfRangeException("vertex");
		}

		internal HashSet<int> GetSharedVertexHandles(IEnumerable<int> vertices)
		{
			var lookup = sharedVertexLookup;
			HashSet<int> common = new HashSet<int>();
			foreach (var i in vertices)
				common.Add(lookup[i]);
			return common;
		}

		/// <summary>
		/// Get a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
		/// </summary>
		/// <param name="vertices">A collection of indexes relative to the mesh positions.</param>
		/// <returns>A list of all vertices that share a position with any of the passed vertices.</returns>
		/// <exception cref="ArgumentNullException">The vertices parameter may not be null.</exception>
		public List<int> GetCoincidentVertices(IEnumerable<int> vertices)
		{
			if (vertices == null)
				throw new ArgumentNullException("vertices");

			List<int> shared = new List<int>();
			GetCoincidentVertices(vertices, shared);
			return shared;
		}

		/// <summary>
		/// Populate a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
		/// </summary>
		/// <param name="vertices">A collection of indexes relative to the mesh positions.</param>
		/// <param name="coincident">A list to be cleared and populated with any vertices that are coincident.</param>
		/// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
		public void GetCoincidentVertices(IEnumerable<int> vertices, List<int> coincident)
		{
			if (vertices == null)
				throw new ArgumentNullException("vertices");

			if (coincident == null)
				throw new ArgumentNullException("coincident");

			s_CachedHashSet.Clear();
			var lookup = sharedVertexLookup;

			foreach (var v in vertices)
			{
				var common = lookup[v];

				if (s_CachedHashSet.Add(common))
					coincident.AddRange(m_SharedVertices[common]);
			}
		}

		/// <summary>
		/// Populate a list with all the vertices that are coincident to the requested vertex.
		/// </summary>
		/// <param name="vertex">An index relative to a positions array.</param>
		/// <param name="coincident">A list to be populated with all coincident vertices.</param>
		/// <exception cref="ArgumentNullException">The coincident list may not be null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The SharedVertex[] does not contain an entry for the requested vertex.</exception>
		public void GetCoincidentVertices(int vertex, List<int> coincident)
		{
			if (coincident == null)
				throw new ArgumentNullException("coincident");

			int common;

			if (!sharedVertexLookup.TryGetValue(vertex, out common))
				throw new ArgumentOutOfRangeException("vertex");

			coincident.AddRange(m_SharedVertices[common]);
		}

		/// <summary>
		/// Sets the passed vertices as being considered coincident by the ProBuilderMesh.
		/// </summary>
		/// <remarks>
		/// Note that it is up to the caller to ensure that the passed vertices are indeed sharing a position.
		/// </remarks>
		/// <param name="vertices">Returns a list of vertices to be associated as coincident.</param>
		public void SetVerticesCoincident(IEnumerable<int> vertices)
		{
			var lookup = sharedVertexLookup;
			List<int> coincident = new List<int>();
			GetCoincidentVertices(vertices, coincident);
			SharedVertex.SetCoincident(ref lookup, coincident);
			SetSharedVertices(lookup);
		}

		internal void SetTexturesCoincident(IEnumerable<int> vertices)
		{
			var lookup = sharedTextureLookup;
			SharedVertex.SetCoincident(ref lookup, vertices);
			SetSharedTextures(lookup);
		}

		internal void AddToSharedVertex(int sharedVertexHandle, int vertex)
		{
			if (sharedVertexHandle < 0 || sharedVertexHandle >= m_SharedVertices.Length)
				throw new ArgumentOutOfRangeException("sharedVertexHandle");

			m_SharedVertices[sharedVertexHandle].Add(vertex);
			InvalidateSharedVertexLookup();
		}

		internal void AddSharedVertex(SharedVertex vertex)
		{
			if (vertex == null)
				throw new ArgumentNullException("vertex");

			m_SharedVertices = m_SharedVertices.Add(vertex);
			InvalidateSharedVertexLookup();
		}
	}
}
