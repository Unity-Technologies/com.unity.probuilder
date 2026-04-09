using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Hue (0,360), Saturation (0,1), Value (0,1)
    /// </summary>
    sealed class HSVColor
    {
        public float h, s, v;

        public HSVColor(float h, float s, float v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
        }

        /**
         * Wikipedia colors are from 0-100 %, so this constructor includes and S, V normalizes the values.
         * modifier value that affects saturation and value, making it useful for any SV value range.
         */
        public HSVColor(float h, float s, float v, float sv_modifier)
        {
            this.h = h;
            this.s = s * sv_modifier;
            this.v = v * sv_modifier;
        }

        public static HSVColor FromRGB(Color col)
        {
            return ColorUtility.RGBtoHSV(col);
        }

        public override string ToString()
        {
            return string.Format("( {0}, {1}, {2} )", h, s, v);
        }

        public float SqrDistance(HSVColor InColor)
        {
            return (InColor.h / 360f - this.h / 360f) + (InColor.s - this.s) + (InColor.v - this.v);
        }
    }

    /// <summary>
    /// XYZ color
    /// <remarks>http://www.easyrgb.com/index.php?X=MATH&H=07#text7</remarks>
    /// </summary>
    sealed class XYZColor
    {
        public float x, y, z;

        public XYZColor(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static XYZColor FromRGB(Color col)
        {
            return ColorUtility.RGBToXYZ(col);
        }

        public static XYZColor FromRGB(float R, float G, float B)
        {
            return ColorUtility.RGBToXYZ(R, G, B);
        }

        public override string ToString()
        {
            return string.Format("( {0}, {1}, {2} )", x, y, z);
        }
    }

    /// <summary>
    /// CIE_Lab* color
    /// </summary>
    sealed class CIELabColor
    {
        public float L, a, b;

        public CIELabColor(float L, float a, float b)
        {
            this.L = L;
            this.a = a;
            this.b = b;
        }

        public static CIELabColor FromXYZ(XYZColor xyz)
        {
            return ColorUtility.XYZToCIE_Lab(xyz);
        }

        public static CIELabColor FromRGB(Color col)
        {
            XYZColor xyz = XYZColor.FromRGB(col);

            return ColorUtility.XYZToCIE_Lab(xyz);
        }

        public override string ToString()
        {
            return string.Format("( {0}, {1}, {2} )", L, a, b);
        }
    }

    /// <summary>
    /// Conversion methods for RGB, HSV, XYZ, CIE-Lab
    /// </summary>
    static class ColorUtility
    {
        /// <summary>
        /// Compare float values within Epsilon distance.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        static bool approx(float lhs, float rhs)
        {
            return Mathf.Abs(lhs - rhs) < Mathf.Epsilon;
        }

        public static Color GetColor(Vector3 vec)
        {
            vec.Normalize();
            return new Color(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z), 1f);
        }

        /// <summary>
        /// Convert RGBA color to XYZ
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static XYZColor RGBToXYZ(Color col)
        {
            return RGBToXYZ(col.r, col.g, col.b);
        }

        public static XYZColor RGBToXYZ(float r, float g, float b)
        {
            if (r > 0.04045f)
                r = Mathf.Pow(((r + 0.055f) / 1.055f), 2.4f);
            else
                r = r / 12.92f;

            if (g > 0.04045f)
                g = Mathf.Pow(((g + 0.055f) / 1.055f), 2.4f);
            else
                g = g / 12.92f;

            if (b > 0.04045f)
                b = Mathf.Pow(((b + 0.055f) / 1.055f), 2.4f);
            else
                b = b / 12.92f;

            r = r * 100f;
            g = g * 100f;
            b = b * 100f;

            // Observer. = 2°, Illuminant = D65
            float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
            float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
            float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

            return new XYZColor(x, y, z);
        }

        /// <summary>
        /// Convert XYZ color to CIE_Lab
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static CIELabColor XYZToCIE_Lab(XYZColor xyz)
        {
            float var_X = xyz.x / 95.047f;           // ref_X =  95.047   Observer= 2°, Illuminant= D65
            float var_Y = xyz.y / 100.000f;          // ref_Y = 100.000
            float var_Z = xyz.z / 108.883f;          // ref_Z = 108.883

            if (var_X > 0.008856f)
                var_X = Mathf.Pow(var_X, (1 / 3f));
            else
                var_X = (7.787f * var_X) + (16f / 116f);

            if (var_Y > 0.008856f)
                var_Y = Mathf.Pow(var_Y, (1 / 3f));
            else
                var_Y = (7.787f * var_Y) + (16f / 116f);

            if (var_Z > 0.008856f)
                var_Z = Mathf.Pow(var_Z, (1 / 3f));
            else
                var_Z = (7.787f * var_Z) + (16f / 116f);

            float L = (116f * var_Y) - 16f;
            float a = 500f * (var_X - var_Y);
            float b = 200f * (var_Y - var_Z);

            return new CIELabColor(L, a, b);
        }

        /// <summary>
        /// Calculate the euclidean distance between two Cie-Lab colors (DeltaE).
        /// http://www.easyrgb.com/index.php?X=DELT&H=03#text3
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static float DeltaE(CIELabColor lhs, CIELabColor rhs)
        {
            return Mathf.Sqrt(
                Mathf.Pow((lhs.L - rhs.L), 2) +
                Mathf.Pow((lhs.a - rhs.a), 2) +
                Mathf.Pow((lhs.b - rhs.b), 2));
        }

        /// <summary>
        /// Convert HSV to RGB.
        ///  http://www.cs.rit.edu/~ncs/color/t_convert.html
        /// r,g,b values are from 0 to 1
        /// h = [0,360], s = [0,1], v = [0,1]
        /// if s == 0, then h = -1 (undefined)
        /// </summary>
        /// <param name="hsv"></param>
        /// <returns></returns>
        public static Color HSVtoRGB(HSVColor hsv)
        {
            return HSVtoRGB(hsv.h, hsv.s, hsv.v);
        }

        /// <summary>
        /// Convert HSV color to RGB.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Color HSVtoRGB(float h, float s, float v)
        {
            float r, g, b;
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                return new Color(v, v, v, 1f);
            }
            h /= 60;            // sector 0 to 5
            i = (int)Mathf.Floor(h);
            f = h - i;          // factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:        // case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(r, g, b, 1f);
        }

        /// <summary>
        /// http://www.cs.rit.edu/~ncs/color/t_convert.html
        /// r,g,b values are from 0 to 1
        /// h = [0,360], s = [0,1], v = [0,1]
        ///     if s == 0, then h = -1 (undefined)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static HSVColor RGBtoHSV(Color color)
        {
            float h, s, v;
            float r = color.r, b = color.b, g = color.g;

            float min, max, delta;
            min = Mathf.Min(Mathf.Min(r, g), b);
            max = Mathf.Max(Mathf.Max(r, g), b);

            v = max;                // v

            delta = max - min;

            if (max != 0f)
            {
                s = delta / max;        // s
            }
            else
            {
                // r = g = b = 0        // s = 0, v is undefined
                s = 0f;
                h = 0f;
                return new HSVColor(h, s, v);
            }

            if (approx(r, max))
            {
                h = (g - b) / delta;        // between yellow & magenta
                if (float.IsNaN(h))
                    h = 0f;
            }
            else if (approx(g, max))
            {
                h = 2f + (b - r) / delta;   // between cyan & yellow
            }
            else
            {
                h = 4f + (r - g) / delta;   // between magenta & cyan
            }

            h *= 60f;                   // degrees

            if (h < 0)
                h += 360;

            return new HSVColor(h, s, v);
        }
    }
}
