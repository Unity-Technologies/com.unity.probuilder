using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A collection of settings describing how to project UV coordinates for a <see cref="Face"/>.
    /// </summary>
    [System.Serializable]
    public struct AutoUnwrapSettings
    {
        /// <summary>
        /// Creates and returns the default set of AutoUnwrapSettings.
        /// </summary>
        public static AutoUnwrapSettings defaultAutoUnwrapSettings
        {
            get
            {
                var settings = new AutoUnwrapSettings();
                settings.Reset();
                return settings;
            }
        }

        /// <summary>
        /// Defines the point from which ProBuilder performs UV transform operations.
        ///
        /// After the initial projection into 2D space, ProBuilder translates UVs to the anchor position.
        /// Next, ProBuilder applies the offset and rotation, followed by the other settings.
        /// </summary>
        public enum Anchor
        {
            /// <summary>
            /// The top left bounding point of the projected UVs is aligned with UV coordinate {0, 1}.
            /// </summary>
            UpperLeft,
            /// <summary>
            /// The center top bounding point of the projected UVs is aligned with UV coordinate {.5, 1}.
            /// </summary>
            UpperCenter,
            /// <summary>
            /// The right top bounding point of the projected UVs is aligned with UV coordinate {1, 1}.
            /// </summary>
            UpperRight,
            /// <summary>
            /// The middle left bounding point of the projected UVs is aligned with UV coordinate {0, .5}.
            /// </summary>
            MiddleLeft,
            /// <summary>
            /// The center bounding point of the projected UVs is aligned with UV coordinate {.5, .5}.
            /// </summary>
            MiddleCenter,
            /// <summary>
            /// The middle right bounding point of the projected UVs is aligned with UV coordinate {1, .5}.
            /// </summary>
            MiddleRight,
            /// <summary>
            /// The lower left bounding point of the projected UVs is aligned with UV coordinate {0, 0}.
            /// </summary>
            LowerLeft,
            /// <summary>
            /// The lower center bounding point of the projected UVs is aligned with UV coordinate {.5, 0}.
            /// </summary>
            LowerCenter,
            /// <summary>
            /// The lower right bounding point of the projected UVs is aligned with UV coordinate {1, 0}.
            /// </summary>
            LowerRight,
            /// <summary>
            /// UVs are not aligned with any projection.
            /// </summary>
            None
        }

        /// <summary>
        /// Describes how ProBuilder optionally stretches the projected UV bounds to fill the normalized coordinate space.
        /// </summary>
        public enum Fill
        {
            /// <summary>
            /// Retain original aspect ratio while resizing UV bounds to fit within a 1 unit square.
            /// </summary>
            Fit,
            /// <summary>
            /// Don't resize UV bounds.
            /// </summary>
            Tile,
            /// <summary>
            /// Don't retain aspect ratio while resizing UV bounds to fit within a 1 unit square.
            /// </summary>
            Stretch
        }

        [SerializeField]
        [FormerlySerializedAs("useWorldSpace")]
        bool m_UseWorldSpace;

        [SerializeField]
        [FormerlySerializedAs("flipU")]
        bool m_FlipU;

        [SerializeField]
        [FormerlySerializedAs("flipV")]
        bool m_FlipV;

        [SerializeField]
        [FormerlySerializedAs("swapUV")]
        bool m_SwapUV;

        [SerializeField]
        [FormerlySerializedAs("fill")]
        Fill m_Fill;

        [SerializeField]
        [FormerlySerializedAs("scale")]
        Vector2 m_Scale;

        [SerializeField]
        [FormerlySerializedAs("offset")]
        Vector2 m_Offset;

        [SerializeField]
        [FormerlySerializedAs("rotation")]
        float m_Rotation;

        [SerializeField]
        [FormerlySerializedAs("anchor")]
        Anchor m_Anchor;

        /// <summary>
        /// Gets or sets whether to transform vertex positions into world space for UV projection.
        /// By default, ProBuilder projects UVs in local (or model) coordinates.
        /// </summary>
        public bool useWorldSpace
        {
            get { return m_UseWorldSpace; }
            set { m_UseWorldSpace = value; }
        }

        /// <summary>
        /// Gets or sets whether to invert UV coordinates horizontally.
        /// </summary>
        public bool flipU
        {
            get { return m_FlipU; }
            set { m_FlipU = value; }
        }

        /// <summary>
        /// Gets or sets whether to invert UV coordinates vertically.
        /// </summary>
        public bool flipV
        {
            get { return m_FlipV; }
            set { m_FlipV = value; }
        }

        /// <summary>
        /// Gets or sets whether to exchange the U and V coordinate parameters.
        /// </summary>
        /// <example>
        /// {U, V} becomes {V, U}
        /// </example>
        public bool swapUV
        {
            get { return m_SwapUV; }
            set { m_SwapUV = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="AutoUnwrapSettings.Fill"/> mode.
        /// </summary>
        public Fill fill
        {
            get { return m_Fill; }
            set { m_Fill = value; }
        }

        /// <summary>
        /// Gets or sets the scaling value to apply to coordinates after applying projection and anchor settings.
        /// </summary>
        public Vector2 scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        /// <summary>
        /// Gets or sets the value to add to UV coordinates after applying projection and anchor settings.
        /// </summary>
        public Vector2 offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        /// <summary>
        /// Gets or sets the amount in degrees to rotate UV coordinates clockwise.
        /// </summary>
        public float rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        /// <summary>
        /// Gets or sets the starting point to use for UV transform operations.
        /// </summary>
        public Anchor anchor
        {
            get { return m_Anchor; }
            set { m_Anchor = value; }
        }

        /// <summary>
        /// Creates a new set of AutoUnwrapSettings from the specified source object.
        /// </summary>
        /// <param name="unwrapSettings">The settings to copy to the new instance.</param>
        public AutoUnwrapSettings(AutoUnwrapSettings unwrapSettings)
        {
            m_UseWorldSpace = unwrapSettings.m_UseWorldSpace;
            m_FlipU = unwrapSettings.m_FlipU;
            m_FlipV = unwrapSettings.m_FlipV;
            m_SwapUV = unwrapSettings.m_SwapUV;
            m_Fill = unwrapSettings.m_Fill;
            m_Scale = unwrapSettings.m_Scale;
            m_Offset = unwrapSettings.m_Offset;
            m_Rotation = unwrapSettings.m_Rotation;
            m_Anchor = unwrapSettings.m_Anchor;
        }

        /// <summary>
        /// Gets a set of unwrap parameters that tiles UVs.
        /// </summary>
        public static AutoUnwrapSettings tile
        {
            get
            {
                var res = new AutoUnwrapSettings();
                res.Reset();
                return res;
            }
        }

        /// <summary>
        /// Gets a set of unwrap parameters that strectches the face texture to fill a normalized coordinate space, maintaining the aspect ratio.
        /// </summary>
        public static AutoUnwrapSettings fit
        {
            get
            {
                var res = new AutoUnwrapSettings();
                res.Reset();
                res.fill = Fill.Fit;
                return res;
            }
        }

        /// <summary>
        /// Gets a set of unwrap parameters that stretches the face texture to fill a normalized coordinate space, disregarding the aspect ratio.
        /// </summary>
        public static AutoUnwrapSettings stretch
        {
            get
            {
                var res = new AutoUnwrapSettings();
                res.Reset();
                res.fill = Fill.Stretch;
                return res;
            }
        }

        /// <summary>
        /// Resets all parameters to their default values.
        /// </summary>
        public void Reset()
        {
            m_UseWorldSpace = false;
            m_FlipU = false;
            m_FlipV = false;
            m_SwapUV = false;
            m_Fill = Fill.Tile;
            m_Scale = new Vector2(1f, 1f);
            m_Offset = new Vector2(0f, 0f);
            m_Rotation = 0f;
            m_Anchor = Anchor.None;
        }

        /// <summary>
        /// Returns a string representation of the AutoUnwrapSettings.
        /// </summary>
        /// <returns>A multi-line string containing the values for each setting.</returns>
        public override string ToString()
        {
            string str =
                "Use World Space: " + useWorldSpace + "\n" +
                "Flip U: " + flipU + "\n" +
                "Flip V: " + flipV + "\n" +
                "Swap UV: " + swapUV + "\n" +
                "Fill Mode: " + fill + "\n" +
                "Anchor: " + anchor + "\n" +
                "Scale: " + scale + "\n" +
                "Offset: " + offset + "\n" +
                "Rotation: " + rotation;
            return str;
        }
    }
}
