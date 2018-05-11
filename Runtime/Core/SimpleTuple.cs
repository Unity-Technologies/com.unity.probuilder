
namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A bare-bones Tuple class. Holds 2 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike System.Tuple, SimpleTuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1">First element.</typeparam>
	/// <typeparam name="T2">Second element.</typeparam>
	public sealed class SimpleTuple<T1, T2>
	{
		public T1 item1 { get; set; }
		public T2 item2 { get; set; }

		public SimpleTuple() {}

		public SimpleTuple(T1 item1, T2 item2)
		{
			this.item1 = item1;
			this.item2 = item2;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", item1.ToString(), item2.ToString());
		}
	}

	/// <summary>
	/// A bare-bones Tuple class. Holds 3 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike System.Tuple, SimpleTuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <typeparam name="T3"></typeparam>
	sealed class SimpleTuple<T1, T2, T3>
	{
		public T1 item1 { get; set; }
		public T2 item2 { get; set; }
		public T3 item3 { get; set; }

		public SimpleTuple() {}

		public SimpleTuple(T1 item1, T2 item2, T3 item3)
		{
			this.item1 = item1;
			this.item2 = item2;
			this.item3 = item3;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", item1.ToString(), item2.ToString(), item3.ToString());
		}
	}

	/// <summary>
	/// A bare-bones Tuple class. Holds 4 items, does not implement equality, comparison, or anything else.
	/// <remarks>Unlike System.Tuple, SimpleTuple is a class.</remarks>
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <typeparam name="T3"></typeparam>
	/// <typeparam name="T4"></typeparam>
	sealed class SimpleTuple<T1, T2, T3, T4>
	{
		public T1 item1 { get; set; }
		public T2 item2 { get; set; }
		public T3 item3 { get; set; }
		public T4 item4 { get; set; }

		public SimpleTuple() {}

		public SimpleTuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			this.item1 = item1;
			this.item2 = item2;
			this.item3 = item3;
			this.item4 = item4;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", item1.ToString(), item2.ToString(), item3.ToString(), item4.ToString());
		}
	}
}
