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
				CompareMesh.AttributesAreValid(pb.msh);
				Mesh template = AssetDatabase.LoadAssetAtPath<Mesh>(k_MeshTemplatePath + type.ToString() + "_Default.asset");
				Assert.IsNotNull(template, type.ToString());
				Assert.IsTrue(CompareMesh.AreEqual(template, pb.msh), type.ToString() + " value-wise mesh comparison");
			}
			finally
			{
				Object.DestroyImmediate(pb);
			}
		}

		[Test]
		public static void Create_Default_Cube()
		{
			CreateBasicAndCompare(pb_ShapeType.Cube);
		}

		[Test]
		public static void Create_Default_Stair()
		{
			CreateBasicAndCompare(pb_ShapeType.Stair);
		}

		[Test]
		public static void Create_Default_CurvedStair()
		{
			CreateBasicAndCompare(pb_ShapeType.CurvedStair);
		}

		[Test]
		public static void Create_Default_Prism()
		{
			CreateBasicAndCompare(pb_ShapeType.Prism);
		}

		[Test]
		public static void Create_Default_Cylinder()
		{
			CreateBasicAndCompare(pb_ShapeType.Cylinder);
		}

		[Test]
		public static void Create_Default_Plane()
		{
			CreateBasicAndCompare(pb_ShapeType.Plane);
		}

		[Test]
		public static void Create_Default_Door()
		{
			CreateBasicAndCompare(pb_ShapeType.Door);
		}

		[Test]
		public static void Create_Default_Pipe()
		{
			CreateBasicAndCompare(pb_ShapeType.Pipe);
		}

		[Test]
		public static void Create_Default_Cone()
		{
			CreateBasicAndCompare(pb_ShapeType.Cone);
		}

		[Test]
		public static void Create_Default_Sprite()
		{
			CreateBasicAndCompare(pb_ShapeType.Sprite);
		}

		[Test]
		public static void Create_Default_Arch()
		{
			CreateBasicAndCompare(pb_ShapeType.Arch);
		}

		[Test]
		public static void Create_Default_Icosahedron()
		{
			CreateBasicAndCompare(pb_ShapeType.Icosahedron);
		}


		[Test]
		public static void Create_Default_Torus()
		{
			CreateBasicAndCompare(pb_ShapeType.Torus);
		}
	}
}
