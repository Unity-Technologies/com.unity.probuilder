using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// A set of colors for use in the color palette editor.
	/// </summary>
	[System.Serializable]
	public class pb_ColorPalette : ScriptableObject, pb_IHasDefault
	{
		/// <summary>
		/// The currently selected color.
		/// </summary>
		public Color current = Color.white;

		/// <summary>
		/// All colors in this palette.
		/// </summary>
		public List<Color> colors;

		public void SetDefaultValues()
		{
			colors = new List<Color>()
			{
				new Color(0.000f, 0.122f, 0.247f, 1f),
				new Color(0.000f, 0.455f, 0.851f, 1f),
				new Color(0.498f, 0.859f, 1.000f, 1f),
				new Color(0.224f, 0.800f, 0.800f, 1f),
				new Color(0.239f, 0.600f, 0.439f, 1f),
				new Color(0.180f, 0.800f, 0.251f, 1f),
				new Color(0.004f, 1.000f, 0.439f, 1f),
				new Color(1.000f, 0.863f, 0.000f, 1f),
				new Color(1.000f, 0.522f, 0.106f, 1f),
				new Color(1.000f, 0.255f, 0.212f, 1f),
				new Color(0.522f, 0.078f, 0.294f, 1f),
				new Color(0.941f, 0.071f, 0.745f, 1f),
				new Color(0.694f, 0.051f, 0.788f, 1f),
				new Color(0.067f, 0.067f, 0.067f, 1f),
				new Color(0.667f, 0.667f, 0.667f, 1f),
				new Color(0.867f, 0.867f, 0.867f, 1f)
			};
		}

		/// <summary>
		/// Copy this color palettes values to a new color palette.
		/// </summary>
		/// <param name="target"></param>
		public void CopyTo(pb_ColorPalette target)
		{
			target.colors = new List<Color>(colors);
		}

		public static implicit operator List<Color>(pb_ColorPalette palette)
		{
			return palette.colors;
		}

		public Color this[int i]
		{
			get { return colors[i]; }
			set { colors[i] = value; }
		}

		public int Count
		{
			get { return colors.Count; }
		}
	}
}
