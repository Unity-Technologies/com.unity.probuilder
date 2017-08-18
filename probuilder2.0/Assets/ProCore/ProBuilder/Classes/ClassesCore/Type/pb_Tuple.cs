
namespace ProBuilder2.Common
{
	/**
	 * A bare-bones Tuple class. Holds 2 items, does not implement equality, comparison, or anything else.
	 * Unlike C# Tuple pb_Tuple is a class.
	 */
	public class pb_Tuple<T1, T2>
	{
		public T1 Item1;
		public T2 Item2;

		public pb_Tuple() {}

		public pb_Tuple(T1 item1, T2 item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", Item1.ToString(), Item2.ToString());
		}
	}

	public class pb_Tuple<T1, T2, T3>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;

		public pb_Tuple() {}

		public pb_Tuple(T1 item1, T2 item2, T3 item3)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", Item1.ToString(), Item2.ToString(), Item3.ToString());
		}
	}

	public class pb_Tuple<T1, T2, T3, T4>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;
		public T4 Item4;

		public pb_Tuple() {}

		public pb_Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", Item1.ToString(), Item2.ToString(), Item3.ToString(), Item4.ToString());
		}
	}
}
