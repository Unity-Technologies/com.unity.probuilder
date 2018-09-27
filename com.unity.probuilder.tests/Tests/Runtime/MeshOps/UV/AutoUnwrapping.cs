using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.UV
{
	static class AutoUnwrapping
	{
		[Test]
		public static void LocalSpaceOffsetAndRotate()
		{
			var shape = ShapeGenerator.CreateShape(ShapeType.Stair);

			foreach (var face in shape.faces)
			{
				AutoUnwrapSettings uv = face.uv;
				uv.offset += new Vector2(.3f, .5f);
				uv.rotation = 35f;
				face.uv = uv;
			}

			shape.ToMesh();
			shape.Refresh();

			TestUtility.AssertMeshAttributesValid(shape.mesh);

#if PB_CREATE_TEST_MESH_TEMPLATES
			TestUtility.SaveAssetTemplate(shape.mesh, shape.name);
#endif

			Mesh template = TestUtility.GetAssetTemplate<Mesh>(shape.name);
			TestUtility.AssertAreEqual(template, shape.mesh, message: shape.name);
			UObject.DestroyImmediate(shape.gameObject);
		}

		[Test]
		public static void Anchor()
		{
			var anchors = Enum.GetValues(typeof(AutoUnwrapSettings.Anchor));

			foreach (var anchor in anchors)
			{
				var shape = ShapeGenerator.CreateShape(ShapeType.Cylinder);

				foreach (var face in shape.faces)
				{
					AutoUnwrapSettings uv = face.uv;
					uv.anchor = (AutoUnwrapSettings.Anchor) anchor;
					face.uv = uv;
				}

				shape.ToMesh();
				shape.Refresh();

				var name = shape.name + "-Anchor(" + anchor + ")";
				shape.name = name;

#if PB_CREATE_TEST_MESH_TEMPLATES
				TestUtility.SaveAssetTemplate(shape.mesh, name);
#endif

				Mesh template = TestUtility.GetAssetTemplate<Mesh>(name);
				TestUtility.AssertAreEqual(template, shape.mesh, message: name);
				UObject.DestroyImmediate(shape.gameObject);
			}
		}

		[Test]
		public static void FillMode()
		{
			var fillModes = Enum.GetValues(typeof(AutoUnwrapSettings.Fill));

			foreach (var fill in fillModes)
			{
				var shape = ShapeGenerator.CreateShape(ShapeType.Sprite);
				var positions = shape.positionsInternal;

				// move it off center so that we can be sure fill/scale doesn't change the offset
				for (int i = 0; i < shape.vertexCount; i++)
				{
					var p = positions[i];
					p.x *= .7f;
					p.z *= .4f;
					p.x += 1.5f;
					p.z += 1.3f;
					positions[i] = p;
				}

				foreach (var face in shape.faces)
				{
					AutoUnwrapSettings uv = face.uv;
					uv.fill = (AutoUnwrapSettings.Fill) fill;
					face.uv = uv;
				}

				shape.ToMesh();
				shape.Refresh();

				var name = shape.name + "-Fill(" + fill + ")";
				shape.name = name;

#if PB_CREATE_TEST_MESH_TEMPLATES
				TestUtility.SaveAssetTemplate(shape.mesh, name);
#endif

				Mesh template = TestUtility.GetAssetTemplate<Mesh>(name);
				TestUtility.AssertAreEqual(template, shape.mesh, message: name);
				UObject.DestroyImmediate(shape.gameObject);
			}
		}

		[Test]
		public static void WorldSpace()
		{
			// Stair includes texture groups and non-grouped faces
			var shape = ShapeGenerator.CreateShape(ShapeType.Stair);

			foreach (var face in shape.faces)
			{
				AutoUnwrapSettings uv = face.uv;
				uv.useWorldSpace = true;
				face.uv = uv;
			}

			shape.ToMesh();
			shape.Refresh();
			shape.name += "-UV-World-Space";

#if PB_CREATE_TEST_MESH_TEMPLATES
			TestUtility.SaveAssetTemplate(shape.mesh, shape.name);
#endif

			Mesh template = TestUtility.GetAssetTemplate<Mesh>(shape.name);
			TestUtility.AssertAreEqual(template, shape.mesh, message: shape.name);
			UObject.DestroyImmediate(shape.gameObject);
		}

		[Test]
		public static void ComplicatedSettings()
		{
			// Stair includes texture groups and non-grouped faces
			var shape = ShapeGenerator.CreateShape(ShapeType.Stair);

			foreach (var face in shape.faces)
			{
				AutoUnwrapSettings uv = face.uv;
				uv.anchor = AutoUnwrapSettings.Anchor.LowerCenter;
				uv.fill = AutoUnwrapSettings.Fill.Fit;
				uv.offset = new Vector3(.2f, .2f);
				uv.rotation = 32f;
				uv.scale = new Vector2(1.2f, 3f);
				uv.swapUV = true;
				uv.useWorldSpace = false;
				face.uv = uv;
			}

			shape.ToMesh();
			shape.Refresh();
			shape.name += "-SettingGumbo";

#if PB_CREATE_TEST_MESH_TEMPLATES
			TestUtility.SaveAssetTemplate(shape.mesh, shape.name);
#endif

			Mesh template = TestUtility.GetAssetTemplate<Mesh>(shape.name);
			TestUtility.AssertAreEqual(template, shape.mesh, message: shape.name);
			UObject.DestroyImmediate(shape.gameObject);
		}

	}
}