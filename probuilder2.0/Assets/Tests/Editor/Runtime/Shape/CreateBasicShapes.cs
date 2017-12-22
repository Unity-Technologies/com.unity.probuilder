using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using ProBuilder.Core;

namespace ProBuilder.Test
{
	public class CreateBasicShapes
	{
		const string k_MeshTemplatePath = "Assets/Tests/Editor/Runtime/Shape/Mesh Templates/";

		static void CreateBasicAndCompare(pb_ShapeType type)
		{
			pb_Object pb = pb_ShapeGenerator.CreateShape(type);

			try
			{
				Assert.IsNotNull(pb, type.ToString());
				pb_TestUtility.AssertMeshAttributesValid(pb.msh);
				Mesh template = AssetDatabase.LoadAssetAtPath<Mesh>(k_MeshTemplatePath + type.ToString() + "_Default.asset");
				Assert.IsNotNull(template, type.ToString());
				Assert.IsTrue(pb_TestUtility.AssertAreEqual(template, pb.msh), type.ToString() + " value-wise mesh comparison");
			}
			finally
			{
				Object.DestroyImmediate(pb);
			}
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
