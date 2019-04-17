using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    static class AutoTextureUnwrappingTests
    {
        static readonly ShapeType[] offsetRotationShapes = new ShapeType[]
        {
            ShapeType.Sprite,
            ShapeType.Stair
        };

        static readonly Vector2[] offsetValues = new Vector2[]
        {
            new Vector2(.3f, .3f),
            new Vector2(-.3f, -.3f),
            new Vector2(10f, -10f)
        };

        static readonly float[] rotationValues = new float[]
        {
            0f,
            45f,
            190f
        };

        [Test]
        public static void SetOffsetAndRotate_InLocalSpace_IsAppliedToMesh(
            [ValueSource("offsetRotationShapes")] ShapeType shape,
            [ValueSource("offsetValues")] Vector2 offset,
            [ValueSource("rotationValues")] float rotation)
        {
            var mesh = ShapeGenerator.CreateShape(shape);

            Assume.That(mesh, Is.Not.Null);

            try
            {
                foreach (var face in mesh.faces)
                {
                    AutoUnwrapSettings uv = face.uv;
                    uv.offset += new Vector2(.3f, .5f);
                    uv.rotation = 35f;
                    face.uv = uv;
                }

                mesh.ToMesh();
                mesh.Refresh();

                TestUtility.AssertMeshAttributesValid(mesh.mesh);

                string templateName = shape.ToString() + "offset: " + offset.ToString() + " rotation: " + rotation;

#if PB_CREATE_TEST_MESH_TEMPLATES
                TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif

                Mesh template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
                TestUtility.AssertAreEqual(template, mesh.mesh, message: mesh.name);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh.gameObject);
            }
        }

        static AutoUnwrapSettings.Anchor[] anchorValues
        {
            get { return (AutoUnwrapSettings.Anchor[]) typeof(AutoUnwrapSettings.Anchor).GetEnumValues(); }
        }

        [Test]
        public static void SetAnchor_IsAppliedToMesh([ValueSource("anchorValues")] AutoUnwrapSettings.Anchor anchor)
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            Assume.That(mesh, Is.Not.Null);

            try
            {
                foreach (var face in mesh.faces)
                {
                    AutoUnwrapSettings uv = face.uv;
                    uv.anchor = anchor;
                    face.uv = uv;
                }

                mesh.ToMesh();
                mesh.Refresh();

                var name = mesh.name + "-Anchor(" + anchor + ")";
                mesh.name = name;

#if PB_CREATE_TEST_MESH_TEMPLATES
                TestUtility.SaveAssetTemplate(mesh.mesh, name);
#endif

                Mesh template = TestUtility.GetAssetTemplate<Mesh>(name);
                TestUtility.AssertAreEqual(template, mesh.mesh, message: name);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh.gameObject);
            }
        }

        static AutoUnwrapSettings.Fill[] fillModeValues
        {
            get { return (AutoUnwrapSettings.Fill[])typeof(AutoUnwrapSettings.Fill).GetEnumValues(); }
        }

        [Test]
        public static void SetFillMode_IsAppliedToMesh([ValueSource("fillModeValues")] AutoUnwrapSettings.Fill fill)
        {
            var shape = ShapeGenerator.CreateShape(ShapeType.Sprite);

            Assume.That(shape, Is.Not.Null);

            try
            {
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
                    uv.fill = (AutoUnwrapSettings.Fill)fill;
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
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(shape.gameObject);
            }
        }

        [Test]
        public static void SetWorldSpace_IsAppliedToMesh()
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
    }
}
