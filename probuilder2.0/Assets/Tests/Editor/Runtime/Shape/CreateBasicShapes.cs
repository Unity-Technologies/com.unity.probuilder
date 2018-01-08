using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.Test;

namespace ProBuilder.RuntimeTests.Shape
{
	public class CreateBasicShapes
	{
		static void CreateBasicAndCompare(pb_ShapeType type)
		{
			pb_Object pb = pb_ShapeGenerator.CreateShape(type);

#if PB_CREATE_TEST_MESH_TEMPLATES
			// save a template mesh. the mesh is saved in a the Templates folder with the path extracted from:
			// Templates/{Asset Type}/{CallingFilePathRelativeToTests}/{MethodName}/{AssetName}.asset
			// note - pb_DestroyListener will not let pb_Object destroy meshes backed by an asset, so there's no need
			// to set `dontDestroyOnDelete` in the editor.
			pb_TestUtility.SaveAssetTemplate(pb.GetComponent<MeshFilter>().sharedMesh, type.ToString());
#else

			try
			{
				Assert.IsNotNull(pb, type.ToString());
				pb_TestUtility.AssertMeshAttributesValid(pb.msh);
				// Loads an asset by name from the template path. See also pb_TestUtility.GetTemplatePath
				Mesh template = pb_TestUtility.GetAssetTemplate<Mesh>(type.ToString());
				Assert.IsTrue(pb_TestUtility.AssertAreEqual(template, pb.msh), type.ToString() + " value-wise mesh comparison");
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
			CreateBasicAndCompare(pb_ShapeType.Cube);
		}

		[Test]
		public static void Stair()
		{
			CreateBasicAndCompare(pb_ShapeType.Stair);
		}

		[Test]
		public static void CurvedStair()
		{
			CreateBasicAndCompare(pb_ShapeType.CurvedStair);
		}

		[Test]
		public static void Prism()
		{
			CreateBasicAndCompare(pb_ShapeType.Prism);
		}

		[Test]
		public static void Cylinder()
		{
			CreateBasicAndCompare(pb_ShapeType.Cylinder);
		}

		[Test]
		public static void Plane()
		{
			CreateBasicAndCompare(pb_ShapeType.Plane);
		}

		[Test]
		public static void Door()
		{
			CreateBasicAndCompare(pb_ShapeType.Door);
		}

		[Test]
		public static void Pipe()
		{
			CreateBasicAndCompare(pb_ShapeType.Pipe);
		}

		[Test]
		public static void Cone()
		{
			CreateBasicAndCompare(pb_ShapeType.Cone);
		}

		[Test]
		public static void Sprite()
		{
			CreateBasicAndCompare(pb_ShapeType.Sprite);
		}

		[Test]
		public static void Arch()
		{
			CreateBasicAndCompare(pb_ShapeType.Arch);
		}

		[Test]
		public static void Icosahedron()
		{
			CreateBasicAndCompare(pb_ShapeType.Icosahedron);
		}

		[Test]
		public static void Torus()
		{
			CreateBasicAndCompare(pb_ShapeType.Torus);
		}
	}
}
