using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.EditorTests.Type
{
	static class TestVertex
	{
		static Vertex a = new Vertex()
		{
			position = new Vector3(.01f, .4f, .49f)
		};

		static Vertex b = new Vertex()
		{
			position = new Vector3(.01f, .4f, .49f)
		};

		[Test]
		public static void AttributesAreInitializedMissing()
		{
			var x = new Vertex()
			{
				normal = Vector3.up
			};

			Assert.IsTrue(x.HasAttribute(MeshAttributes.Normal));
			Assert.IsFalse(x.HasAttribute(MeshAttributes.Position));
			Assert.IsFalse(x.HasAttribute(MeshAttributes.Tangent));
		}

		[Test]
		public static void AssignedAttributesAreStored()
		{
			var x = new Vertex();
			Assert.IsFalse(x.HasAttribute(MeshAttributes.Position));
			x.position = Vector3.one;
			Assert.IsTrue(x.HasAttribute(MeshAttributes.Position));
		}

		[Test]
		public static void ReferenceEquality()
		{
			var dup = new Vertex(a);

			Assert.AreSame(a, a);
			Assert.AreNotSame(a, dup);
			Assert.AreEqual(a, dup);
		}

		[Test]
		public static void Equality()
		{
			Assert.AreEqual(a, b);
			var t = new Vertex(a);
			t.position = new Vector3(t.position.x + float.Epsilon, t.position.y, t.position.z);
			Assert.AreEqual(a, t);
		}

		[Test]
		public static void Addition()
		{
			var one = new Vertex() { position = Vector3.one };
			var two = new Vertex() { position = new Vector3(2f, 2f, 2f) };
			var res = one + one;
			Assert.AreEqual(two, res);
		}

		[Test]
		public static void Subtraction()
		{
			var one = new Vertex() { position = Vector3.one };
			var two = new Vertex() { position = new Vector3(2f, 2f, 2f) };
			var res = two - one;

			Assert.AreEqual(one, res);
		}
	}
}