using UnityEngine;
using NUnit.Framework;
using System;
using UnityEngine.ProBuilder;

public static class EdgeTests
{
    public class EdgeComparisonTestCase
    {
        public Edge a, b;
        public bool expected;

        public EdgeComparisonTestCase(Edge a, Edge b, bool res)
        {
            this.a = a;
            this.b = b;
            expected = res;
        }

        public override string ToString()
        {
            return $"{a} == {b} = {expected}";
        }
    }

    static readonly EdgeComparisonTestCase[] s_EdgeEqualityNonSequential = new EdgeComparisonTestCase[]
    {
        new EdgeComparisonTestCase(new Edge(0, 1), new Edge(0, 1), true),
        new EdgeComparisonTestCase(new Edge(0, 1), new Edge(1, 0), true),
        new EdgeComparisonTestCase(new Edge(0, 1), new Edge(1, 1), false),
        new EdgeComparisonTestCase(new Edge(-1, 1), new Edge(-1, 1), true),
        new EdgeComparisonTestCase(new Edge(-1, 1), new Edge(1, -1), true),
        new EdgeComparisonTestCase(new Edge(0, 0), new Edge(0, 0), true),
        new EdgeComparisonTestCase(new Edge(42901, 399928), Edge.Empty, false),
        new EdgeComparisonTestCase(new Edge(42901, 399928), new Edge(42901, 399928), true),
        new EdgeComparisonTestCase(new Edge(42901, 399928), new Edge(399928, 42901), true)
    };

    [Test]
    public static void EdgeEquals_AllowsNonSequentialEquality([ValueSource("s_EdgeEqualityNonSequential")] EdgeComparisonTestCase test)
    {
        Assert.That(test.a.Equals(test.b), Is.EqualTo(test.expected));
    }

    [Test]
    public static void EdgeEqualsOperator_AllowsNonSequentialEquality([ValueSource("s_EdgeEqualityNonSequential")] EdgeComparisonTestCase test)
    {
        Assert.That(test.a == test.b, Is.EqualTo(test.expected));
    }

    [Test]
    public static void EdgeHashCode_IsSameForNonSequentialIndexes([ValueSource("s_EdgeEqualityNonSequential")] EdgeComparisonTestCase test)
    {
        Assert.That(test.a.GetHashCode() == test.b.GetHashCode(), Is.EqualTo(test.expected));
    }
}
