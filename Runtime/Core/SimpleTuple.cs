namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// This is simplified Tuple struct that holds only two elements, and doesn't implement any operations.
    /// </summary>
    /// <typeparam name="T1">First element.</typeparam>
    /// <typeparam name="T2">Second element.</typeparam>
    public struct SimpleTuple<T1, T2>
    {
        T1 m_Item1;
        T2 m_Item2;

        /// <summary>
        /// Gets or sets the first element.
        /// </summary>
        public T1 item1
        {
            get { return m_Item1; }
            set { m_Item1 = value; }
        }

        /// <summary>
        /// Gets or sets the second element.
        /// </summary>
        public T2 item2
        {
            get { return m_Item2; }
            set { m_Item2 = value; }
        }

        /// <summary>
        /// Constructs a simplified Tuple with two elements.
        /// </summary>
        /// <param name="item1">First element.</param>
        /// <param name="item2">Second element.</param>
        public SimpleTuple(T1 item1, T2 item2)
        {
            m_Item1 = item1;
            m_Item2 = item2;
        }

        /// <summary>
        /// Returns a string that represents this Tuple.
        /// </summary>
        /// <returns>A comma-delimited string (for example `"[item1],[item2]"`).</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}", item1.ToString(), item2.ToString());
        }
    }

    /// <summary>
    /// This is simplified Tuple struct that holds only three elements, and doesn't implement any operations.
    /// </summary>
    /// <typeparam name="T1">First element.</typeparam>
    /// <typeparam name="T2">Second element.</typeparam>
    /// <typeparam name="T3">Third element.</typeparam>
    struct SimpleTuple<T1, T2, T3>
    {
        T1 m_Item1;
        T2 m_Item2;
        T3 m_Item3;

        /// <summary>
        /// Gets or sets the first element.
        /// </summary>
        public T1 item1
        {
            get { return m_Item1; }
            set { m_Item1 = value; }
        }

        /// <summary>
        /// Gets or sets the second element.
        /// </summary>
        public T2 item2
        {
            get { return m_Item2; }
            set { m_Item2 = value; }
        }

        /// <summary>
        /// Gets or sets the third element.
        /// </summary>
        public T3 item3
        {
            get { return m_Item3; }
            set { m_Item3 = value; }
        }

        /// <summary>
        /// Constructs a simplified Tuple with three elements.
        /// </summary>
        /// <param name="item1">First element.</param>
        /// <param name="item2">Second element.</param>
        /// <param name="item3">Third element.</param>
        public SimpleTuple(T1 item1, T2 item2, T3 item3)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
        }

        /// <summary>
        /// Returns a string that represents this Tuple.
        /// </summary>
        /// <returns>A comma-delimited string (for example `"[item1],[item2],[item3]"`).</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", item1.ToString(), item2.ToString(), item3.ToString());
        }
    }
}
