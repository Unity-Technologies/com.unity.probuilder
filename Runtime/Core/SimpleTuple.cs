namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A bare-bones Tuple class. Holds 2 items, does not implement equality, comparison, or anything else.
    /// </summary>
    /// <typeparam name="T1">First element.</typeparam>
    /// <typeparam name="T2">Second element.</typeparam>
    public struct SimpleTuple<T1, T2>
    {
        T1 m_Item1;
        T2 m_Item2;

        public T1 item1
        {
            get { return m_Item1; }
            set { m_Item1 = value; }
        }

        public T2 item2
        {
            get { return m_Item2; }
            set { m_Item2 = value; }
        }

        public SimpleTuple(T1 item1, T2 item2)
        {
            m_Item1 = item1;
            m_Item2 = item2;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", item1.ToString(), item2.ToString());
        }
    }

    /// <summary>
    /// A bare-bones Tuple class. Holds 3 items, does not implement equality, comparison, or anything else.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    struct SimpleTuple<T1, T2, T3>
    {
        T1 m_Item1;
        T2 m_Item2;
        T3 m_Item3;

        public T1 item1
        {
            get { return m_Item1; }
            set { m_Item1 = value; }
        }

        public T2 item2
        {
            get { return m_Item2; }
            set { m_Item2 = value; }
        }

        public T3 item3
        {
            get { return m_Item3; }
            set { m_Item3 = value; }
        }

        public SimpleTuple(T1 item1, T2 item2, T3 item3)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", item1.ToString(), item2.ToString(), item3.ToString());
        }
    }
}
