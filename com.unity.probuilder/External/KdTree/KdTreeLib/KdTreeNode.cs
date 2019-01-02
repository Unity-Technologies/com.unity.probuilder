using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.KdTree
{
	[Serializable]
	class KdTreeNode<TKey, TValue>
	{
		public KdTreeNode()
		{
		}

		public KdTreeNode(TKey[] point, TValue value)
		{
			Point = point;
			Value = value;
		}

		public TKey[] Point;
		public TValue Value = default(TValue);
		public List<TValue> Duplicates = null;

		internal KdTreeNode<TKey, TValue> LeftChild = null;
		internal KdTreeNode<TKey, TValue> RightChild = null;

		internal KdTreeNode<TKey, TValue> this[int compare]
		{
			get
			{
				if (compare <= 0)
					return LeftChild;
				else
					return RightChild;
			}
			set
			{
				if (compare <= 0)
					LeftChild = value;
				else
					RightChild = value;
			}
		}

		public bool IsLeaf
		{
			get
			{
				return (LeftChild == null) && (RightChild == null);
			}
		}

		public void AddDuplicate(TValue value)
		{
			if (Duplicates == null)
				Duplicates = new List<TValue>() { value };
			else
				Duplicates.Add(value);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			for (var dimension = 0; dimension < Point.Length; dimension++)
			{
				sb.Append(Point[dimension].ToString());
			}

			if (Value == null)
				sb.Append("null");
			else
				sb.Append(Value.ToString());

			return sb.ToString();
		}
	}
}
