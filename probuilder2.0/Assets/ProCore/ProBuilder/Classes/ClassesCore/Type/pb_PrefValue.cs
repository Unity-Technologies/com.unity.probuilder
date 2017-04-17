using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	/**
	 *	Describes the backing type of a pb_PrefValue.
	 */
	public enum pb_PrefValueType
	{
		Unknown,
		Integer,
		Float,
		String,
		Color,
		Bool
	}

	/**
	 *	A serializable value type that stores data for preferences.
	 */
	[Serializable]
	public class pb_PrefValue
	{
		// The type that this object is.
		private pb_PrefValueType m_Type;

		// Stored value.
		private object m_Value;

		public pb_PrefValue(int value)
		{
			m_Type = pb_PrefValueType.Integer;
			m_Value = value;
		}

		public pb_PrefValue(float value)
		{
			m_Type = pb_PrefValueType.Float;
			m_Value = value;
		}

		public pb_PrefValue(string value)
		{
			m_Type = pb_PrefValueType.String;
			m_Value = value;
		}

		public pb_PrefValue(Color value)
		{
			m_Type = pb_PrefValueType.Color;
			m_Value = value;
		}

		public pb_PrefValue(bool value)
		{
			m_Type = pb_PrefValueType.Bool;
			m_Value = value;
		}

		public pb_PrefValue(object value)
		{
			m_Type = GetType( value );
			m_Value = value;
		}

		public object Get() { return m_Value; }

		// public object Set(object value) { m_Value = value; }

		public static pb_PrefValueType GetType(object o)
		{
			if(o == null)
				return pb_PrefValueType.Unknown;

			Type t = o.GetType();

			if(t == typeof(int))
				return pb_PrefValueType.Integer;
			else if(t == typeof(float))
				return pb_PrefValueType.Float;
			else if(t == typeof(string))
				return pb_PrefValueType.String;
			else if(t == typeof(Color))
				return pb_PrefValueType.Color;
			else if(t == typeof(bool))
				return pb_PrefValueType.Bool;

			return pb_PrefValueType.Unknown;
		}
	}
}
