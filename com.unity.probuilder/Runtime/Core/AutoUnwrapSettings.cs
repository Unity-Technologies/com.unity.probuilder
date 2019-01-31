using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A collection of settings describing how to project UV coordinates for a @"UnityEngine.ProBuilder.Face".
    /// </summary>
    [System.Serializable]
    public struct AutoUnwrapSettings
    {
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
        /// The point from which UV transform operations will be performed.
        /// <br />
        /// After the initial projection into 2d space, UVs will be translated to the anchor position. Next, the offset and rotation are applied, followed by the various other settings.
        /// </summary>
        public enum Anchor
        {
            /// <summary>
            /// The top left bound of the projected UVs is aligned with UV coordinate {0, 1}.
            /// </summary>
            UpperLeft,
            /// <summary>
            /// The center top bound of the projected UVs is aligned with UV coordinate {.5, 1}.
            /// </summary>
            UpperCenter,
            /// <summary>
            /// The right top bound of the projected UVs is aligned with UV coordinate {1, 1}.
            /// </summary>
            UpperRight,
            /// <summary>
            /// The middle left bound of the projected UVs is aligned with UV coordinate {0, .5}.
            /// </summary>
            MiddleLeft,
            /// <summary>
            /// The center bounding point of the projected UVs is aligned with UV coordinate {.5, .5}.
            /// </summary>
            MiddleCenter,
            /// <summary>
            /// The middle right bound of the projected UVs is aligned with UV coordinate {1, .5}.
            /// </summary>
            MiddleRight,
            /// <summary>
            /// The lower left bound of the projected UVs is aligned with UV coordinate {0, 0}.
            /// </summary>
            LowerLeft,
            /// <summary>
            /// The lower center bound of the projected UVs is aligned with UV coordinate {.5, 0}.
            /// </summary>
            LowerCenter,
            /// <summary>
            /// The lower right bound of the projected UVs is aligned with UV coordinate {1, 0}.
            /// </summary>
            LowerRight,
            /// <summary>
            /// UVs are not aligned following projection.
            /// </summary>
            None
        }

        /// <summary>
        /// Describes how the projected UV bounds are optionally stretched to fill normalized coordinate space.
        /// </summary>
        public enum Fill
        {
            /// <summary>
            /// UV bounds are resized to fit within a 1 unit square while retaining original aspect ratio.
            /// </summary>
            Fit,
            /// <summary>
            /// UV bounds are not resized.
            /// </summary>
            Tile,
            /// <summary>
            /// UV bounds are resized to fit within a 1 unit square, not retaining aspect ratio.
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
        /// By default, UVs are project in local (or model) coordinates. Enable useWorldSpace to transform vertex positions into world space for UV projection.
        /// </summary>
        public bool useWorldSpace
        {
            get { return m_UseWorldSpace; }
            set { m_UseWorldSpace = value; }
        }

        /// <summary>
        /// When enabled UV coordinates will be inverted horizontally.
        /// </summary>
        public bool flipU
        {
            get { return m_FlipU; }
            set { m_FlipU = value; }
        }

        /// <summary>
        /// When enabled UV coordinates will be inverted vertically.
        /// </summary>
        public bool flipV
        {
            get { return m_FlipV; }
            set { m_FlipV = value; }
        }

        /// <summary>
        /// When enabled the coordinates will have their U and V parameters exchanged.
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
        /// The @"UnityEngine.ProBuilder.AutoUnwrapSettings.Fill" mode.
        /// </summary>
        public Fill fill
        {
            get { return m_Fill; }
            set { m_Fill = value; }
        }

        /// <summary>
        /// Coordinates are multiplied by this value after projection and anchor settings.
        /// </summary>
        public Vector2 scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        /// <summary>
        /// Added to UV coordinates after projection and anchor settings.
        /// </summary>
        public Vector2 offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        /// <summary>
        /// An amount in degrees that UV coordinates are to be rotated clockwise.
        /// </summary>
        public float rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        /// <summary>
        /// The starting point from which UV transform operations will be performed.
        /// </summary>
        public Anchor anchor
        {
            get { return m_Anchor; }
            set { m_Anchor = value; }
        }

        /// <summary>
        /// A copy constructor.
        /// </summary>
        /// <param name="unwrapSettings">The settings to copy to this new instance.</param>
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
        /// Get a set of unwrap parameters that tiles UVs.
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
        /// Get a set of unwrap parameters that strectches the face texture to fill a normalized coordinate space, maintaining th aspect ratio.
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
        /// Get a set of unwrap parameters that strectches the face texture to fill a normalized coordinate space, disregarding the aspect ratio.
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
        /// Resets all parameters to default values.
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
