using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <inheritdoc cref="UnityEngine.ScriptableObject"/>
    /// <inheritdoc cref="IHasDefault"/>
    /// <summary>
    /// A set of colors for use in the color palette editor.
    /// </summary>
    [Serializable]
    sealed class ColorPalette : ScriptableObject, IHasDefault
    {
        /// <value>
        /// The currently selected color.
        /// </value>
        public Color current { get; set; }

        [FormerlySerializedAs("colors")]
        [SerializeField]
        List<Color> m_Colors;

        /// <value>
        /// The colors present in this palette.
        /// </value>
        public ReadOnlyCollection<Color> colors
        {
            get { return new ReadOnlyCollection<Color>(m_Colors); }
        }

        /// <summary>
        /// Set the colors in this palette.
        /// </summary>
        /// <param name="colors"></param>
        /// <exception cref="ArgumentNullException">Thrown when the colors argument is null.</exception>
        public void SetColors(IEnumerable<Color> colors)
        {
            if (colors == null)
                throw new ArgumentNullException("colors");

            m_Colors = colors.ToList();
        }

        /// <inheritdoc />
        /// <summary>
        /// Reset the colors property to a collection of default values.
        /// </summary>
        public void SetDefaultValues()
        {
            m_Colors = new List<Color>()
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
        /// Access the colors in this palette by index.
        /// </summary>
        /// <seealso cref="Count"/>
        /// <param name="i">The index to access.</param>
        /// <returns>The color at index i.</returns>
        public Color this[int i]
        {
            get { return m_Colors[i]; }
            set { m_Colors[i] = value; }
        }

        /// <value>
        /// Return the number of colors in this palette.
        /// </value>
        public int Count
        {
            get { return m_Colors.Count; }
        }
    }
}
