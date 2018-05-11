using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.RuntimeTests.Shape
{
	static class CreateBasicShapes
	{
		static void CreateBasicAndCompare(ShapeType type)
		{
			ProBuilderMesh pb = ShapeGenerator.CreateShape(type);

#if PB_CREATE_TEST_MESH_TEMPLATES
			// save a template mesh. the mesh is saved in a the Templates folder with the path extracted from:
			// Templates/{Asset Type}/{CallingFilePathRelativeToTests}/{MethodName}/{AssetName}.asset
			// note - pb_DestroyListener will not let pb_Object destroy meshes backed by an asset, so there's no need
			// to set `dontDestroyOnDelete` in the editor.
			TestUtility.SaveAssetTemplate(pb.GetComponent<MeshFilter>().sharedMesh, type.ToString());
#else

			try
			{
				Assert.IsNotNull(pb, type.ToString());
				TestUtility.AssertMeshAttributesValid(pb.mesh);
				// Loads an asset by name from the template path. See also pb_TestUtility.GetTemplatePath
				Mesh template = TestUtility.GetAssetTemplate<Mesh>(type.ToString());
				Assert.IsTrue(TestUtility.AssertAreEqual(template, pb.mesh), type.ToString() + " value-wise mesh comparison");
			}
			finally
			{
				Object.DestroyImmediate(pb.gameObject);
			}
#endif
		}

		[Test]
		public static void Cube()
		{
			CreateBasicAndCompare(ShapeType.Cube);
		}

		[Test]
		public static void Stair()
		{
			CreateBasicAndCompare(ShapeType.Stair);
		}

		[Test]
		public static void CurvedStair()
		{
			CreateBasicAndCompare(ShapeType.CurvedStair);
		}

		[Test]
		public static void Prism()
		{
			CreateBasicAndCompare(ShapeType.Prism);
		}

		[Test]
		public static void Cylinder()
		{
			CreateBasicAndCompare(ShapeType.Cylinder);
		}

		[Test]
		public static void Plane()
		{
			CreateBasicAndCompare(ShapeType.Plane);
		}

		[Test]
		public static void Door()
		{
			CreateBasicAndCompare(ShapeType.Door);
		}

		[Test]
		public static void Pipe()
		{
			CreateBasicAndCompare(ShapeType.Pipe);
		}

		[Test]
		public static void Cone()
		{
			CreateBasicAndCompare(ShapeType.Cone);
		}

		[Test]
		public static void Sprite()
		{
			CreateBasicAndCompare(ShapeType.Sprite);
		}

		[Test]
		public static void Arch()
		{
			CreateBasicAndCompare(ShapeType.Arch);
		}

		[Test]
		public static void Sphere()
		{
			CreateBasicAndCompare(ShapeType.Sphere);
		}

		[Test]
		public static void Torus()
		{
			CreateBasicAndCompare(ShapeType.Torus);
		}

		[Test]
		public static void MeshAttributesAreValidOnInit()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>) shapes)
				{
					Assert.NotNull(pb.positionsInternal, pb.name);
					Assert.NotNull(pb.facesInternal, pb.name);
					Assert.NotNull(pb.texturesInternal, pb.name);
					Assert.NotNull(pb.sharedIndicesInternal, pb.name);
					Assert.NotNull(pb.sharedIndicesUVInternal, pb.name);
				}
			}
		}
	}
}
