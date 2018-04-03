
namespace ProBuilder.Core
{
	/// <summary>
	/// A bare-bones Tuple class. Holds 2 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike the System Tuple pb_Tuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
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

	/// <summary>
	/// A bare-bones Tuple class. Holds 3 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike System Tuple pb_Tuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <typeparam name="T3"></typeparam>
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

	/// <summary>
	/// A bare-bones Tuple class. Holds 4 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike System Tuple pb_Tuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <typeparam name="T3"></typeparam>
	/// <typeparam name="T4"></typeparam>
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
