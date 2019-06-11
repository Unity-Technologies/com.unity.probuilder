using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    [Serializable]
    struct Triangle : IEquatable<Triangle>
    {
        [SerializeField]
        int m_A;

        [SerializeField]
        int m_B;

        [SerializeField]
        int m_C;

        public int a
        {
            get { return m_A; }
        }

        public int b
        {
            get { return m_B; }
        }

        public int c
        {
            get { return m_C; }
        }

        public IEnumerable<int> indices
        {
            get { return new[] { m_A, m_B, m_C }; }
        }

        public Triangle(int a, int b, int c)
        {
            m_A = a;
            m_B = b;
            m_C = c;
        }

        public bool Equals(Triangle other)
        {
            return m_A == other.a && m_B == other.b && m_C == other.c;
        }

        public override bool Equals(object obj)
        {
            return obj is Triangle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_A;
                hashCode = (hashCode * 397) ^ m_B;
                hashCode = (hashCode * 397) ^ m_C;
                return hashCode;
            }
        }

        public bool IsAdjacent(Triangle other)
        {
            return other.ContainsEdge(new Edge(a, b))
                || other.ContainsEdge(new Edge(b, c))
                || other.ContainsEdge(new Edge(c, a));
        }

        bool ContainsEdge(Edge edge)
        {
            if (new Edge(a, b) == edge)
                return true;
            if (new Edge(b, c) == edge)
                return true;
            return new Edge(c, a) == edge;
        }
    }
}
