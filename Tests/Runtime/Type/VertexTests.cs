using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;

static class TestHashUtility
{
    public static int GetCollisionsCount<T>(IEnumerable<T> list)
    {
        IEnumerable<IGrouping<int, T>> hashes = list.GroupBy(x => x.GetHashCode(), x => x);
        int collisions = 0;

        foreach (var group in hashes)
        {
            IEnumerable<T> dist = group.Distinct();

            if (dist.Count() > 1)
                collisions += dist.Count() - 1;
        }

        return collisions;
    }
}

static class IntVectorTests
{
    const int TestIterationCount = 512;

    static Vector3 RandVec3()
    {
        return new Vector3(
            UnityEngine.Random.Range(-10f, 10f),
            UnityEngine.Random.Range(-10f, 10f),
            UnityEngine.Random.Range(-10f, 10f));
    }

    static float RandFlt()
    {
        return UnityEngine.Random.Range(0f, 100f) * .001f;
    }

    static float RandJitter()
    {
        return UnityEngine.Random.Range(-.001f, .001f);
    }

    static Vertex RandVertex()
    {
        Vertex v = new Vertex();
        v.position = RandVec3();
        v.color = new Color(RandFlt(), RandFlt(), RandFlt(), RandFlt());
        v.normal = RandVec3();
        v.tangent = (Vector4)RandVec3();
        v.uv0 = (Vector2)RandVec3();
        v.uv2 = (Vector2)RandVec3();
        v.uv3 = (Vector4)RandVec3();
        v.uv4 = (Vector4)RandVec3();
        return v;
    }

    [Test]
    public static void TestHashCollisions_IVEC3()
    {
        IntVec3[] ivec3 = ArrayUtility.Fill<IntVec3>(TestIterationCount, (i) => { return (IntVec3)RandVec3(); });
        Assert.IsTrue(TestHashUtility.GetCollisionsCount(ivec3) < TestIterationCount * .05f);
    }

    [Test]
    public static void TestVectorHashOverflow()
    {
        Vector3 over = new Vector3(((float)int.MaxValue) + 10f, 0f, 0f);
        Vector3 under = new Vector3(((float)-int.MaxValue) - 10f, 0f, 0f);
        Vector3 inf = new Vector3(Mathf.Infinity, 0f, 0f);
        Vector3 negInf = new Vector3(Mathf.NegativeInfinity, 0f, 0f);
        Vector3 nan = new Vector3(float.NaN, 0f, 0f);

        // Out-of-range components saturate and NaN quantizes to zero, so these hash codes are
        // identical on every runtime and architecture. See UUM-148935.
        Assert.AreEqual(-2146825986, VectorHash.GetHashCode(over), "Over");
        Assert.AreEqual(-2146825145, VectorHash.GetHashCode(under), "Under");
        Assert.AreEqual(-2146825986, VectorHash.GetHashCode(inf), "Inf");
        Assert.AreEqual(-2146825145, VectorHash.GetHashCode(negInf), "NegInf");
        Assert.AreEqual(VectorHash.GetHashCode(Vector3.zero), VectorHash.GetHashCode(nan), "NaN");
    }

    [Test]
    public static void TestComparison_IVEC3()
    {
        IntVec3 a = (IntVec3)RandVec3();
        IntVec3 b = (IntVec3)(a.value * 2.3f);
        IntVec3 c = (IntVec3) new Vector3(a.x, a.y + .001f, a.z);
        IntVec3 d = (IntVec3) new Vector3(a.x, a.y, a.z);

        IntVec3[] arr = ArrayUtility.Fill<IntVec3>(24, (i) => { return i % 2 == 0 ? a : (IntVec3)RandVec3(); });

        Assert.IsFalse(a == b);
        Assert.IsFalse(a == c);
        Assert.IsTrue(a == d);
        Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
        Assert.IsFalse(a.GetHashCode() == c.GetHashCode());
        Assert.IsTrue(a.GetHashCode() == d.GetHashCode());
        Assert.AreEqual(13, arr.Distinct().Count());
    }

    [Test]
    public static void TestEqualIntVec3SharesHashCode()
    {
        // IntVec3.Equals and VectorHash must quantize identically, otherwise positions that
        // ProBuilder considers coincident land in different dictionary buckets and never weld.
        for (int i = 0; i < TestIterationCount; ++i)
        {
            IntVec3 a = (IntVec3)RandVec3();
            IntVec3 b = (IntVec3)(a.value + new Vector3(RandJitter(), RandJitter(), RandJitter()));

            if (a == b)
                Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), a + " == " + b);
        }
    }

    [Test]
    public static void TestComparison_VERTEX()
    {
        Vertex a = RandVertex();
        Vertex b = RandVertex();
        Vertex c = RandVertex();
        Vertex d = new Vertex(a);

        // reference
        Assert.IsFalse(a == b);
        Assert.IsFalse(a == c);
        Assert.IsTrue(a == d);

        // hash
        Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
        Assert.IsFalse(a.GetHashCode() == c.GetHashCode());
        Assert.True(a.GetHashCode() == d.GetHashCode());

        // value
        Assert.AreNotEqual(a, b);
        Assert.AreNotEqual(a, c);
        Assert.AreEqual(a, d);

        d.normal *= 3f;
        Assert.AreNotEqual(a, d);
    }
}
