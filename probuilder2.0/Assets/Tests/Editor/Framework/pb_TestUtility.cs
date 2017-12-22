using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UObject = UnityEngine.Object;
using ProBuilder.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.EventSystems;

namespace ProBuilder.Test
{
	static class pb_TestUtility
	{
		const string k_TemplatesDirectory = "Assets/Tests/Editor/Templates/";
		const string k_TestsDirectory = "Assets/Tests/Editor/";

		public static string TemplatesDirectory
		{
			get { return k_TemplatesDirectory; }
		}

		public static string TestsRootDirectory
		{
			get { return k_TestsDirectory; }
		}

		public static T GetAssetTemplate<T>(string name) where T : UObject
		{
			T asset = AssetDatabase.LoadAssetAtPath<T>(GetTemplatePath<T>(name));
			Assert.IsFalse(asset == null, "Failed loading asset template: " + name);
			return asset;
		}

		public static T GetAssetTemplateWithPath<T>(string pathRelativeToTemplatesDirectory) where T : UObject
		{
			T asset = AssetDatabase.LoadAssetAtPath<T>(pathRelativeToTemplatesDirectory);
			Assert.IsFalse(asset == null, "Failed loading asset template: " + pathRelativeToTemplatesDirectory);
			return asset;
		}

		public class BasicShapes : IDisposable, IEnumerable<pb_Object>
		{
			pb_Object[] m_Shapes;

			static pb_Object[] GetBasicShapes()
			{
				var shapes = Enum.GetValues(typeof(pb_ShapeType)) as pb_ShapeType[];
				pb_Object[] primitives = new pb_Object[shapes.Length];
				for (int i = 0, c = shapes.Length; i < c; i++)
				{
					primitives[i] = pb_ShapeGenerator.CreateShape(shapes[i]);
					primitives[i].msh.name = shapes[i].ToString();
				}
				return primitives;
			}

			public BasicShapes()
			{
				m_Shapes = GetBasicShapes();
			}

			public int Count { get { return m_Shapes.Length; } }

			public pb_Object this[int i]
			{
				get { return m_Shapes[i]; }
				set { m_Shapes[i] = value; }
			}

			public void Dispose()
			{
				for(int i = 0, c = m_Shapes.Length; i < c; i++)
					UObject.DestroyImmediate(m_Shapes[i].gameObject);
			}

			IEnumerator<pb_Object> IEnumerable<pb_Object>.GetEnumerator()
			{
				return ((IEnumerable<pb_Object>)m_Shapes).GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return m_Shapes.GetEnumerator();
			}
		}

		public static void AssertMeshAttributesValid(Mesh mesh)
		{
			int vertexCount = mesh.vertexCount;

			Vector3[] positions = mesh.vertices;
			Color[] colors 		= mesh.colors;
			Vector3[] normals 	= mesh.normals;
			Vector4[] tangents 	= mesh.tangents;
			Vector2[] uv0s 		= mesh.uv;
			Vector2[] uv2s 		= mesh.uv2;
			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();
			mesh.GetUVs(2, uv3s);
			mesh.GetUVs(3, uv4s);

			bool _hasPositions	= positions != null && positions.Count() == vertexCount;
			bool _hasColors		= colors != null 	&& colors.Count() == vertexCount;
			bool _hasNormals	= normals != null 	&& normals.Count() == vertexCount;
			bool _hasTangents	= tangents != null 	&& tangents.Count() == vertexCount;
			bool _hasUv0		= uv0s != null 		&& uv0s.Count() == vertexCount;
			bool _hasUv2		= uv2s != null 		&& uv2s.Count() == vertexCount;
			bool _hasUv3		= uv3s.Count() == vertexCount;
			bool _hasUv4		= uv4s.Count() == vertexCount;

			for(int i = 0; i < vertexCount; i++)
			{
				if( _hasPositions )
				{
					Assert.IsFalse(float.IsNaN(positions[i].x), "mesh attribute \"position\" is NaN");
					Assert.IsFalse(float.IsNaN(positions[i].y), "mesh attribute \"position\" is NaN");
					Assert.IsFalse(float.IsNaN(positions[i].z), "mesh attribute \"position\" is NaN");
				}

				if( _hasColors )
				{
					Assert.IsFalse(float.IsNaN(colors[i].r), "mesh attribute \"color\" is NaN");
					Assert.IsFalse(float.IsNaN(colors[i].g), "mesh attribute \"color\" is NaN");
					Assert.IsFalse(float.IsNaN(colors[i].b), "mesh attribute \"color\" is NaN");
					Assert.IsFalse(float.IsNaN(colors[i].a), "mesh attribute \"color\" is NaN");
				}

				if( _hasNormals )
				{
					Assert.IsFalse(float.IsNaN(normals[i].x), "mesh attribute \"normal\" is NaN");
					Assert.IsFalse(float.IsNaN(normals[i].y), "mesh attribute \"normal\" is NaN");
					Assert.IsFalse(float.IsNaN(normals[i].z), "mesh attribute \"normal\" is NaN");
				}

				if( _hasTangents )
				{
					Assert.IsFalse(float.IsNaN(tangents[i].x), "mesh attribute \"tangent\" is NaN");
					Assert.IsFalse(float.IsNaN(tangents[i].y), "mesh attribute \"tangent\" is NaN");
					Assert.IsFalse(float.IsNaN(tangents[i].z), "mesh attribute \"tangent\" is NaN");
					Assert.IsFalse(float.IsNaN(tangents[i].w), "mesh attribute \"tangent\" is NaN");
				}

				if( _hasUv0 )
				{
					Assert.IsFalse(float.IsNaN(uv0s[i].x), "mesh attribute \"uv0\" is NaN");
					Assert.IsFalse(float.IsNaN(uv0s[i].y), "mesh attribute \"uv0\" is NaN");
				}

				if( _hasUv2 )
				{
					Assert.IsFalse(float.IsNaN(uv2s[i].x), "mesh attribute \"uv2\" is NaN");
					Assert.IsFalse(float.IsNaN(uv2s[i].y), "mesh attribute \"uv2\" is NaN");
				}

				if( _hasUv3 )
				{
					Assert.IsFalse(float.IsNaN(uv3s[i].x), "mesh attribute \"uv3\" is NaN");
					Assert.IsFalse(float.IsNaN(uv3s[i].y), "mesh attribute \"uv3\" is NaN");
					Assert.IsFalse(float.IsNaN(uv3s[i].z), "mesh attribute \"uv3\" is NaN");
					Assert.IsFalse(float.IsNaN(uv3s[i].w), "mesh attribute \"uv3\" is NaN");
				}

				if( _hasUv4 )
				{
					Assert.IsFalse(float.IsNaN(uv4s[i].x), "mesh attribute \"uv4\" is NaN");
					Assert.IsFalse(float.IsNaN(uv4s[i].y), "mesh attribute \"uv4\" is NaN");
					Assert.IsFalse(float.IsNaN(uv4s[i].z), "mesh attribute \"uv4\" is NaN");
					Assert.IsFalse(float.IsNaN(uv4s[i].w), "mesh attribute \"uv4\" is NaN");
				}

			}

		}

		/// <summary>
		/// Compare two meshes for value-wise equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool AssertAreEqual(Mesh left, Mesh right)
		{
			int vertexCount = left.vertexCount;
			int subMeshCount = left.subMeshCount;

			Assert.AreEqual(vertexCount, right.vertexCount);
			Assert.AreEqual(subMeshCount, right.subMeshCount);

			pb_Vertex[] leftVertices = pb_Vertex.GetVertices(left);
			pb_Vertex[] rightVertices = pb_Vertex.GetVertices(right);

			for (int i = 0; i < vertexCount; i++)
				Assert.AreEqual(leftVertices[i], rightVertices[i]);

			List<int> leftIndices = new List<int>();
			List<int> rightIndices = new List<int>();

			for (int i = 0; i < subMeshCount; i++)
			{
				uint indexCount = left.GetIndexCount(i);

				Assert.AreEqual(left.GetTopology(i), right.GetTopology(i));
				Assert.AreEqual(indexCount, right.GetIndexCount(i));

				left.GetIndices(leftIndices, i);
				right.GetIndices(rightIndices, i);

				for(int n = 0; n < indexCount; n++)
					Assert.AreEqual(leftIndices[n], rightIndices[n]);
			}

			return true;
		}

		public static void SaveAssetTemplate<T>(T asset, int methodOffset = 0) where T : UObject
		{
			string templatePath = GetTemplatePath<T>(asset.name, methodOffset + 1);

//			UnityEngine.Debug.Log(relativeTemplateDir + "   " + Path.GetFileNameWithoutExtension(filePath) + "   " + methodName);
			SaveAssetTemplate(asset, templatePath);
		}

		public static string GetTemplatePath<T>(string assetName, int methodOffset = 0)
		{
			StackTrace trace = new StackTrace(1 + methodOffset, true);
			StackFrame calling = trace.GetFrame(0);

			string filePath = calling.GetFileName();

			if(string.IsNullOrEmpty(filePath))
			{
				UnityEngine.Debug.LogError(
					"Cannot generate mesh templates directory path from calling method. Please use the explicit SaveMeshTemplate overload.");
				return null;
			}

			string fullFilePath = Path.GetFullPath(filePath).Replace("\\", "/");
			string fullTestRootPath = Path.GetFullPath(TestsRootDirectory).Replace("\\", "/");
			string relativeTemplatePath = fullFilePath.Replace(fullTestRootPath, "");
			string relativeTemplateDir = Path.GetDirectoryName(relativeTemplatePath);
			string methodName = calling.GetMethod().Name;

			return string.Format("{0}/{1}/{2}/{3}/{4}.asset",
				typeof(T).ToString(),
				relativeTemplateDir,
				Path.GetFileNameWithoutExtension(filePath),
				methodName,
				assetName);
		}

		/// <summary>
		/// Path is relative to the "Tests/Templates/" directory. Optional flag disables overwriting.
		/// </summary>
		/// <param name="asset"></param>
		/// <param name="path"></param>
		public static void SaveAssetTemplate(UObject asset, string path, bool overwrite = true)
		{

			if (!path.EndsWith(".asset"))
				path += ".asset";

			string assetPath = string.Format("{0}{1}", TemplatesDirectory, path);

			UnityEngine.Debug.Log(assetPath);
		}
	}
}

