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

        /// <summary>
        /// Get human readable name from a Color.
        /// </summary>
        /// <param name="InColor"></param>
        /// <returns></returns>
        public static string GetColorName(Color InColor)
        {
            CIELabColor lab = CIELabColor.FromRGB(InColor);

            string name = "Unknown";
            float diff = Mathf.Infinity;

            foreach (KeyValuePair<string, CIELabColor> kvp in ColorNameLookup)
            {
                float dist = Mathf.Abs(DeltaE(lab, kvp.Value));

                if (dist < diff)
                {
                    diff = dist;
                    name = kvp.Key;
                }
            }

            return name;
        }

        static CIELabColor CIELabFromRGB(float R, float G, float B, float Scale)
        {
            float inv_scale = 1f / Scale;
            XYZColor xyz = XYZColor.FromRGB(R * inv_scale, G * inv_scale, B * inv_scale);

            return CIELabColor.FromXYZ(xyz);
        }

        /// <summary>
        /// http://en.wikipedia.org/wiki/List_of_colors:_A%E2%80%93F
        /// </summary>
        static readonly Dictionary<string, CIELabColor> ColorNameLookup = new Dictionary<string, CIELabColor>()
        {
            { "Acid Green",                                 CIELabFromRGB(69f, 75f, 10f, 100f)      },
            { "Aero",                                       CIELabFromRGB(49f, 73f, 91f, 100f)      },
            { "Aero Blue",                                  CIELabFromRGB(79f, 100f, 90f, 100f)     },
            { "African Violet",                             CIELabFromRGB(70f, 52f, 75f, 100f)      },
            { "Air Force Blue (RAF)",                       CIELabFromRGB(36f, 54f, 66f, 100f)      },
            { "Air Force Blue (USAF)",                      CIELabFromRGB(0f, 19f, 56f, 100f)       },
            { "Air Superiority Blue",                       CIELabFromRGB(45f, 63f, 76f, 100f)      },
            { "Alabama Crimson",                            CIELabFromRGB(69f, 0f, 16f, 100f)       },
            { "Alice Blue",                                 CIELabFromRGB(94f, 97f, 100f, 100f)     },
            { "Alizarin Crimson",                           CIELabFromRGB(89f, 15f, 21f, 100f)      },
            { "Alloy Orange",                               CIELabFromRGB(77f, 38f, 6f, 100f)       },
            { "Almond",                                     CIELabFromRGB(94f, 87f, 80f, 100f)      },
            { "Amaranth",                                   CIELabFromRGB(90f, 17f, 31f, 100f)      },
            { "Amaranth Deep Purple",                       CIELabFromRGB(67f, 15f, 31f, 100f)      },
            { "Amaranth Pink",                              CIELabFromRGB(95f, 61f, 73f, 100f)      },
            { "Amaranth Purple",                            CIELabFromRGB(67f, 15f, 31f, 100f)      },
            { "Amaranth Red",                               CIELabFromRGB(83f, 13f, 18f, 100f)      },
            { "Amazon",                                     CIELabFromRGB(23f, 48f, 34f, 100f)      },
            { "Amber",                                      CIELabFromRGB(100f, 75f, 0f, 100f)      },
            { "Amber (SAE/ECE)",                            CIELabFromRGB(100f, 49f, 0f, 100f)      },
            { "American Rose",                              CIELabFromRGB(100f, 1f, 24f, 100f)      },
            { "Amethyst",                                   CIELabFromRGB(60f, 40f, 80f, 100f)      },
            { "Android Green",                              CIELabFromRGB(64f, 78f, 22f, 100f)      },
            { "Anti-Flash White",                           CIELabFromRGB(95f, 95f, 96f, 100f)      },
            { "Antique Brass",                              CIELabFromRGB(80f, 58f, 46f, 100f)      },
            { "Antique Bronze",                             CIELabFromRGB(40f, 36f, 12f, 100f)      },
            { "Antique Fuchsia",                            CIELabFromRGB(57f, 36f, 51f, 100f)      },
            { "Antique Ruby",                               CIELabFromRGB(52f, 11f, 18f, 100f)      },
            { "Antique White",                              CIELabFromRGB(98f, 92f, 84f, 100f)      },
            { "Ao (English)",                               CIELabFromRGB(0f, 50f, 0f, 100f)        },
            { "Apple Green",                                CIELabFromRGB(55f, 71f, 0f, 100f)       },
            { "Apricot",                                    CIELabFromRGB(98f, 81f, 69f, 100f)      },
            { "Aqua",                                       CIELabFromRGB(0f, 100f, 100f, 100f)     },
            { "Aquamarine",                                 CIELabFromRGB(50f, 100f, 83f, 100f)     },
            { "Army Green",                                 CIELabFromRGB(29f, 33f, 13f, 100f)      },
            { "Arsenic",                                    CIELabFromRGB(23f, 27f, 29f, 100f)      },
            { "Artichoke",                                  CIELabFromRGB(56f, 59f, 47f, 100f)      },
            { "Arylide Yellow",                             CIELabFromRGB(91f, 84f, 42f, 100f)      },
            { "Ash Grey",                                   CIELabFromRGB(70f, 75f, 71f, 100f)      },
            { "Asparagus",                                  CIELabFromRGB(53f, 66f, 42f, 100f)      },
            { "Atomic Tangerine",                           CIELabFromRGB(100f, 60f, 40f, 100f)     },
            { "Auburn",                                     CIELabFromRGB(65f, 16f, 16f, 100f)      },
            { "Aureolin",                                   CIELabFromRGB(99f, 93f, 0f, 100f)       },
            { "AuroMetalSaurus",                            CIELabFromRGB(43f, 50f, 50f, 100f)      },
            { "Avocado",                                    CIELabFromRGB(34f, 51f, 1f, 100f)       },
            { "Azure",                                      CIELabFromRGB(0f, 50f, 100f, 100f)      },
            { "Azure (Web Color)",                          CIELabFromRGB(94f, 100f, 100f, 100f)    },
            { "Azure Mist",                                 CIELabFromRGB(94f, 100f, 100f, 100f)    },
            { "Azureish White",                             CIELabFromRGB(86f, 91f, 96f, 100f)      },
            { "Baby Blue",                                  CIELabFromRGB(54f, 81f, 94f, 100f)      },
            { "Baby Blue Eyes",                             CIELabFromRGB(63f, 79f, 95f, 100f)      },
            { "Baby Pink",                                  CIELabFromRGB(96f, 76f, 76f, 100f)      },
            { "Baby Powder",                                CIELabFromRGB(100f, 100f, 98f, 100f)    },
            { "Baker-Miller Pink",                          CIELabFromRGB(100f, 57f, 69f, 100f)     },
            { "Ball Blue",                                  CIELabFromRGB(13f, 67f, 80f, 100f)      },
            { "Banana Mania",                               CIELabFromRGB(98f, 91f, 71f, 100f)      },
            { "Banana Yellow",                              CIELabFromRGB(100f, 88f, 21f, 100f)     },
            { "Bangladesh Green",                           CIELabFromRGB(0f, 42f, 31f, 100f)       },
            { "Barbie Pink",                                CIELabFromRGB(88f, 13f, 54f, 100f)      },
            { "Barn Red",                                   CIELabFromRGB(49f, 4f, 1f, 100f)        },
            { "Battleship Grey",                            CIELabFromRGB(52f, 52f, 51f, 100f)      },
            { "Bazaar",                                     CIELabFromRGB(60f, 47f, 48f, 100f)      },
            { "Beau Blue",                                  CIELabFromRGB(74f, 83f, 90f, 100f)      },
            { "Beaver",                                     CIELabFromRGB(62f, 51f, 44f, 100f)      },
            { "Beige",                                      CIELabFromRGB(96f, 96f, 86f, 100f)      },
            { "B'dazzled Blue",                             CIELabFromRGB(18f, 35f, 58f, 100f)      },
            { "Big Dip O’ruby",                             CIELabFromRGB(61f, 15f, 26f, 100f)      },
            { "Bisque",                                     CIELabFromRGB(100f, 89f, 77f, 100f)     },
            { "Bistre",                                     CIELabFromRGB(24f, 17f, 12f, 100f)      },
            { "Bistre Brown",                               CIELabFromRGB(59f, 44f, 9f, 100f)       },
            { "Bitter Lemon",                               CIELabFromRGB(79f, 88f, 5f, 100f)       },
            { "Bitter Lime",                                CIELabFromRGB(75f, 100f, 0f, 100f)      },
            { "Bittersweet",                                CIELabFromRGB(100f, 44f, 37f, 100f)     },
            { "Bittersweet Shimmer",                        CIELabFromRGB(75f, 31f, 32f, 100f)      },
            { "Black",                                      CIELabFromRGB(0f, 0f, 0f, 100f)         },
            { "Black Bean",                                 CIELabFromRGB(24f, 5f, 1f, 100f)        },
            { "Black Leather Jacket",                       CIELabFromRGB(15f, 21f, 16f, 100f)      },
            { "Black Olive",                                CIELabFromRGB(23f, 24f, 21f, 100f)      },
            { "Blanched Almond",                            CIELabFromRGB(100f, 92f, 80f, 100f)     },
            { "Blast-Off Bronze",                           CIELabFromRGB(65f, 44f, 39f, 100f)      },
            { "Bleu De France",                             CIELabFromRGB(19f, 55f, 91f, 100f)      },
            { "Blizzard Blue",                              CIELabFromRGB(67f, 90f, 93f, 100f)      },
            { "Blond",                                      CIELabFromRGB(98f, 94f, 75f, 100f)      },
            { "Blue",                                       CIELabFromRGB(0f, 0f, 100f, 100f)       },
            { "Blue (Crayola)",                             CIELabFromRGB(12f, 46f, 100f, 100f)     },
            { "Blue (Munsell)",                             CIELabFromRGB(0f, 58f, 69f, 100f)       },
            { "Blue (NCS)",                                 CIELabFromRGB(0f, 53f, 74f, 100f)       },
            { "Blue (Pantone)",                             CIELabFromRGB(0f, 9f, 66f, 100f)        },
            { "Blue (Pigment)",                             CIELabFromRGB(20f, 20f, 60f, 100f)      },
            { "Blue (RYB)",                                 CIELabFromRGB(1f, 28f, 100f, 100f)      },
            { "Blue Bell",                                  CIELabFromRGB(64f, 64f, 82f, 100f)      },
            { "Blue-Gray",                                  CIELabFromRGB(40f, 60f, 80f, 100f)      },
            { "Blue-Green",                                 CIELabFromRGB(5f, 60f, 73f, 100f)       },
            { "Blue Lagoon",                                CIELabFromRGB(37f, 58f, 63f, 100f)      },
            { "Blue-Magenta Violet",                        CIELabFromRGB(33f, 21f, 57f, 100f)      },
            { "Blue Sapphire",                              CIELabFromRGB(7f, 38f, 50f, 100f)       },
            { "Blue-Violet",                                CIELabFromRGB(54f, 17f, 89f, 100f)      },
            { "Blue Yonder",                                CIELabFromRGB(31f, 45f, 65f, 100f)      },
            { "Blueberry",                                  CIELabFromRGB(31f, 53f, 97f, 100f)      },
            { "Bluebonnet",                                 CIELabFromRGB(11f, 11f, 94f, 100f)      },
            { "Blush",                                      CIELabFromRGB(87f, 36f, 51f, 100f)      },
            { "Bole",                                       CIELabFromRGB(47f, 27f, 23f, 100f)      },
            { "Bondi Blue",                                 CIELabFromRGB(0f, 58f, 71f, 100f)       },
            { "Bone",                                       CIELabFromRGB(89f, 85f, 79f, 100f)      },
            { "Boston University Red",                      CIELabFromRGB(80f, 0f, 0f, 100f)        },
            { "Bottle Green",                               CIELabFromRGB(0f, 42f, 31f, 100f)       },
            { "Boysenberry",                                CIELabFromRGB(53f, 20f, 38f, 100f)      },
            { "Brandeis Blue",                              CIELabFromRGB(0f, 44f, 100f, 100f)      },
            { "Brass",                                      CIELabFromRGB(71f, 65f, 26f, 100f)      },
            { "Brick Red",                                  CIELabFromRGB(80f, 25f, 33f, 100f)      },
            { "Bright Cerulean",                            CIELabFromRGB(11f, 67f, 84f, 100f)      },
            { "Bright Green",                               CIELabFromRGB(40f, 100f, 0f, 100f)      },
            { "Bright Lavender",                            CIELabFromRGB(75f, 58f, 89f, 100f)      },
            { "Bright Lilac",                               CIELabFromRGB(85f, 57f, 94f, 100f)      },
            { "Bright Maroon",                              CIELabFromRGB(76f, 13f, 28f, 100f)      },
            { "Bright Navy Blue",                           CIELabFromRGB(10f, 45f, 82f, 100f)      },
            { "Bright Pink",                                CIELabFromRGB(100f, 0f, 50f, 100f)      },
            { "Bright Turquoise",                           CIELabFromRGB(3f, 91f, 87f, 100f)       },
            { "Bright Ube",                                 CIELabFromRGB(82f, 62f, 91f, 100f)      },
            { "Brilliant Azure",                            CIELabFromRGB(20f, 60f, 100f, 100f)     },
            { "Brilliant Lavender",                         CIELabFromRGB(96f, 73f, 100f, 100f)     },
            { "Brilliant Rose",                             CIELabFromRGB(100f, 33f, 64f, 100f)     },
            { "Brink Pink",                                 CIELabFromRGB(98f, 38f, 50f, 100f)      },
            { "British Racing Green",                       CIELabFromRGB(0f, 26f, 15f, 100f)       },
            { "Bronze",                                     CIELabFromRGB(80f, 50f, 20f, 100f)      },
            { "Bronze Yellow",                              CIELabFromRGB(45f, 44f, 0f, 100f)       },
            { "Brown (Traditional)",                        CIELabFromRGB(59f, 29f, 0f, 100f)       },
            { "Brown (Web)",                                CIELabFromRGB(65f, 16f, 16f, 100f)      },
            { "Brown-Nose",                                 CIELabFromRGB(42f, 27f, 14f, 100f)      },
            { "Brown Yellow",                               CIELabFromRGB(80f, 60f, 40f, 100f)      },
            { "Brunswick Green",                            CIELabFromRGB(11f, 30f, 24f, 100f)      },
            { "Bubble Gum",                                 CIELabFromRGB(100f, 76f, 80f, 100f)     },
            { "Bubbles",                                    CIELabFromRGB(91f, 100f, 100f, 100f)    },
            { "Buff",                                       CIELabFromRGB(94f, 86f, 51f, 100f)      },
            { "Bud Green",                                  CIELabFromRGB(48f, 71f, 38f, 100f)      },
            { "Bulgarian Rose",                             CIELabFromRGB(28f, 2f, 3f, 100f)        },
            { "Burgundy",                                   CIELabFromRGB(50f, 0f, 13f, 100f)       },
            { "Burlywood",                                  CIELabFromRGB(87f, 72f, 53f, 100f)      },
            { "Burnt Orange",                               CIELabFromRGB(80f, 33f, 0f, 100f)       },
            { "Burnt Sienna",                               CIELabFromRGB(91f, 45f, 32f, 100f)      },
            { "Burnt Umber",                                CIELabFromRGB(54f, 20f, 14f, 100f)      },
            { "Byzantine",                                  CIELabFromRGB(74f, 20f, 64f, 100f)      },
            { "Byzantium",                                  CIELabFromRGB(44f, 16f, 39f, 100f)      },
            { "Cadet",                                      CIELabFromRGB(33f, 41f, 45f, 100f)      },
            { "Cadet Blue",                                 CIELabFromRGB(37f, 62f, 63f, 100f)      },
            { "Cadet Grey",                                 CIELabFromRGB(57f, 64f, 69f, 100f)      },
            { "Cadmium Green",                              CIELabFromRGB(0f, 42f, 24f, 100f)       },
            { "Cadmium Orange",                             CIELabFromRGB(93f, 53f, 18f, 100f)      },
            { "Cadmium Red",                                CIELabFromRGB(89f, 0f, 13f, 100f)       },
            { "Cadmium Yellow",                             CIELabFromRGB(100f, 96f, 0f, 100f)      },
            { "Cafe Au Lait",                               CIELabFromRGB(65f, 48f, 36f, 100f)      },
            { "Cafe Noir",                                  CIELabFromRGB(29f, 21f, 13f, 100f)      },
            { "Cal Poly Green",                             CIELabFromRGB(12f, 30f, 17f, 100f)      },
            { "Cambridge Blue",                             CIELabFromRGB(64f, 76f, 68f, 100f)      },
            { "Camel",                                      CIELabFromRGB(76f, 60f, 42f, 100f)      },
            { "Cameo Pink",                                 CIELabFromRGB(94f, 73f, 80f, 100f)      },
            { "Camouflage Green",                           CIELabFromRGB(47f, 53f, 42f, 100f)      },
            { "Canary Yellow",                              CIELabFromRGB(100f, 94f, 0f, 100f)      },
            { "Candy Apple Red",                            CIELabFromRGB(100f, 3f, 0f, 100f)       },
            { "Candy Pink",                                 CIELabFromRGB(89f, 44f, 48f, 100f)      },
            { "Capri",                                      CIELabFromRGB(0f, 75f, 100f, 100f)      },
            { "Caput Mortuum",                              CIELabFromRGB(35f, 15f, 13f, 100f)      },
            { "Cardinal",                                   CIELabFromRGB(77f, 12f, 23f, 100f)      },
            { "Caribbean Green",                            CIELabFromRGB(0f, 80f, 60f, 100f)       },
            { "Carmine",                                    CIELabFromRGB(59f, 0f, 9f, 100f)        },
            { "Carmine (M&P)",                              CIELabFromRGB(84f, 0f, 25f, 100f)       },
            { "Carmine Pink",                               CIELabFromRGB(92f, 30f, 26f, 100f)      },
            { "Carmine Red",                                CIELabFromRGB(100f, 0f, 22f, 100f)      },
            { "Carnation Pink",                             CIELabFromRGB(100f, 65f, 79f, 100f)     },
            { "Carnelian",                                  CIELabFromRGB(70f, 11f, 11f, 100f)      },
            { "Carolina Blue",                              CIELabFromRGB(34f, 63f, 83f, 100f)      },
            { "Carrot Orange",                              CIELabFromRGB(93f, 57f, 13f, 100f)      },
            { "Castleton Green",                            CIELabFromRGB(0f, 34f, 25f, 100f)       },
            { "Catalina Blue",                              CIELabFromRGB(2f, 16f, 47f, 100f)       },
            { "Catawba",                                    CIELabFromRGB(44f, 21f, 26f, 100f)      },
            { "Cedar Chest",                                CIELabFromRGB(79f, 35f, 29f, 100f)      },
            { "Ceil",                                       CIELabFromRGB(57f, 63f, 81f, 100f)      },
            { "Celadon",                                    CIELabFromRGB(67f, 88f, 69f, 100f)      },
            { "Celadon Blue",                               CIELabFromRGB(0f, 48f, 65f, 100f)       },
            { "Celadon Green",                              CIELabFromRGB(18f, 52f, 49f, 100f)      },
            { "Celeste",                                    CIELabFromRGB(70f, 100f, 100f, 100f)    },
            { "Celestial Blue",                             CIELabFromRGB(29f, 59f, 82f, 100f)      },
            { "Cerise",                                     CIELabFromRGB(87f, 19f, 39f, 100f)      },
            { "Cerise Pink",                                CIELabFromRGB(93f, 23f, 51f, 100f)      },
            { "Cerulean",                                   CIELabFromRGB(0f, 48f, 65f, 100f)       },
            { "Cerulean Blue",                              CIELabFromRGB(16f, 32f, 75f, 100f)      },
            { "Cerulean Frost",                             CIELabFromRGB(43f, 61f, 76f, 100f)      },
            { "CG Blue",                                    CIELabFromRGB(0f, 48f, 65f, 100f)       },
            { "CG Red",                                     CIELabFromRGB(88f, 24f, 19f, 100f)      },
            { "Chamoisee",                                  CIELabFromRGB(63f, 47f, 35f, 100f)      },
            { "Champagne",                                  CIELabFromRGB(97f, 91f, 81f, 100f)      },
            { "Charcoal",                                   CIELabFromRGB(21f, 27f, 31f, 100f)      },
            { "Charleston Green",                           CIELabFromRGB(14f, 17f, 17f, 100f)      },
            { "Charm Pink",                                 CIELabFromRGB(90f, 56f, 67f, 100f)      },
            { "Chartreuse (Traditional)",                   CIELabFromRGB(87f, 100f, 0f, 100f)      },
            { "Chartreuse (Web)",                           CIELabFromRGB(50f, 100f, 0f, 100f)      },
            { "Cherry",                                     CIELabFromRGB(87f, 19f, 39f, 100f)      },
            { "Cherry Blossom Pink",                        CIELabFromRGB(100f, 72f, 77f, 100f)     },
            { "Chestnut",                                   CIELabFromRGB(58f, 27f, 21f, 100f)      },
            { "China Pink",                                 CIELabFromRGB(87f, 44f, 63f, 100f)      },
            { "China Rose",                                 CIELabFromRGB(66f, 32f, 43f, 100f)      },
            { "Chinese Red",                                CIELabFromRGB(67f, 22f, 12f, 100f)      },
            { "Chinese Violet",                             CIELabFromRGB(52f, 38f, 53f, 100f)      },
            { "Chocolate (Traditional)",                    CIELabFromRGB(48f, 25f, 0f, 100f)       },
            { "Chocolate (Web)",                            CIELabFromRGB(82f, 41f, 12f, 100f)      },
            { "Chrome Yellow",                              CIELabFromRGB(100f, 65f, 0f, 100f)      },
            { "Cinereous",                                  CIELabFromRGB(60f, 51f, 48f, 100f)      },
            { "Cinnabar",                                   CIELabFromRGB(89f, 26f, 20f, 100f)      },
            { "Cinnamon",                                   CIELabFromRGB(82f, 41f, 12f, 100f)      },
            { "Citrine",                                    CIELabFromRGB(89f, 82f, 4f, 100f)       },
            { "Citron",                                     CIELabFromRGB(62f, 66f, 12f, 100f)      },
            { "Claret",                                     CIELabFromRGB(50f, 9f, 20f, 100f)       },
            { "Classic Rose",                               CIELabFromRGB(98f, 80f, 91f, 100f)      },
            { "Cobalt Blue",                                CIELabFromRGB(0f, 28f, 67f, 100f)       },
            { "Cocoa Brown",                                CIELabFromRGB(82f, 41f, 12f, 100f)      },
            { "Coconut",                                    CIELabFromRGB(59f, 35f, 24f, 100f)      },
            { "Coffee",                                     CIELabFromRGB(44f, 31f, 22f, 100f)      },
            { "Columbia Blue",                              CIELabFromRGB(77f, 85f, 89f, 100f)      },
            { "Congo Pink",                                 CIELabFromRGB(97f, 51f, 47f, 100f)      },
            { "Cool Black",                                 CIELabFromRGB(0f, 18f, 39f, 100f)       },
            { "Cool Grey",                                  CIELabFromRGB(55f, 57f, 67f, 100f)      },
            { "Copper",                                     CIELabFromRGB(72f, 45f, 20f, 100f)      },
            { "Copper (Crayola)",                           CIELabFromRGB(85f, 54f, 40f, 100f)      },
            { "Copper Penny",                               CIELabFromRGB(68f, 44f, 41f, 100f)      },
            { "Copper Red",                                 CIELabFromRGB(80f, 43f, 32f, 100f)      },
            { "Copper Rose",                                CIELabFromRGB(60f, 40f, 40f, 100f)      },
            { "Coquelicot",                                 CIELabFromRGB(100f, 22f, 0f, 100f)      },
            { "Coral",                                      CIELabFromRGB(100f, 50f, 31f, 100f)     },
            { "Coral Pink",                                 CIELabFromRGB(97f, 51f, 47f, 100f)      },
            { "Coral Red",                                  CIELabFromRGB(100f, 25f, 25f, 100f)     },
            { "Cordovan",                                   CIELabFromRGB(54f, 25f, 27f, 100f)      },
            { "Corn",                                       CIELabFromRGB(98f, 93f, 36f, 100f)      },
            { "Cornell Red",                                CIELabFromRGB(70f, 11f, 11f, 100f)      },
            { "Cornflower Blue",                            CIELabFromRGB(39f, 58f, 93f, 100f)      },
            { "Cornsilk",                                   CIELabFromRGB(100f, 97f, 86f, 100f)     },
            { "Cosmic Latte",                               CIELabFromRGB(100f, 97f, 91f, 100f)     },
            { "Coyote Brown",                               CIELabFromRGB(51f, 38f, 24f, 100f)      },
            { "Cotton Candy",                               CIELabFromRGB(100f, 74f, 85f, 100f)     },
            { "Cream",                                      CIELabFromRGB(100f, 99f, 82f, 100f)     },
            { "Crimson",                                    CIELabFromRGB(86f, 8f, 24f, 100f)       },
            { "Crimson Glory",                              CIELabFromRGB(75f, 0f, 20f, 100f)       },
            { "Crimson Red",                                CIELabFromRGB(60f, 0f, 0f, 100f)        },
            { "Cyan",                                       CIELabFromRGB(0f, 100f, 100f, 100f)     },
            { "Cyan Azure",                                 CIELabFromRGB(31f, 51f, 71f, 100f)      },
            { "Cyan-Blue Azure",                            CIELabFromRGB(27f, 51f, 75f, 100f)      },
            { "Cyan Cobalt Blue",                           CIELabFromRGB(16f, 35f, 61f, 100f)      },
            { "Cyan Cornflower Blue",                       CIELabFromRGB(9f, 55f, 76f, 100f)       },
            { "Cyan (Process)",                             CIELabFromRGB(0f, 72f, 92f, 100f)       },
            { "Cyber Grape",                                CIELabFromRGB(35f, 26f, 49f, 100f)      },
            { "Cyber Yellow",                               CIELabFromRGB(100f, 83f, 0f, 100f)      },
            { "Daffodil",                                   CIELabFromRGB(100f, 100f, 19f, 100f)    },
            { "Dandelion",                                  CIELabFromRGB(94f, 88f, 19f, 100f)      },
            { "Dark Blue",                                  CIELabFromRGB(0f, 0f, 55f, 100f)        },
            { "Dark Blue-Gray",                             CIELabFromRGB(40f, 40f, 60f, 100f)      },
            { "Dark Brown",                                 CIELabFromRGB(40f, 26f, 13f, 100f)      },
            { "Dark Brown-Tangelo",                         CIELabFromRGB(53f, 40f, 31f, 100f)      },
            { "Dark Byzantium",                             CIELabFromRGB(36f, 22f, 33f, 100f)      },
            { "Dark Candy Apple Red",                       CIELabFromRGB(64f, 0f, 0f, 100f)        },
            { "Dark Cerulean",                              CIELabFromRGB(3f, 27f, 49f, 100f)       },
            { "Dark Chestnut",                              CIELabFromRGB(60f, 41f, 38f, 100f)      },
            { "Dark Coral",                                 CIELabFromRGB(80f, 36f, 27f, 100f)      },
            { "Dark Cyan",                                  CIELabFromRGB(0f, 55f, 55f, 100f)       },
            { "Dark Electric Blue",                         CIELabFromRGB(33f, 41f, 47f, 100f)      },
            { "Dark Goldenrod",                             CIELabFromRGB(72f, 53f, 4f, 100f)       },
            { "Dark Gray (X11)",                            CIELabFromRGB(66f, 66f, 66f, 100f)      },
            { "Dark Green",                                 CIELabFromRGB(0f, 20f, 13f, 100f)       },
            { "Dark Green (X11)",                           CIELabFromRGB(0f, 39f, 0f, 100f)        },
            { "Dark Imperial Blue",                         CIELabFromRGB(0f, 25f, 42f, 100f)       },
            { "Dark Imperial-er Blue",                      CIELabFromRGB(0f, 8f, 49f, 100f)        },
            { "Dark Jungle Green",                          CIELabFromRGB(10f, 14f, 13f, 100f)      },
            { "Dark Khaki",                                 CIELabFromRGB(74f, 72f, 42f, 100f)      },
            { "Dark Lava",                                  CIELabFromRGB(28f, 24f, 20f, 100f)      },
            { "Dark Lavender",                              CIELabFromRGB(45f, 31f, 59f, 100f)      },
            { "Dark Liver",                                 CIELabFromRGB(33f, 29f, 31f, 100f)      },
            { "Dark Liver (Horses)",                        CIELabFromRGB(33f, 24f, 22f, 100f)      },
            { "Dark Magenta",                               CIELabFromRGB(55f, 0f, 55f, 100f)       },
            { "Dark Medium Gray",                           CIELabFromRGB(66f, 66f, 66f, 100f)      },
            { "Dark Midnight Blue",                         CIELabFromRGB(0f, 20f, 40f, 100f)       },
            { "Dark Moss Green",                            CIELabFromRGB(29f, 36f, 14f, 100f)      },
            { "Dark Olive Green",                           CIELabFromRGB(33f, 42f, 18f, 100f)      },
            { "Dark Orange",                                CIELabFromRGB(100f, 55f, 0f, 100f)      },
            { "Dark Orchid",                                CIELabFromRGB(60f, 20f, 80f, 100f)      },
            { "Dark Pastel Blue",                           CIELabFromRGB(47f, 62f, 80f, 100f)      },
            { "Dark Pastel Green",                          CIELabFromRGB(1f, 75f, 24f, 100f)       },
            { "Dark Pastel Purple",                         CIELabFromRGB(59f, 44f, 84f, 100f)      },
            { "Dark Pastel Red",                            CIELabFromRGB(76f, 23f, 13f, 100f)      },
            { "Dark Pink",                                  CIELabFromRGB(91f, 33f, 50f, 100f)      },
            { "Dark Powder Blue",                           CIELabFromRGB(0f, 20f, 60f, 100f)       },
            { "Dark Puce",                                  CIELabFromRGB(31f, 23f, 24f, 100f)      },
            { "Dark Purple",                                CIELabFromRGB(19f, 10f, 20f, 100f)      },
            { "Dark Raspberry",                             CIELabFromRGB(53f, 15f, 34f, 100f)      },
            { "Dark Red",                                   CIELabFromRGB(55f, 0f, 0f, 100f)        },
            { "Dark Salmon",                                CIELabFromRGB(91f, 59f, 48f, 100f)      },
            { "Dark Scarlet",                               CIELabFromRGB(34f, 1f, 10f, 100f)       },
            { "Dark Sea Green",                             CIELabFromRGB(56f, 74f, 56f, 100f)      },
            { "Dark Sienna",                                CIELabFromRGB(24f, 8f, 8f, 100f)        },
            { "Dark Sky Blue",                              CIELabFromRGB(55f, 75f, 84f, 100f)      },
            { "Dark Slate Blue",                            CIELabFromRGB(28f, 24f, 55f, 100f)      },
            { "Dark Slate Gray",                            CIELabFromRGB(18f, 31f, 31f, 100f)      },
            { "Dark Spring Green",                          CIELabFromRGB(9f, 45f, 27f, 100f)       },
            { "Dark Tan",                                   CIELabFromRGB(57f, 51f, 32f, 100f)      },
            { "Dark Tangerine",                             CIELabFromRGB(100f, 66f, 7f, 100f)      },
            { "Dark Taupe",                                 CIELabFromRGB(28f, 24f, 20f, 100f)      },
            { "Dark Terra Cotta",                           CIELabFromRGB(80f, 31f, 36f, 100f)      },
            { "Dark Turquoise",                             CIELabFromRGB(0f, 81f, 82f, 100f)       },
            { "Dark Vanilla",                               CIELabFromRGB(82f, 75f, 66f, 100f)      },
            { "Dark Violet",                                CIELabFromRGB(58f, 0f, 83f, 100f)       },
            { "Dark Yellow",                                CIELabFromRGB(61f, 53f, 5f, 100f)       },
            { "Dartmouth Green",                            CIELabFromRGB(0f, 44f, 24f, 100f)       },
            { "Davy's Grey",                                CIELabFromRGB(33f, 33f, 33f, 100f)      },
            { "Debian Red",                                 CIELabFromRGB(84f, 4f, 33f, 100f)       },
            { "Deep Aquamarine",                            CIELabFromRGB(25f, 51f, 43f, 100f)      },
            { "Deep Carmine",                               CIELabFromRGB(66f, 13f, 24f, 100f)      },
            { "Deep Carmine Pink",                          CIELabFromRGB(94f, 19f, 22f, 100f)      },
            { "Deep Carrot Orange",                         CIELabFromRGB(91f, 41f, 17f, 100f)      },
            { "Deep Cerise",                                CIELabFromRGB(85f, 20f, 53f, 100f)      },
            { "Deep Champagne",                             CIELabFromRGB(98f, 84f, 65f, 100f)      },
            { "Deep Chestnut",                              CIELabFromRGB(73f, 31f, 28f, 100f)      },
            { "Deep Coffee",                                CIELabFromRGB(44f, 26f, 25f, 100f)      },
            { "Deep Fuchsia",                               CIELabFromRGB(76f, 33f, 76f, 100f)      },
            { "Deep Green",                                 CIELabFromRGB(2f, 40f, 3f, 100f)        },
            { "Deep Green-Cyan Turquoise",                  CIELabFromRGB(5f, 49f, 38f, 100f)       },
            { "Deep Jungle Green",                          CIELabFromRGB(0f, 29f, 29f, 100f)       },
            { "Deep Koamaru",                               CIELabFromRGB(20f, 20f, 40f, 100f)      },
            { "Deep Lemon",                                 CIELabFromRGB(96f, 78f, 10f, 100f)      },
            { "Deep Lilac",                                 CIELabFromRGB(60f, 33f, 73f, 100f)      },
            { "Deep Magenta",                               CIELabFromRGB(80f, 0f, 80f, 100f)       },
            { "Deep Maroon",                                CIELabFromRGB(51f, 0f, 0f, 100f)        },
            { "Deep Mauve",                                 CIELabFromRGB(83f, 45f, 83f, 100f)      },
            { "Deep Moss Green",                            CIELabFromRGB(21f, 37f, 23f, 100f)      },
            { "Deep Peach",                                 CIELabFromRGB(100f, 80f, 64f, 100f)     },
            { "Deep Pink",                                  CIELabFromRGB(100f, 8f, 58f, 100f)      },
            { "Deep Puce",                                  CIELabFromRGB(66f, 36f, 41f, 100f)      },
            { "Deep Red",                                   CIELabFromRGB(52f, 0f, 0f, 100f)        },
            { "Deep Ruby",                                  CIELabFromRGB(52f, 25f, 36f, 100f)      },
            { "Deep Saffron",                               CIELabFromRGB(100f, 60f, 20f, 100f)     },
            { "Deep Sky Blue",                              CIELabFromRGB(0f, 75f, 100f, 100f)      },
            { "Deep Space Sparkle",                         CIELabFromRGB(29f, 39f, 42f, 100f)      },
            { "Deep Spring Bud",                            CIELabFromRGB(33f, 42f, 18f, 100f)      },
            { "Deep Taupe",                                 CIELabFromRGB(49f, 37f, 38f, 100f)      },
            { "Deep Tuscan Red",                            CIELabFromRGB(40f, 26f, 30f, 100f)      },
            { "Deep Violet",                                CIELabFromRGB(20f, 0f, 40f, 100f)       },
            { "Deer",                                       CIELabFromRGB(73f, 53f, 35f, 100f)      },
            { "Denim",                                      CIELabFromRGB(8f, 38f, 74f, 100f)       },
            { "Desaturated Cyan",                           CIELabFromRGB(40f, 60f, 60f, 100f)      },
            { "Desert",                                     CIELabFromRGB(76f, 60f, 42f, 100f)      },
            { "Desert Sand",                                CIELabFromRGB(93f, 79f, 69f, 100f)      },
            { "Desire",                                     CIELabFromRGB(92f, 24f, 33f, 100f)      },
            { "Diamond",                                    CIELabFromRGB(73f, 95f, 100f, 100f)     },
            { "Dim Gray",                                   CIELabFromRGB(41f, 41f, 41f, 100f)      },
            { "Dirt",                                       CIELabFromRGB(61f, 46f, 33f, 100f)      },
            { "Dodger Blue",                                CIELabFromRGB(12f, 56f, 100f, 100f)     },
            { "Dogwood Rose",                               CIELabFromRGB(84f, 9f, 41f, 100f)       },
            { "Dollar Bill",                                CIELabFromRGB(52f, 73f, 40f, 100f)      },
            { "Donkey Brown",                               CIELabFromRGB(40f, 30f, 16f, 100f)      },
            { "Drab",                                       CIELabFromRGB(59f, 44f, 9f, 100f)       },
            { "Duke Blue",                                  CIELabFromRGB(0f, 0f, 61f, 100f)        },
            { "Dust Storm",                                 CIELabFromRGB(90f, 80f, 79f, 100f)      },
            { "Dutch White",                                CIELabFromRGB(94f, 87f, 73f, 100f)      },
            { "Earth Yellow",                               CIELabFromRGB(88f, 66f, 37f, 100f)      },
            { "Ebony",                                      CIELabFromRGB(33f, 36f, 31f, 100f)      },
            { "Ecru",                                       CIELabFromRGB(76f, 70f, 50f, 100f)      },
            { "Eerie Black",                                CIELabFromRGB(11f, 11f, 11f, 100f)      },
            { "Eggplant",                                   CIELabFromRGB(38f, 25f, 32f, 100f)      },
            { "Eggshell",                                   CIELabFromRGB(94f, 92f, 84f, 100f)      },
            { "Egyptian Blue",                              CIELabFromRGB(6f, 20f, 65f, 100f)       },
            { "Electric Blue",                              CIELabFromRGB(49f, 98f, 100f, 100f)     },
            { "Electric Crimson",                           CIELabFromRGB(100f, 0f, 25f, 100f)      },
            { "Electric Cyan",                              CIELabFromRGB(0f, 100f, 100f, 100f)     },
            { "Electric Green",                             CIELabFromRGB(0f, 100f, 0f, 100f)       },
            { "Electric Indigo",                            CIELabFromRGB(44f, 0f, 100f, 100f)      },
            { "Electric Lavender",                          CIELabFromRGB(96f, 73f, 100f, 100f)     },
            { "Electric Lime",                              CIELabFromRGB(80f, 100f, 0f, 100f)      },
            { "Electric Purple",                            CIELabFromRGB(75f, 0f, 100f, 100f)      },
            { "Electric Ultramarine",                       CIELabFromRGB(25f, 0f, 100f, 100f)      },
            { "Electric Violet",                            CIELabFromRGB(56f, 0f, 100f, 100f)      },
            { "Electric Yellow",                            CIELabFromRGB(100f, 100f, 20f, 100f)    },
            { "Emerald",                                    CIELabFromRGB(31f, 78f, 47f, 100f)      },
            { "Eminence",                                   CIELabFromRGB(42f, 19f, 51f, 100f)      },
            { "English Green",                              CIELabFromRGB(11f, 30f, 24f, 100f)      },
            { "English Lavender",                           CIELabFromRGB(71f, 51f, 58f, 100f)      },
            { "English Red",                                CIELabFromRGB(67f, 29f, 32f, 100f)      },
            { "English Violet",                             CIELabFromRGB(34f, 24f, 36f, 100f)      },
            { "Eton Blue",                                  CIELabFromRGB(59f, 78f, 64f, 100f)      },
            { "Eucalyptus",                                 CIELabFromRGB(27f, 84f, 66f, 100f)      },
            { "Fallow",                                     CIELabFromRGB(76f, 60f, 42f, 100f)      },
            { "Falu Red",                                   CIELabFromRGB(50f, 9f, 9f, 100f)        },
            { "Fandango",                                   CIELabFromRGB(71f, 20f, 54f, 100f)      },
            { "Fandango Pink",                              CIELabFromRGB(87f, 32f, 52f, 100f)      },
            { "Fashion Fuchsia",                            CIELabFromRGB(96f, 0f, 63f, 100f)       },
            { "Fawn",                                       CIELabFromRGB(90f, 67f, 44f, 100f)      },
            { "Feldgrau",                                   CIELabFromRGB(30f, 36f, 33f, 100f)      },
            { "Feldspar",                                   CIELabFromRGB(99f, 84f, 69f, 100f)      },
            { "Fern Green",                                 CIELabFromRGB(31f, 47f, 26f, 100f)      },
            { "Ferrari Red",                                CIELabFromRGB(100f, 16f, 0f, 100f)      },
            { "Field Drab",                                 CIELabFromRGB(42f, 33f, 12f, 100f)      },
            { "Firebrick",                                  CIELabFromRGB(70f, 13f, 13f, 100f)      },
            { "Fire Engine Red",                            CIELabFromRGB(81f, 13f, 16f, 100f)      },
            { "Flame",                                      CIELabFromRGB(89f, 35f, 13f, 100f)      },
            { "Flamingo Pink",                              CIELabFromRGB(99f, 56f, 67f, 100f)      },
            { "Flattery",                                   CIELabFromRGB(42f, 27f, 14f, 100f)      },
            { "Flavescent",                                 CIELabFromRGB(97f, 91f, 56f, 100f)      },
            { "Flax",                                       CIELabFromRGB(93f, 86f, 51f, 100f)      },
            { "Flirt",                                      CIELabFromRGB(64f, 0f, 43f, 100f)       },
            { "Floral White",                               CIELabFromRGB(100f, 98f, 94f, 100f)     },
            { "Fluorescent Orange",                         CIELabFromRGB(100f, 75f, 0f, 100f)      },
            { "Fluorescent Pink",                           CIELabFromRGB(100f, 8f, 58f, 100f)      },
            { "Fluorescent Yellow",                         CIELabFromRGB(80f, 100f, 0f, 100f)      },
            { "Folly",                                      CIELabFromRGB(100f, 0f, 31f, 100f)      },
            { "Forest Green (Traditional)",                 CIELabFromRGB(0f, 27f, 13f, 100f)       },
            { "Forest Green (Web)",                         CIELabFromRGB(13f, 55f, 13f, 100f)      },
            { "French Beige",                               CIELabFromRGB(65f, 48f, 36f, 100f)      },
            { "French Bistre",                              CIELabFromRGB(52f, 43f, 30f, 100f)      },
            { "French Blue",                                CIELabFromRGB(0f, 45f, 73f, 100f)       },
            { "French Fuchsia",                             CIELabFromRGB(99f, 25f, 57f, 100f)      },
            { "French Lilac",                               CIELabFromRGB(53f, 38f, 56f, 100f)      },
            { "French Lime",                                CIELabFromRGB(62f, 99f, 22f, 100f)      },
            { "French Mauve",                               CIELabFromRGB(83f, 45f, 83f, 100f)      },
            { "French Pink",                                CIELabFromRGB(99f, 42f, 62f, 100f)      },
            { "French Plum",                                CIELabFromRGB(51f, 8f, 33f, 100f)       },
            { "French Puce",                                CIELabFromRGB(31f, 9f, 4f, 100f)        },
            { "French Raspberry",                           CIELabFromRGB(78f, 17f, 28f, 100f)      },
            { "French Rose",                                CIELabFromRGB(96f, 29f, 54f, 100f)      },
            { "French Sky Blue",                            CIELabFromRGB(47f, 71f, 100f, 100f)     },
            { "French Violet",                              CIELabFromRGB(53f, 2f, 81f, 100f)       },
            { "French Wine",                                CIELabFromRGB(67f, 12f, 27f, 100f)      },
            { "Fresh Air",                                  CIELabFromRGB(65f, 91f, 100f, 100f)     },
            { "Fuchsia",                                    CIELabFromRGB(100f, 0f, 100f, 100f)     },
            { "Fuchsia (Crayola)",                          CIELabFromRGB(76f, 33f, 76f, 100f)      },
            { "Fuchsia Pink",                               CIELabFromRGB(100f, 47f, 100f, 100f)    },
            { "Fuchsia Purple",                             CIELabFromRGB(80f, 22f, 48f, 100f)      },
            { "Fuchsia Rose",                               CIELabFromRGB(78f, 26f, 46f, 100f)      },
            { "Fulvous",                                    CIELabFromRGB(89f, 52f, 0f, 100f)       },
            { "Fuzzy Wuzzy",                                CIELabFromRGB(80f, 40f, 40f, 100f)      },
            { "Gainsboro",                                  CIELabFromRGB(86f, 86f, 86f, 100f) },
            { "Gamboge",                                    CIELabFromRGB(89f, 61f, 6f, 100f) },
            { "Gamboge Orange (Brown)",                     CIELabFromRGB(60f, 40f, 0f, 100f) },
            { "Generic Viridian",                           CIELabFromRGB(0f, 50f, 40f, 100f) },
            { "Ghost White",                                CIELabFromRGB(97f, 97f, 100f, 100f) },
            { "Giants Orange",                              CIELabFromRGB(100f, 35f, 11f, 100f) },
            { "Grussrel",                                   CIELabFromRGB(69f, 40f, 0f, 100f) },
            { "Glaucous",                                   CIELabFromRGB(38f, 51f, 71f, 100f) },
            { "Glitter",                                    CIELabFromRGB(90f, 91f, 98f, 100f) },
            { "GO Green",                                   CIELabFromRGB(0f, 67f, 40f, 100f) },
            { "Gold (Metallic)",                            CIELabFromRGB(83f, 69f, 22f, 100f) },
            { "Gold (Web) (Golden)",                        CIELabFromRGB(100f, 84f, 0f, 100f) },
            { "Gold Fusion",                                CIELabFromRGB(52f, 46f, 31f, 100f) },
            { "Golden Brown",                               CIELabFromRGB(60f, 40f, 8f, 100f) },
            { "Golden Poppy",                               CIELabFromRGB(99f, 76f, 0f, 100f) },
            { "Golden Yellow",                              CIELabFromRGB(100f, 87f, 0f, 100f) },
            { "Goldenrod",                                  CIELabFromRGB(85f, 65f, 13f, 100f) },
            { "Granny Smith Apple",                         CIELabFromRGB(66f, 89f, 63f, 100f) },
            { "Grape",                                      CIELabFromRGB(44f, 18f, 66f, 100f) },
            { "Gray",                                       CIELabFromRGB(50f, 50f, 50f, 100f) },
            { "Gray (HTML/CSS Gray)",                       CIELabFromRGB(50f, 50f, 50f, 100f) },
            { "Gray (X11 Gray)",                            CIELabFromRGB(75f, 75f, 75f, 100f) },
            { "Gray-Asparagus",                             CIELabFromRGB(27f, 35f, 27f, 100f) },
            { "Gray-Blue",                                  CIELabFromRGB(55f, 57f, 67f, 100f) },
            { "Green (Color Wheel) (X11 Green)",            CIELabFromRGB(0f, 100f, 0f, 100f) },
            { "Green (Crayola)",                            CIELabFromRGB(11f, 67f, 47f, 100f) },
            { "Green (HTML/CSS Color)",                     CIELabFromRGB(0f, 50f, 0f, 100f) },
            { "Green (Munsell)",                            CIELabFromRGB(0f, 66f, 47f, 100f) },
            { "Green (NCS)",                                CIELabFromRGB(0f, 62f, 42f, 100f) },
            { "Green (Pantone)",                            CIELabFromRGB(0f, 68f, 26f, 100f) },
            { "Green (Pigment)",                            CIELabFromRGB(0f, 65f, 31f, 100f) },
            { "Green (RYB)",                                CIELabFromRGB(40f, 69f, 20f, 100f) },
            { "Green-Blue",                                 CIELabFromRGB(7f, 39f, 71f, 100f) },
            { "Green-Cyan",                                 CIELabFromRGB(0f, 60f, 40f, 100f) },
            { "Green-Yellow",                               CIELabFromRGB(68f, 100f, 18f, 100f) },
            { "Grizzly",                                    CIELabFromRGB(53f, 35f, 9f, 100f) },
            { "Grullo",                                     CIELabFromRGB(66f, 60f, 53f, 100f) },
            { "Guppie Green",                               CIELabFromRGB(0f, 100f, 50f, 100f) },
            { "Halayà Úbe",                                 CIELabFromRGB(40f, 22f, 33f, 100f) },
            { "Han Blue",                                   CIELabFromRGB(27f, 42f, 81f, 100f) },
            { "Han Purple",                                 CIELabFromRGB(32f, 9f, 98f, 100f) },
            { "Hansa Yellow",                               CIELabFromRGB(91f, 84f, 42f, 100f) },
            { "Harlequin",                                  CIELabFromRGB(25f, 100f, 0f, 100f) },
            { "Harlequin Green",                            CIELabFromRGB(27f, 80f, 9f, 100f) },
            { "Harvard Crimson",                            CIELabFromRGB(79f, 0f, 9f, 100f) },
            { "Harvest Gold",                               CIELabFromRGB(85f, 57f, 0f, 100f) },
            { "Heart Gold",                                 CIELabFromRGB(50f, 50f, 0f, 100f) },
            { "Heliotrope",                                 CIELabFromRGB(87f, 45f, 100f, 100f) },
            { "Heliotrope Gray",                            CIELabFromRGB(67f, 60f, 66f, 100f) },
            { "Heliotrope Magenta",                         CIELabFromRGB(67f, 0f, 73f, 100f) },
            { "Hollywood Cerise",                           CIELabFromRGB(96f, 0f, 63f, 100f) },
            { "Honeydew",                                   CIELabFromRGB(94f, 100f, 94f, 100f) },
            { "Honolulu Blue",                              CIELabFromRGB(0f, 43f, 69f, 100f) },
            { "Hooker's Green",                             CIELabFromRGB(29f, 47f, 42f, 100f) },
            { "Hot Magenta",                                CIELabFromRGB(100f, 11f, 81f, 100f) },
            { "Hot Pink",                                   CIELabFromRGB(100f, 41f, 71f, 100f) },
            { "Hunter Green",                               CIELabFromRGB(21f, 37f, 23f, 100f) },
            { "Iceberg",                                    CIELabFromRGB(44f, 65f, 82f, 100f) },
            { "Icterine",                                   CIELabFromRGB(99f, 97f, 37f, 100f) },
            { "Illuminating Emerald",                       CIELabFromRGB(19f, 57f, 47f, 100f) },
            { "Imperial",                                   CIELabFromRGB(38f, 18f, 42f, 100f) },
            { "Imperial Blue",                              CIELabFromRGB(0f, 14f, 58f, 100f) },
            { "Imperial Purple",                            CIELabFromRGB(40f, 1f, 24f, 100f) },
            { "Imperial Red",                               CIELabFromRGB(93f, 16f, 22f, 100f) },
            { "Inchworm",                                   CIELabFromRGB(70f, 93f, 36f, 100f) },
            { "Independence",                               CIELabFromRGB(30f, 32f, 43f, 100f) },
            { "India Green",                                CIELabFromRGB(7f, 53f, 3f, 100f) },
            { "Indian Red",                                 CIELabFromRGB(80f, 36f, 36f, 100f) },
            { "Indian Yellow",                              CIELabFromRGB(89f, 66f, 34f, 100f) },
            { "Indigo",                                     CIELabFromRGB(44f, 0f, 100f, 100f) },
            { "Indigo Dye",                                 CIELabFromRGB(4f, 12f, 57f, 100f) },
            { "Indigo (Web)",                               CIELabFromRGB(29f, 0f, 51f, 100f) },
            { "International Klein Blue",                   CIELabFromRGB(0f, 18f, 65f, 100f) },
            { "International Orange (Aerospace)",           CIELabFromRGB(100f, 31f, 0f, 100f) },
            { "International Orange (Engineering)",         CIELabFromRGB(73f, 9f, 5f, 100f) },
            { "International Orange (Golden Gate Bridge)",  CIELabFromRGB(75f, 21f, 17f, 100f) },
            { "Iris",                                       CIELabFromRGB(35f, 31f, 81f, 100f) },
            { "Irresistible",                               CIELabFromRGB(70f, 27f, 42f, 100f) },
            { "Isabelline",                                 CIELabFromRGB(96f, 94f, 93f, 100f) },
            { "Islamic Green",                              CIELabFromRGB(0f, 56f, 0f, 100f) },
            { "Italian Sky Blue",                           CIELabFromRGB(70f, 100f, 100f, 100f) },
            { "Ivory",                                      CIELabFromRGB(100f, 100f, 94f, 100f) },
            { "Jade",                                       CIELabFromRGB(0f, 66f, 42f, 100f) },
            { "Japanese Carmine",                           CIELabFromRGB(62f, 16f, 20f, 100f) },
            { "Japanese Indigo",                            CIELabFromRGB(15f, 26f, 28f, 100f) },
            { "Japanese Violet",                            CIELabFromRGB(36f, 20f, 34f, 100f) },
            { "Jasmine",                                    CIELabFromRGB(97f, 87f, 49f, 100f) },
            { "Jasper",                                     CIELabFromRGB(84f, 23f, 24f, 100f) },
            { "Jazzberry Jam",                              CIELabFromRGB(65f, 4f, 37f, 100f) },
            { "Jelly Bean",                                 CIELabFromRGB(85f, 38f, 31f, 100f) },
            { "Jet",                                        CIELabFromRGB(20f, 20f, 20f, 100f) },
            { "Jonquil",                                    CIELabFromRGB(96f, 79f, 9f, 100f) },
            { "Jordy Blue",                                 CIELabFromRGB(54f, 73f, 95f, 100f) },
            { "June Bud",                                   CIELabFromRGB(74f, 85f, 34f, 100f) },
            { "Jungle Green",                               CIELabFromRGB(16f, 67f, 53f, 100f) },
            { "Kelly Green",                                CIELabFromRGB(30f, 73f, 9f, 100f) },
            { "Kenyan Copper",                              CIELabFromRGB(49f, 11f, 2f, 100f) },
            { "Keppel",                                     CIELabFromRGB(23f, 69f, 62f, 100f) },
            { "Jawad/Chicken Color (HTML/CSS) (Khaki)",     CIELabFromRGB(76f, 69f, 57f, 100f) },
            { "Khaki (X11) (Light Khaki)",                  CIELabFromRGB(94f, 90f, 55f, 100f) },
            { "Kobe",                                       CIELabFromRGB(53f, 18f, 9f, 100f) },
            { "Kobi",                                       CIELabFromRGB(91f, 62f, 77f, 100f) },
            { "Kombu Green",                                CIELabFromRGB(21f, 26f, 19f, 100f) },
            { "KU Crimson",                                 CIELabFromRGB(91f, 0f, 5f, 100f) },
            { "La Salle Green",                             CIELabFromRGB(3f, 47f, 19f, 100f) },
            { "Languid Lavender",                           CIELabFromRGB(84f, 79f, 87f, 100f) },
            { "Lapis Lazuli",                               CIELabFromRGB(15f, 38f, 61f, 100f) },
            { "Laser Lemon",                                CIELabFromRGB(100f, 100f, 40f, 100f) },
            { "Laurel Green",                               CIELabFromRGB(66f, 73f, 62f, 100f) },
            { "Lava",                                       CIELabFromRGB(81f, 6f, 13f, 100f) },
            { "Lavender (Floral)",                          CIELabFromRGB(71f, 49f, 86f, 100f) },
            { "Lavender (Web)",                             CIELabFromRGB(90f, 90f, 98f, 100f) },
            { "Lavender Blue",                              CIELabFromRGB(80f, 80f, 100f, 100f) },
            { "Lavender Blush",                             CIELabFromRGB(100f, 94f, 96f, 100f) },
            { "Lavender Gray",                              CIELabFromRGB(77f, 76f, 82f, 100f) },
            { "Lavender Indigo",                            CIELabFromRGB(58f, 34f, 92f, 100f) },
            { "Lavender Magenta",                           CIELabFromRGB(93f, 51f, 93f, 100f) },
            { "Lavender Mist",                              CIELabFromRGB(90f, 90f, 98f, 100f) },
            { "Lavender Pink",                              CIELabFromRGB(98f, 68f, 82f, 100f) },
            { "Lavender Purple",                            CIELabFromRGB(59f, 48f, 71f, 100f) },
            { "Lavender Rose",                              CIELabFromRGB(98f, 63f, 89f, 100f) },
            { "Lawn Green",                                 CIELabFromRGB(49f, 99f, 0f, 100f) },
            { "Lemon",                                      CIELabFromRGB(100f, 97f, 0f, 100f) },
            { "Lemon Chiffon",                              CIELabFromRGB(100f, 98f, 80f, 100f) },
            { "Lemon Curry",                                CIELabFromRGB(80f, 63f, 11f, 100f) },
            { "Lemon Glacier",                              CIELabFromRGB(99f, 100f, 0f, 100f) },
            { "Lemon Lime",                                 CIELabFromRGB(89f, 100f, 0f, 100f) },
            { "Lemon Meringue",                             CIELabFromRGB(96f, 92f, 75f, 100f) },
            { "Lemon Yellow",                               CIELabFromRGB(100f, 96f, 31f, 100f) },
            { "Lenurple",                                   CIELabFromRGB(73f, 58f, 85f, 100f) },
            { "Licorice",                                   CIELabFromRGB(10f, 7f, 6f, 100f) },
            { "Liberty",                                    CIELabFromRGB(33f, 35f, 65f, 100f) },
            { "Light Apricot",                              CIELabFromRGB(99f, 84f, 69f, 100f) },
            { "Light Blue",                                 CIELabFromRGB(68f, 85f, 90f, 100f) },
            { "Light Brilliant Red",                        CIELabFromRGB(100f, 18f, 18f, 100f) },
            { "Light Brown",                                CIELabFromRGB(71f, 40f, 11f, 100f) },
            { "Light Carmine Pink",                         CIELabFromRGB(90f, 40f, 44f, 100f) },
            { "Light Cobalt Blue",                          CIELabFromRGB(53f, 67f, 88f, 100f) },
            { "Light Coral",                                CIELabFromRGB(94f, 50f, 50f, 100f) },
            { "Light Cornflower Blue",                      CIELabFromRGB(58f, 80f, 92f, 100f) },
            { "Light Crimson",                              CIELabFromRGB(96f, 41f, 57f, 100f) },
            { "Light Cyan",                                 CIELabFromRGB(88f, 100f, 100f, 100f) },
            { "Light Deep Pink",                            CIELabFromRGB(100f, 36f, 80f, 100f) },
            { "Light French Beige",                         CIELabFromRGB(78f, 68f, 50f, 100f) },
            { "Light Fuchsia Pink",                         CIELabFromRGB(98f, 52f, 94f, 100f) },
            { "Light Goldenrod Yellow",                     CIELabFromRGB(98f, 98f, 82f, 100f) },
            { "Light Gray",                                 CIELabFromRGB(83f, 83f, 83f, 100f) },
            { "Light Grayish Magenta",                      CIELabFromRGB(80f, 60f, 80f, 100f) },
            { "Light Green",                                CIELabFromRGB(56f, 93f, 56f, 100f) },
            { "Light Hot Pink",                             CIELabFromRGB(100f, 70f, 87f, 100f) },
            { "Light Khaki",                                CIELabFromRGB(94f, 90f, 55f, 100f) },
            { "Light Medium Orchid",                        CIELabFromRGB(83f, 61f, 80f, 100f) },
            { "Light Moss Green",                           CIELabFromRGB(68f, 87f, 68f, 100f) },
            { "Light Orchid",                               CIELabFromRGB(90f, 66f, 84f, 100f) },
            { "Light Pastel Purple",                        CIELabFromRGB(69f, 61f, 85f, 100f) },
            { "Light Pink",                                 CIELabFromRGB(100f, 71f, 76f, 100f) },
            { "Light Red Ochre",                            CIELabFromRGB(91f, 45f, 32f, 100f) },
            { "Light Salmon",                               CIELabFromRGB(100f, 63f, 48f, 100f) },
            { "Light Salmon Pink",                          CIELabFromRGB(100f, 60f, 60f, 100f) },
            { "Light Sea Green",                            CIELabFromRGB(13f, 70f, 67f, 100f) },
            { "Light Sky Blue",                             CIELabFromRGB(53f, 81f, 98f, 100f) },
            { "Light Slate Gray",                           CIELabFromRGB(47f, 53f, 60f, 100f) },
            { "Light Steel Blue",                           CIELabFromRGB(69f, 77f, 87f, 100f) },
            { "Light Taupe",                                CIELabFromRGB(70f, 55f, 43f, 100f) },
            { "Light Thulian Pink",                         CIELabFromRGB(90f, 56f, 67f, 100f) },
            { "Light Yellow",                               CIELabFromRGB(100f, 100f, 88f, 100f) },
            { "Lilac",                                      CIELabFromRGB(78f, 64f, 78f, 100f) },
            { "Lime (Color Wheel)",                         CIELabFromRGB(75f, 100f, 0f, 100f) },
            { "Lime (Web) (X11 Green)",                     CIELabFromRGB(0f, 100f, 0f, 100f) },
            { "Lime Green",                                 CIELabFromRGB(20f, 80f, 20f, 100f) },
            { "Limerick",                                   CIELabFromRGB(62f, 76f, 4f, 100f) },
            { "Lincoln Green",                              CIELabFromRGB(10f, 35f, 2f, 100f) },
            { "Linen",                                      CIELabFromRGB(98f, 94f, 90f, 100f) },
            { "Lion",                                       CIELabFromRGB(76f, 60f, 42f, 100f) },
            { "Liseran Purple",                             CIELabFromRGB(87f, 44f, 63f, 100f) },
            { "Little Boy Blue",                            CIELabFromRGB(42f, 63f, 86f, 100f) },
            { "Liver",                                      CIELabFromRGB(40f, 30f, 28f, 100f) },
            { "Liver (Dogs)",                               CIELabFromRGB(72f, 43f, 16f, 100f) },
            { "Liver (Organ)",                              CIELabFromRGB(42f, 18f, 12f, 100f) },
            { "Liver Chestnut",                             CIELabFromRGB(60f, 45f, 34f, 100f) },
            { "Livid",                                      CIELabFromRGB(40f, 60f, 80f, 100f) },
            { "Lumber",                                     CIELabFromRGB(100f, 89f, 80f, 100f) },
            { "Lust",                                       CIELabFromRGB(90f, 13f, 13f, 100f) },
            { "Magenta",                                    CIELabFromRGB(100f, 0f, 100f, 100f) },
            { "Magenta (Crayola)",                          CIELabFromRGB(100f, 33f, 64f, 100f) },
            { "Magenta (Dye)",                              CIELabFromRGB(79f, 12f, 48f, 100f) },
            { "Magenta (Pantone)",                          CIELabFromRGB(82f, 25f, 49f, 100f) },
            { "Magenta (Process)",                          CIELabFromRGB(100f, 0f, 56f, 100f) },
            { "Magenta Haze",                               CIELabFromRGB(62f, 27f, 46f, 100f) },
            { "Magenta-Pink",                               CIELabFromRGB(80f, 20f, 55f, 100f) },
            { "Magic Mint",                                 CIELabFromRGB(67f, 94f, 82f, 100f) },
            { "Magnolia",                                   CIELabFromRGB(97f, 96f, 100f, 100f) },
            { "Mahogany",                                   CIELabFromRGB(75f, 25f, 0f, 100f) },
            { "Maize",                                      CIELabFromRGB(98f, 93f, 36f, 100f) },
            { "Majorelle Blue",                             CIELabFromRGB(38f, 31f, 86f, 100f) },
            { "Malachite",                                  CIELabFromRGB(4f, 85f, 32f, 100f) },
            { "Manatee",                                    CIELabFromRGB(59f, 60f, 67f, 100f) },
            { "Mango Tango",                                CIELabFromRGB(100f, 51f, 26f, 100f) },
            { "Mantis",                                     CIELabFromRGB(45f, 76f, 40f, 100f) },
            { "Mardi Gras",                                 CIELabFromRGB(53f, 0f, 52f, 100f) },
            { "Marigold",                                   CIELabFromRGB(92f, 64f, 13f, 100f) },
            { "Maroon (Crayola)",                           CIELabFromRGB(76f, 13f, 28f, 100f) },
            { "Maroon (HTML/CSS)",                          CIELabFromRGB(50f, 0f, 0f, 100f) },
            { "Maroon (X11)",                               CIELabFromRGB(69f, 19f, 38f, 100f) },
            { "Mauve",                                      CIELabFromRGB(88f, 69f, 100f, 100f) },
            { "Mauve Taupe",                                CIELabFromRGB(57f, 37f, 43f, 100f) },
            { "Mauvelous",                                  CIELabFromRGB(94f, 60f, 67f, 100f) },
            { "May Green",                                  CIELabFromRGB(30f, 57f, 25f, 100f) },
            { "Maya Blue",                                  CIELabFromRGB(45f, 76f, 98f, 100f) },
            { "Meat Brown",                                 CIELabFromRGB(90f, 72f, 23f, 100f) },
            { "Medium Aquamarine",                          CIELabFromRGB(40f, 87f, 67f, 100f) },
            { "Medium Blue",                                CIELabFromRGB(0f, 0f, 80f, 100f) },
            { "Medium Candy Apple Red",                     CIELabFromRGB(89f, 2f, 17f, 100f) },
            { "Medium Carmine",                             CIELabFromRGB(69f, 25f, 21f, 100f) },
            { "Medium Champagne",                           CIELabFromRGB(95f, 90f, 67f, 100f) },
            { "Medium Electric Blue",                       CIELabFromRGB(1f, 31f, 59f, 100f) },
            { "Medium Jungle Green",                        CIELabFromRGB(11f, 21f, 18f, 100f) },
            { "Medium Lavender Magenta",                    CIELabFromRGB(87f, 63f, 87f, 100f) },
            { "Medium Orchid",                              CIELabFromRGB(73f, 33f, 83f, 100f) },
            { "Medium Persian Blue",                        CIELabFromRGB(0f, 40f, 65f, 100f) },
            { "Medium Purple",                              CIELabFromRGB(58f, 44f, 86f, 100f) },
            { "Medium Red-Violet",                          CIELabFromRGB(73f, 20f, 52f, 100f) },
            { "Medium Ruby",                                CIELabFromRGB(67f, 25f, 41f, 100f) },
            { "Medium Sea Green",                           CIELabFromRGB(24f, 70f, 44f, 100f) },
            { "Medium Sky Blue",                            CIELabFromRGB(50f, 85f, 92f, 100f) },
            { "Medium Slate Blue",                          CIELabFromRGB(48f, 41f, 93f, 100f) },
            { "Medium Spring Bud",                          CIELabFromRGB(79f, 86f, 53f, 100f) },
            { "Medium Spring Green",                        CIELabFromRGB(0f, 98f, 60f, 100f) },
            { "Medium Taupe",                               CIELabFromRGB(40f, 30f, 28f, 100f) },
            { "Medium Turquoise",                           CIELabFromRGB(28f, 82f, 80f, 100f) },
            { "Medium Tuscan Red",                          CIELabFromRGB(47f, 27f, 23f, 100f) },
            { "Medium Vermilion",                           CIELabFromRGB(85f, 38f, 23f, 100f) },
            { "Medium Violet-Red",                          CIELabFromRGB(78f, 8f, 52f, 100f) },
            { "Mellow Apricot",                             CIELabFromRGB(97f, 72f, 47f, 100f) },
            { "Mellow Yellow",                              CIELabFromRGB(97f, 87f, 49f, 100f) },
            { "Melon",                                      CIELabFromRGB(99f, 74f, 71f, 100f) },
            { "Metallic Seaweed",                           CIELabFromRGB(4f, 49f, 55f, 100f) },
            { "Metallic Sunburst",                          CIELabFromRGB(61f, 49f, 22f, 100f) },
            { "Mexican Pink",                               CIELabFromRGB(89f, 0f, 49f, 100f) },
            { "Midnight Blue",                              CIELabFromRGB(10f, 10f, 44f, 100f) },
            { "Midnight Green (Eagle Green)",               CIELabFromRGB(0f, 29f, 33f, 100f) },
            { "Mikado Yellow",                              CIELabFromRGB(100f, 77f, 5f, 100f) },
            { "Mindaro",                                    CIELabFromRGB(89f, 98f, 53f, 100f) },
            { "Ming",                                       CIELabFromRGB(21f, 45f, 49f, 100f) },
            { "Mint",                                       CIELabFromRGB(24f, 71f, 54f, 100f) },
            { "Mint Cream",                                 CIELabFromRGB(96f, 100f, 98f, 100f) },
            { "Mint Green",                                 CIELabFromRGB(60f, 100f, 60f, 100f) },
            { "Misty Rose",                                 CIELabFromRGB(100f, 89f, 88f, 100f) },
            { "Moccasin",                                   CIELabFromRGB(98f, 92f, 84f, 100f) },
            { "Mode Beige",                                 CIELabFromRGB(59f, 44f, 9f, 100f) },
            { "Moonstone Blue",                             CIELabFromRGB(45f, 66f, 76f, 100f) },
            { "Mordant Red 19",                             CIELabFromRGB(68f, 5f, 0f, 100f) },
            { "Moss Green",                                 CIELabFromRGB(54f, 60f, 36f, 100f) },
            { "Mountain Meadow",                            CIELabFromRGB(19f, 73f, 56f, 100f) },
            { "Mountbatten Pink",                           CIELabFromRGB(60f, 48f, 55f, 100f) },
            { "MSU Green",                                  CIELabFromRGB(9f, 27f, 23f, 100f) },
            { "Mughal Green",                               CIELabFromRGB(19f, 38f, 19f, 100f) },
            { "Mulberry",                                   CIELabFromRGB(77f, 29f, 55f, 100f) },
            { "Mustard",                                    CIELabFromRGB(100f, 86f, 35f, 100f) },
            { "Myrtle Green",                               CIELabFromRGB(19f, 47f, 45f, 100f) },
            { "Nadeshiko Pink",                             CIELabFromRGB(96f, 68f, 78f, 100f) },
            { "Napier Green",                               CIELabFromRGB(16f, 50f, 0f, 100f) },
            { "Naples Yellow",                              CIELabFromRGB(98f, 85f, 37f, 100f) },
            { "Navajo White",                               CIELabFromRGB(100f, 87f, 68f, 100f) },
            { "Navy",                                       CIELabFromRGB(0f, 0f, 50f, 100f) },
            { "Navy Purple",                                CIELabFromRGB(58f, 34f, 92f, 100f) },
            { "Neon Carrot",                                CIELabFromRGB(100f, 64f, 26f, 100f) },
            { "Neon Fuchsia",                               CIELabFromRGB(100f, 25f, 39f, 100f) },
            { "Neon Green",                                 CIELabFromRGB(22f, 100f, 8f, 100f) },
            { "New Car",                                    CIELabFromRGB(13f, 31f, 78f, 100f) },
            { "New York Pink",                              CIELabFromRGB(84f, 51f, 50f, 100f) },
            { "Non-Photo Blue",                             CIELabFromRGB(64f, 87f, 93f, 100f) },
            { "North Texas Green",                          CIELabFromRGB(2f, 56f, 20f, 100f) },
            { "Nyanza",                                     CIELabFromRGB(91f, 100f, 86f, 100f) },
            { "Ocean Boat Blue",                            CIELabFromRGB(0f, 47f, 75f, 100f) },
            { "Ochre",                                      CIELabFromRGB(80f, 47f, 13f, 100f) },
            { "Office Green",                               CIELabFromRGB(0f, 50f, 0f, 100f) },
            { "Old Burgundy",                               CIELabFromRGB(26f, 19f, 18f, 100f) },
            { "Old Gold",                                   CIELabFromRGB(81f, 71f, 23f, 100f) },
            { "Old Heliotrope",                             CIELabFromRGB(34f, 24f, 36f, 100f) },
            { "Old Lace",                                   CIELabFromRGB(99f, 96f, 90f, 100f) },
            { "Old Lavender",                               CIELabFromRGB(47f, 41f, 47f, 100f) },
            { "Old Mauve",                                  CIELabFromRGB(40f, 19f, 28f, 100f) },
            { "Old Moss Green",                             CIELabFromRGB(53f, 49f, 21f, 100f) },
            { "Old Rose",                                   CIELabFromRGB(75f, 50f, 51f, 100f) },
            { "Old Silver",                                 CIELabFromRGB(52f, 52f, 51f, 100f) },
            { "Olive",                                      CIELabFromRGB(50f, 50f, 0f, 100f) },
            { "Olive Drab",                                 CIELabFromRGB(24f, 20f, 12f, 100f) },
            { "Olivine",                                    CIELabFromRGB(60f, 73f, 45f, 100f) },
            { "Onyx",                                       CIELabFromRGB(21f, 22f, 22f, 100f) },
            { "Opera Mauve",                                CIELabFromRGB(72f, 52f, 65f, 100f) },
            { "Orange (Color Wheel)",                       CIELabFromRGB(100f, 50f, 0f, 100f) },
            { "Orange (Crayola)",                           CIELabFromRGB(100f, 46f, 22f, 100f) },
            { "Orange (Pantone)",                           CIELabFromRGB(100f, 35f, 0f, 100f) },
            { "Orange (RYB)",                               CIELabFromRGB(98f, 60f, 1f, 100f) },
            { "Orange (Web)",                               CIELabFromRGB(100f, 65f, 0f, 100f) },
            { "Orange Peel",                                CIELabFromRGB(100f, 62f, 0f, 100f) },
            { "Orange-Red",                                 CIELabFromRGB(100f, 27f, 0f, 100f) },
            { "Orange-Yellow",                              CIELabFromRGB(97f, 84f, 41f, 100f) },
            { "Orchid",                                     CIELabFromRGB(85f, 44f, 84f, 100f) },
            { "Orchid Pink",                                CIELabFromRGB(95f, 74f, 80f, 100f) },
            { "Orioles Orange",                             CIELabFromRGB(98f, 31f, 8f, 100f) },
            { "Otter Brown",                                CIELabFromRGB(40f, 26f, 13f, 100f) },
            { "Outer Space",                                CIELabFromRGB(25f, 29f, 30f, 100f) },
            { "Outrageous Orange",                          CIELabFromRGB(100f, 43f, 29f, 100f) },
            { "Oxford Blue",                                CIELabFromRGB(0f, 13f, 28f, 100f) },
            { "OU Crimson Red",                             CIELabFromRGB(60f, 0f, 0f, 100f) },
            { "Pakistan Green",                             CIELabFromRGB(0f, 40f, 0f, 100f) },
            { "Palatinate Blue",                            CIELabFromRGB(15f, 23f, 89f, 100f) },
            { "Palatinate Purple",                          CIELabFromRGB(41f, 16f, 38f, 100f) },
            { "Pale Aqua",                                  CIELabFromRGB(74f, 83f, 90f, 100f) },
            { "Pale Blue",                                  CIELabFromRGB(69f, 93f, 93f, 100f) },
            { "Pale Brown",                                 CIELabFromRGB(60f, 46f, 33f, 100f) },
            { "Pale Carmine",                               CIELabFromRGB(69f, 25f, 21f, 100f) },
            { "Pale Cerulean",                              CIELabFromRGB(61f, 77f, 89f, 100f) },
            { "Pale Chestnut",                              CIELabFromRGB(87f, 68f, 69f, 100f) },
            { "Pale Copper",                                CIELabFromRGB(85f, 54f, 40f, 100f) },
            { "Pale Cornflower Blue",                       CIELabFromRGB(67f, 80f, 94f, 100f) },
            { "Pale Cyan",                                  CIELabFromRGB(53f, 83f, 97f, 100f) },
            { "Pale Gold",                                  CIELabFromRGB(90f, 75f, 54f, 100f) },
            { "Pale Goldenrod",                             CIELabFromRGB(93f, 91f, 67f, 100f) },
            { "Pale Green",                                 CIELabFromRGB(60f, 98f, 60f, 100f) },
            { "Pale Lavender",                              CIELabFromRGB(86f, 82f, 100f, 100f) },
            { "Pale Magenta",                               CIELabFromRGB(98f, 52f, 90f, 100f) },
            { "Pale Magenta-Pink",                          CIELabFromRGB(100f, 60f, 80f, 100f) },
            { "Pale Pink",                                  CIELabFromRGB(98f, 85f, 87f, 100f) },
            { "Pale Plum",                                  CIELabFromRGB(87f, 63f, 87f, 100f) },
            { "Pale Red-Violet",                            CIELabFromRGB(86f, 44f, 58f, 100f) },
            { "Pale Robin Egg Blue",                        CIELabFromRGB(59f, 87f, 82f, 100f) },
            { "Pale Silver",                                CIELabFromRGB(79f, 75f, 73f, 100f) },
            { "Pale Spring Bud",                            CIELabFromRGB(93f, 92f, 74f, 100f) },
            { "Pale Taupe",                                 CIELabFromRGB(74f, 60f, 49f, 100f) },
            { "Pale Turquoise",                             CIELabFromRGB(69f, 93f, 93f, 100f) },
            { "Pale Violet",                                CIELabFromRGB(80f, 60f, 100f, 100f) },
            { "Pale Violet-Red",                            CIELabFromRGB(86f, 44f, 58f, 100f) },
            { "Pansy Purple",                               CIELabFromRGB(47f, 9f, 29f, 100f) },
            { "Paolo Veronese Green",                       CIELabFromRGB(0f, 61f, 49f, 100f) },
            { "Papaya Whip",                                CIELabFromRGB(100f, 94f, 84f, 100f) },
            { "Paradise Pink",                              CIELabFromRGB(90f, 24f, 38f, 100f) },
            { "Paris Green",                                CIELabFromRGB(31f, 78f, 47f, 100f) },
            { "Pastel Blue",                                CIELabFromRGB(68f, 78f, 81f, 100f) },
            { "Pastel Brown",                               CIELabFromRGB(51f, 41f, 33f, 100f) },
            { "Pastel Gray",                                CIELabFromRGB(81f, 81f, 77f, 100f) },
            { "Pastel Green",                               CIELabFromRGB(47f, 87f, 47f, 100f) },
            { "Pastel Magenta",                             CIELabFromRGB(96f, 60f, 76f, 100f) },
            { "Pastel Orange",                              CIELabFromRGB(100f, 70f, 28f, 100f) },
            { "Pastel Pink",                                CIELabFromRGB(87f, 65f, 64f, 100f) },
            { "Pastel Purple",                              CIELabFromRGB(70f, 62f, 71f, 100f) },
            { "Pastel Red",                                 CIELabFromRGB(100f, 41f, 38f, 100f) },
            { "Pastel Violet",                              CIELabFromRGB(80f, 60f, 79f, 100f) },
            { "Pastel Yellow",                              CIELabFromRGB(99f, 99f, 59f, 100f) },
            { "Patriarch",                                  CIELabFromRGB(50f, 0f, 50f, 100f) },
            { "Payne's Grey",                               CIELabFromRGB(33f, 41f, 47f, 100f) },
            { "Peachier",                                   CIELabFromRGB(100f, 90f, 71f, 100f) },
            { "Peach",                                      CIELabFromRGB(100f, 80f, 64f, 100f) },
            { "Peach-Orange",                               CIELabFromRGB(100f, 80f, 60f, 100f) },
            { "Peach Puff",                                 CIELabFromRGB(100f, 85f, 73f, 100f) },
            { "Peach-Yellow",                               CIELabFromRGB(98f, 87f, 68f, 100f) },
            { "Pear",                                       CIELabFromRGB(82f, 89f, 19f, 100f) },
            { "Pearl",                                      CIELabFromRGB(92f, 88f, 78f, 100f) },
            { "Pearl Aqua",                                 CIELabFromRGB(53f, 85f, 75f, 100f) },
            { "Pearly Purple",                              CIELabFromRGB(72f, 41f, 64f, 100f) },
            { "Peridot",                                    CIELabFromRGB(90f, 89f, 0f, 100f) },
            { "Periwinkle",                                 CIELabFromRGB(80f, 80f, 100f, 100f) },
            { "Persian Blue",                               CIELabFromRGB(11f, 22f, 73f, 100f) },
            { "Persian Green",                              CIELabFromRGB(0f, 65f, 58f, 100f) },
            { "Persian Indigo",                             CIELabFromRGB(20f, 7f, 48f, 100f) },
            { "Persian Orange",                             CIELabFromRGB(85f, 56f, 35f, 100f) },
            { "Persian Pink",                               CIELabFromRGB(97f, 50f, 75f, 100f) },
            { "Persian Plum",                               CIELabFromRGB(44f, 11f, 11f, 100f) },
            { "Persian Red",                                CIELabFromRGB(80f, 20f, 20f, 100f) },
            { "Persian Rose",                               CIELabFromRGB(100f, 16f, 64f, 100f) },
            { "Persimmon",                                  CIELabFromRGB(93f, 35f, 0f, 100f) },
            { "Peru",                                       CIELabFromRGB(80f, 52f, 25f, 100f) },
            { "Phlox",                                      CIELabFromRGB(87f, 0f, 100f, 100f) },
            { "Phthalo Blue",                               CIELabFromRGB(0f, 6f, 54f, 100f) },
            { "Phthalo Green",                              CIELabFromRGB(7f, 21f, 14f, 100f) },
            { "Picton Blue",                                CIELabFromRGB(27f, 69f, 91f, 100f) },
            { "Pictorial Carmine",                          CIELabFromRGB(76f, 4f, 31f, 100f) },
            { "Piggy Pink",                                 CIELabFromRGB(99f, 87f, 90f, 100f) },
            { "Pine Green",                                 CIELabFromRGB(0f, 47f, 44f, 100f) },
            { "Pineapple",                                  CIELabFromRGB(34f, 24f, 5f, 100f) },
            { "Pink",                                       CIELabFromRGB(100f, 75f, 80f, 100f) },
            { "Pink (Pantone)",                             CIELabFromRGB(84f, 28f, 58f, 100f) },
            { "Pink Lace",                                  CIELabFromRGB(100f, 87f, 96f, 100f) },
            { "Pink Lavender",                              CIELabFromRGB(85f, 70f, 82f, 100f) },
            { "Pink-Orange",                                CIELabFromRGB(100f, 60f, 40f, 100f) },
            { "Pink Pearl",                                 CIELabFromRGB(91f, 67f, 81f, 100f) },
            { "Pink Raspberry",                             CIELabFromRGB(60f, 0f, 21f, 100f) },
            { "Pink Sherbet",                               CIELabFromRGB(97f, 56f, 65f, 100f) },
            { "Pistachio",                                  CIELabFromRGB(58f, 77f, 45f, 100f) },
            { "Platinum",                                   CIELabFromRGB(90f, 89f, 89f, 100f) },
            { "Plum",                                       CIELabFromRGB(56f, 27f, 52f, 100f) },
            { "Plum (Web)",                                 CIELabFromRGB(87f, 63f, 87f, 100f) },
            { "Pomp And Power",                             CIELabFromRGB(53f, 38f, 56f, 100f) },
            { "Popstar",                                    CIELabFromRGB(75f, 31f, 38f, 100f) },
            { "Portland Orange",                            CIELabFromRGB(100f, 35f, 21f, 100f) },
            { "Powder Blue",                                CIELabFromRGB(69f, 88f, 90f, 100f) },
            { "Princeton Orange",                           CIELabFromRGB(96f, 50f, 15f, 100f) },
            { "Prune",                                      CIELabFromRGB(44f, 11f, 11f, 100f) },
            { "Prussian Blue",                              CIELabFromRGB(0f, 19f, 33f, 100f) },
            { "Psychedelic Purple",                         CIELabFromRGB(87f, 0f, 100f, 100f) },
            { "Puce",                                       CIELabFromRGB(80f, 53f, 60f, 100f) },
            { "Puce Red",                                   CIELabFromRGB(45f, 18f, 22f, 100f) },
            { "Pullman Brown (UPS Brown)",                  CIELabFromRGB(39f, 25f, 9f, 100f) },
            { "Pullman Green",                              CIELabFromRGB(23f, 20f, 11f, 100f) },
            { "Pumpkin",                                    CIELabFromRGB(100f, 46f, 9f, 100f) },
            { "Purple (HTML)",                              CIELabFromRGB(50f, 0f, 50f, 100f) },
            { "Purple (Munsell)",                           CIELabFromRGB(62f, 0f, 77f, 100f) },
            { "Purple (X11)",                               CIELabFromRGB(63f, 13f, 94f, 100f) },
            { "Purple Heart",                               CIELabFromRGB(41f, 21f, 61f, 100f) },
            { "Purple Mountain Majesty",                    CIELabFromRGB(59f, 47f, 71f, 100f) },
            { "Purple Navy",                                CIELabFromRGB(31f, 32f, 50f, 100f) },
            { "Purple Pizzazz",                             CIELabFromRGB(100f, 31f, 85f, 100f) },
            { "Purple Taupe",                               CIELabFromRGB(31f, 25f, 30f, 100f) },
            { "Purpureus",                                  CIELabFromRGB(60f, 31f, 68f, 100f) },
            { "Quartz",                                     CIELabFromRGB(32f, 28f, 31f, 100f) },
            { "Queen Blue",                                 CIELabFromRGB(26f, 42f, 58f, 100f) },
            { "Queen Pink",                                 CIELabFromRGB(91f, 80f, 84f, 100f) },
            { "Quinacridone Magenta",                       CIELabFromRGB(56f, 23f, 35f, 100f) },
            { "Rackley",                                    CIELabFromRGB(36f, 54f, 66f, 100f) },
            { "Radical Red",                                CIELabFromRGB(100f, 21f, 37f, 100f) },
            { "Rajah",                                      CIELabFromRGB(98f, 67f, 38f, 100f) },
            { "Raspberry",                                  CIELabFromRGB(89f, 4f, 36f, 100f) },
            { "Raspberry Glace",                            CIELabFromRGB(57f, 37f, 43f, 100f) },
            { "Raspberry Pink",                             CIELabFromRGB(89f, 31f, 60f, 100f) },
            { "Raspberry Rose",                             CIELabFromRGB(70f, 27f, 42f, 100f) },
            { "Raw Umber",                                  CIELabFromRGB(51f, 40f, 27f, 100f) },
            { "Razzle Dazzle Rose",                         CIELabFromRGB(100f, 20f, 80f, 100f) },
            { "Razzmatazz",                                 CIELabFromRGB(89f, 15f, 42f, 100f) },
            { "Razzmic Berry",                              CIELabFromRGB(55f, 31f, 52f, 100f) },
            { "Rebecca Purple",                             CIELabFromRGB(40f, 20f, 60f, 100f) },
            { "Red",                                        CIELabFromRGB(100f, 0f, 0f, 100f) },
            { "Red (Crayola)",                              CIELabFromRGB(93f, 13f, 30f, 100f) },
            { "Red (Munsell)",                              CIELabFromRGB(95f, 0f, 24f, 100f) },
            { "Red (NCS)",                                  CIELabFromRGB(77f, 1f, 20f, 100f) },
            { "Red (Pantone)",                              CIELabFromRGB(93f, 16f, 22f, 100f) },
            { "Red (Pigment)",                              CIELabFromRGB(93f, 11f, 14f, 100f) },
            { "Red (RYB)",                                  CIELabFromRGB(100f, 15f, 7f, 100f) },
            { "Red-Brown",                                  CIELabFromRGB(65f, 16f, 16f, 100f) },
            { "Red Devil",                                  CIELabFromRGB(53f, 0f, 7f, 100f) },
            { "Red-Orange",                                 CIELabFromRGB(100f, 33f, 29f, 100f) },
            { "Red-Purple",                                 CIELabFromRGB(89f, 0f, 47f, 100f) },
            { "Red-Violet",                                 CIELabFromRGB(78f, 8f, 52f, 100f) },
            { "Redwood",                                    CIELabFromRGB(64f, 35f, 32f, 100f) },
            { "Regalia",                                    CIELabFromRGB(32f, 18f, 50f, 100f) },
            { "Registration Black",                         CIELabFromRGB(0f, 0f, 0f, 100f) },
            { "Resolution Blue",                            CIELabFromRGB(0f, 14f, 53f, 100f) },
            { "Rhythm",                                     CIELabFromRGB(47f, 46f, 59f, 100f) },
            { "Rich Black",                                 CIELabFromRGB(0f, 25f, 25f, 100f) },
            { "Rich Black (FOGRA29)",                       CIELabFromRGB(0f, 4f, 7f, 100f) },
            { "Rich Black (FOGRA39)",                       CIELabFromRGB(0f, 1f, 1f, 100f) },
            { "Rich Brilliant Lavender",                    CIELabFromRGB(95f, 65f, 100f, 100f) },
            { "Rich Carmine",                               CIELabFromRGB(84f, 0f, 25f, 100f) },
            { "Rich Electric Blue",                         CIELabFromRGB(3f, 57f, 82f, 100f) },
            { "Rich Lavender",                              CIELabFromRGB(65f, 42f, 81f, 100f) },
            { "Rich Lilac",                                 CIELabFromRGB(71f, 40f, 82f, 100f) },
            { "Rich Maroon",                                CIELabFromRGB(69f, 19f, 38f, 100f) },
            { "Rifle Green",                                CIELabFromRGB(27f, 30f, 22f, 100f) },
            { "Roast Coffee",                               CIELabFromRGB(44f, 26f, 25f, 100f) },
            { "Robin Egg Blue",                             CIELabFromRGB(0f, 80f, 80f, 100f) },
            { "Rocket Metallic",                            CIELabFromRGB(54f, 50f, 50f, 100f) },
            { "Roman Silver",                               CIELabFromRGB(51f, 54f, 59f, 100f) },
            { "Rose",                                       CIELabFromRGB(100f, 0f, 50f, 100f) },
            { "Rose Bonbon",                                CIELabFromRGB(98f, 26f, 62f, 100f) },
            { "Rose Ebony",                                 CIELabFromRGB(40f, 28f, 27f, 100f) },
            { "Rose Gold",                                  CIELabFromRGB(72f, 43f, 47f, 100f) },
            { "Rose Madder",                                CIELabFromRGB(89f, 15f, 21f, 100f) },
            { "Rose Pink",                                  CIELabFromRGB(100f, 40f, 80f, 100f) },
            { "Rose Quartz",                                CIELabFromRGB(67f, 60f, 66f, 100f) },
            { "Rose Red",                                   CIELabFromRGB(76f, 12f, 34f, 100f) },
            { "Rose Taupe",                                 CIELabFromRGB(56f, 36f, 36f, 100f) },
            { "Rose Vale",                                  CIELabFromRGB(67f, 31f, 32f, 100f) },
            { "Rosewood",                                   CIELabFromRGB(40f, 0f, 4f, 100f) },
            { "Rosso Corsa",                                CIELabFromRGB(83f, 0f, 0f, 100f) },
            { "Rosy Brown",                                 CIELabFromRGB(74f, 56f, 56f, 100f) },
            { "Royal Azure",                                CIELabFromRGB(0f, 22f, 66f, 100f) },
            { "Royal Blue",                                 CIELabFromRGB(0f, 14f, 40f, 100f) },
            { "Royal Blue 2",                               CIELabFromRGB(25f, 41f, 88f, 100f) },
            { "Royal Fuchsia",                              CIELabFromRGB(79f, 17f, 57f, 100f) },
            { "Royal Purple",                               CIELabFromRGB(47f, 32f, 66f, 100f) },
            { "Royal Yellow",                               CIELabFromRGB(98f, 85f, 37f, 100f) },
            { "Ruber",                                      CIELabFromRGB(81f, 27f, 46f, 100f) },
            { "Rubine Red",                                 CIELabFromRGB(82f, 0f, 34f, 100f) },
            { "Ruby",                                       CIELabFromRGB(88f, 7f, 37f, 100f) },
            { "Ruby Red",                                   CIELabFromRGB(61f, 7f, 12f, 100f) },
            { "Ruddy",                                      CIELabFromRGB(100f, 0f, 16f, 100f) },
            { "Ruddy Brown",                                CIELabFromRGB(73f, 40f, 16f, 100f) },
            { "Ruddy Pink",                                 CIELabFromRGB(88f, 56f, 59f, 100f) },
            { "Rufous",                                     CIELabFromRGB(66f, 11f, 3f, 100f) },
            { "Russet",                                     CIELabFromRGB(50f, 27f, 11f, 100f) },
            { "Russian Green",                              CIELabFromRGB(40f, 57f, 40f, 100f) },
            { "Russian Violet",                             CIELabFromRGB(20f, 9f, 30f, 100f) },
            { "Rust",                                       CIELabFromRGB(72f, 25f, 5f, 100f) },
            { "Rusty Red",                                  CIELabFromRGB(85f, 17f, 26f, 100f) },
            { "Sacramento State Green",                     CIELabFromRGB(0f, 34f, 25f, 100f) },
            { "Saddle Brown",                               CIELabFromRGB(55f, 27f, 7f, 100f) },
            { "Safety Orange",                              CIELabFromRGB(100f, 47f, 0f, 100f) },
            { "Safety Orange (Blaze Orange)",               CIELabFromRGB(100f, 40f, 0f, 100f) },
            { "Safety Yellow",                              CIELabFromRGB(93f, 82f, 1f, 100f) },
            { "Saffron",                                    CIELabFromRGB(96f, 77f, 19f, 100f) },
            { "Sage",                                       CIELabFromRGB(74f, 72f, 54f, 100f) },
            { "St. Patrick's Blue",                         CIELabFromRGB(14f, 16f, 48f, 100f) },
            { "Salmon",                                     CIELabFromRGB(98f, 50f, 45f, 100f) },
            { "Salmon Pink",                                CIELabFromRGB(100f, 57f, 64f, 100f) },
            { "Sand",                                       CIELabFromRGB(76f, 70f, 50f, 100f) },
            { "Sand Dune",                                  CIELabFromRGB(59f, 44f, 9f, 100f) },
            { "Sandstorm",                                  CIELabFromRGB(93f, 84f, 25f, 100f) },
            { "Sandy Brown",                                CIELabFromRGB(96f, 64f, 38f, 100f) },
            { "Sandy Taupe",                                CIELabFromRGB(59f, 44f, 9f, 100f) },
            { "Sangria",                                    CIELabFromRGB(57f, 0f, 4f, 100f) },
            { "Sap Green",                                  CIELabFromRGB(31f, 49f, 16f, 100f) },
            { "Sapphire",                                   CIELabFromRGB(6f, 32f, 73f, 100f) },
            { "Sapphire Blue",                              CIELabFromRGB(0f, 40f, 65f, 100f) },
            { "Satin Sheen Gold",                           CIELabFromRGB(80f, 63f, 21f, 100f) },
            { "Scarlet",                                    CIELabFromRGB(100f, 14f, 0f, 100f) },
            { "Scarlet-ier",                                CIELabFromRGB(99f, 5f, 21f, 100f) },
            { "Schauss Pink",                               CIELabFromRGB(100f, 57f, 69f, 100f) },
            { "School Bus Yellow",                          CIELabFromRGB(100f, 85f, 0f, 100f) },
            { "Screamin' Green",                            CIELabFromRGB(46f, 100f, 48f, 100f) },
            { "Sea Blue",                                   CIELabFromRGB(0f, 41f, 58f, 100f) },
            { "Sea Green",                                  CIELabFromRGB(18f, 55f, 34f, 100f) },
            { "Seal Brown",                                 CIELabFromRGB(20f, 8f, 8f, 100f) },
            { "Seashell",                                   CIELabFromRGB(100f, 96f, 93f, 100f) },
            { "Selective Yellow",                           CIELabFromRGB(100f, 73f, 0f, 100f) },
            { "Sepia",                                      CIELabFromRGB(44f, 26f, 8f, 100f) },
            { "Shadow",                                     CIELabFromRGB(54f, 47f, 36f, 100f) },
            { "Shadow Blue",                                CIELabFromRGB(47f, 55f, 65f, 100f) },
            { "Shampoo",                                    CIELabFromRGB(100f, 81f, 95f, 100f) },
            { "Shamrock Green",                             CIELabFromRGB(0f, 62f, 38f, 100f) },
            { "Sheen Green",                                CIELabFromRGB(56f, 83f, 0f, 100f) },
            { "Shimmering Blush",                           CIELabFromRGB(85f, 53f, 58f, 100f) },
            { "Shocking Pink",                              CIELabFromRGB(99f, 6f, 75f, 100f) },
            { "Shocking Pink (Crayola)",                    CIELabFromRGB(100f, 44f, 100f, 100f) },
            { "Sienna",                                     CIELabFromRGB(53f, 18f, 9f, 100f) },
            { "Silver",                                     CIELabFromRGB(75f, 75f, 75f, 100f) },
            { "Silver Chalice",                             CIELabFromRGB(67f, 67f, 67f, 100f) },
            { "Silver Lake Blue",                           CIELabFromRGB(36f, 54f, 73f, 100f) },
            { "Silver Pink",                                CIELabFromRGB(77f, 68f, 68f, 100f) },
            { "Silver Sand",                                CIELabFromRGB(75f, 76f, 76f, 100f) },
            { "Sinopia",                                    CIELabFromRGB(80f, 25f, 4f, 100f) },
            { "Skobeloff",                                  CIELabFromRGB(0f, 45f, 45f, 100f) },
            { "Sky Blue",                                   CIELabFromRGB(53f, 81f, 92f, 100f) },
            { "Sky Magenta",                                CIELabFromRGB(81f, 44f, 69f, 100f) },
            { "Slate Blue",                                 CIELabFromRGB(42f, 35f, 80f, 100f) },
            { "Slate Gray",                                 CIELabFromRGB(44f, 50f, 56f, 100f) },
            { "Smalt (Dark Powder Blue)",                   CIELabFromRGB(0f, 20f, 60f, 100f) },
            { "Smitten",                                    CIELabFromRGB(78f, 25f, 53f, 100f) },
            { "Smoke",                                      CIELabFromRGB(45f, 51f, 46f, 100f) },
            { "Smoky Black",                                CIELabFromRGB(6f, 5f, 3f, 100f) },
            { "Smoky Topaz",                                CIELabFromRGB(58f, 24f, 25f, 100f) },
            { "Snow",                                       CIELabFromRGB(100f, 98f, 98f, 100f) },
            { "Soap",                                       CIELabFromRGB(81f, 78f, 94f, 100f) },
            { "Solid Pink",                                 CIELabFromRGB(54f, 22f, 26f, 100f) },
            { "Sonic Silver",                               CIELabFromRGB(46f, 46f, 46f, 100f) },
            { "Spartan Crimson",                            CIELabFromRGB(62f, 7f, 9f, 100f) },
            { "Space Cadet",                                CIELabFromRGB(11f, 16f, 32f, 100f) },
            { "Spanish Bistre",                             CIELabFromRGB(50f, 46f, 20f, 100f) },
            { "Spanish Blue",                               CIELabFromRGB(0f, 44f, 72f, 100f) },
            { "Spanish Carmine",                            CIELabFromRGB(82f, 0f, 28f, 100f) },
            { "Spanish Crimson",                            CIELabFromRGB(90f, 10f, 30f, 100f) },
            { "Spanish Gray",                               CIELabFromRGB(60f, 60f, 60f, 100f) },
            { "Spanish Green",                              CIELabFromRGB(0f, 57f, 31f, 100f) },
            { "Spanish Orange",                             CIELabFromRGB(91f, 38f, 0f, 100f) },
            { "Spanish Pink",                               CIELabFromRGB(97f, 75f, 75f, 100f) },
            { "Spanish Red",                                CIELabFromRGB(90f, 0f, 15f, 100f) },
            { "Spanish Sky Blue",                           CIELabFromRGB(0f, 100f, 100f, 100f) },
            { "Spanish Violet",                             CIELabFromRGB(30f, 16f, 51f, 100f) },
            { "Spanish Viridian",                           CIELabFromRGB(0f, 50f, 36f, 100f) },
            { "Spicy Mix",                                  CIELabFromRGB(55f, 37f, 30f, 100f) },
            { "Spiro Disco Ball",                           CIELabFromRGB(6f, 75f, 99f, 100f) },
            { "Spring Bud",                                 CIELabFromRGB(65f, 99f, 0f, 100f) },
            { "Spring Green",                               CIELabFromRGB(0f, 100f, 50f, 100f) },
            { "Star Command Blue",                          CIELabFromRGB(0f, 48f, 72f, 100f) },
            { "Steel Blue",                                 CIELabFromRGB(27f, 51f, 71f, 100f) },
            { "Steel Pink",                                 CIELabFromRGB(80f, 20f, 80f, 100f) },
            { "Stil De Grain Yellow",                       CIELabFromRGB(98f, 85f, 37f, 100f) },
            { "Stizza",                                     CIELabFromRGB(60f, 0f, 0f, 100f) },
            { "Stormcloud",                                 CIELabFromRGB(31f, 40f, 42f, 100f) },
            { "Thistle",                                    CIELabFromRGB(85f, 75f, 85f, 100f) },
            { "Straw",                                      CIELabFromRGB(89f, 85f, 44f, 100f) },
            { "Strawberry",                                 CIELabFromRGB(99f, 35f, 55f, 100f) },
            { "Sunglow",                                    CIELabFromRGB(100f, 80f, 20f, 100f) },
            { "Sunray",                                     CIELabFromRGB(89f, 67f, 34f, 100f) },
            { "Sunset",                                     CIELabFromRGB(98f, 84f, 65f, 100f) },
            { "Sunset Orange",                              CIELabFromRGB(99f, 37f, 33f, 100f) },
            { "Super Pink",                                 CIELabFromRGB(81f, 42f, 66f, 100f) },
            { "Tan",                                        CIELabFromRGB(82f, 71f, 55f, 100f) },
            { "Tangelo",                                    CIELabFromRGB(98f, 30f, 0f, 100f) },
            { "Tangerine",                                  CIELabFromRGB(95f, 52f, 0f, 100f) },
            { "Tangerine Yellow",                           CIELabFromRGB(100f, 80f, 0f, 100f) },
            { "Tango Pink",                                 CIELabFromRGB(89f, 44f, 48f, 100f) },
            { "Taupe",                                      CIELabFromRGB(28f, 24f, 20f, 100f) },
            { "Taupe Gray",                                 CIELabFromRGB(55f, 52f, 54f, 100f) },
            { "Tea Green",                                  CIELabFromRGB(82f, 94f, 75f, 100f) },
            { "Tea Rose",                                   CIELabFromRGB(97f, 51f, 47f, 100f) },
            { "Tea Rosier",                                 CIELabFromRGB(96f, 76f, 76f, 100f) },
            { "Teal",                                       CIELabFromRGB(0f, 50f, 50f, 100f) },
            { "Teal Blue",                                  CIELabFromRGB(21f, 46f, 53f, 100f) },
            { "Teal Deer",                                  CIELabFromRGB(60f, 90f, 70f, 100f) },
            { "Teal Green",                                 CIELabFromRGB(0f, 51f, 50f, 100f) },
            { "Telemagenta",                                CIELabFromRGB(81f, 20f, 46f, 100f) },
            { "Tenné",                                      CIELabFromRGB(80f, 34f, 0f, 100f) },
            { "Terra Cotta",                                CIELabFromRGB(89f, 45f, 36f, 100f) },
            { "Thulian Pink",                               CIELabFromRGB(87f, 44f, 63f, 100f) },
            { "Tickle Me Pink",                             CIELabFromRGB(99f, 54f, 67f, 100f) },
            { "Tiffany Blue",                               CIELabFromRGB(4f, 73f, 71f, 100f) },
            { "Tiger's Eye",                                CIELabFromRGB(88f, 55f, 24f, 100f) },
            { "Timberwolf",                                 CIELabFromRGB(86f, 84f, 82f, 100f) },
            { "Titanium Yellow",                            CIELabFromRGB(93f, 90f, 0f, 100f) },
            { "Tomato",                                     CIELabFromRGB(100f, 39f, 28f, 100f) },
            { "Toolbox",                                    CIELabFromRGB(45f, 42f, 75f, 100f) },
            { "Topaz",                                      CIELabFromRGB(100f, 78f, 49f, 100f) },
            { "Tractor Red",                                CIELabFromRGB(99f, 5f, 21f, 100f) },
            { "Trolley Grey",                               CIELabFromRGB(50f, 50f, 50f, 100f) },
            { "Tropical Rain Forest",                       CIELabFromRGB(0f, 46f, 37f, 100f) },
            { "True Blue",                                  CIELabFromRGB(0f, 45f, 81f, 100f) },
            { "Tufts Blue",                                 CIELabFromRGB(25f, 49f, 76f, 100f) },
            { "Tulip",                                      CIELabFromRGB(100f, 53f, 55f, 100f) },
            { "Tumbleweed",                                 CIELabFromRGB(87f, 67f, 53f, 100f) },
            { "Turkish Rose",                               CIELabFromRGB(71f, 45f, 51f, 100f) },
            { "Turquoise",                                  CIELabFromRGB(25f, 88f, 82f, 100f) },
            { "Turquoise Blue",                             CIELabFromRGB(0f, 100f, 94f, 100f) },
            { "Turquoise Green",                            CIELabFromRGB(63f, 84f, 71f, 100f) },
            { "Tuscan",                                     CIELabFromRGB(98f, 84f, 65f, 100f) },
            { "Tuscan Brown",                               CIELabFromRGB(44f, 31f, 22f, 100f) },
            { "Tuscan Red",                                 CIELabFromRGB(49f, 28f, 28f, 100f) },
            { "Tuscan Tan",                                 CIELabFromRGB(65f, 48f, 36f, 100f) },
            { "Tuscany",                                    CIELabFromRGB(75f, 60f, 60f, 100f) },
            { "Twilight Lavender",                          CIELabFromRGB(54f, 29f, 42f, 100f) },
            { "Tyrian Purple",                              CIELabFromRGB(40f, 1f, 24f, 100f) },
            { "UA Blue",                                    CIELabFromRGB(0f, 20f, 67f, 100f) },
            { "UA Red",                                     CIELabFromRGB(85f, 0f, 30f, 100f) },
            { "Ube",                                        CIELabFromRGB(53f, 47f, 76f, 100f) },
            { "UCLA Blue",                                  CIELabFromRGB(33f, 41f, 58f, 100f) },
            { "UCLA Gold",                                  CIELabFromRGB(100f, 70f, 0f, 100f) },
            { "UFO Green",                                  CIELabFromRGB(24f, 82f, 44f, 100f) },
            { "Ultramarine",                                CIELabFromRGB(7f, 4f, 56f, 100f) },
            { "Ultramarine Blue",                           CIELabFromRGB(25f, 40f, 96f, 100f) },
            { "Ultra Pink",                                 CIELabFromRGB(100f, 44f, 100f, 100f) },
            { "Ultra Red",                                  CIELabFromRGB(99f, 42f, 52f, 100f) },
            { "Umber",                                      CIELabFromRGB(39f, 32f, 28f, 100f) },
            { "Unbleached Silk",                            CIELabFromRGB(100f, 87f, 79f, 100f) },
            { "United Nations Blue",                        CIELabFromRGB(36f, 57f, 90f, 100f) },
            { "University Of California Gold",              CIELabFromRGB(72f, 53f, 15f, 100f) },
            { "Unmellow Yellow",                            CIELabFromRGB(100f, 100f, 40f, 100f) },
            { "UP Forest Green",                            CIELabFromRGB(0f, 27f, 13f, 100f) },
            { "UP Maroon",                                  CIELabFromRGB(48f, 7f, 7f, 100f) },
            { "Upsdell Red",                                CIELabFromRGB(68f, 13f, 16f, 100f) },
            { "Urobilin",                                   CIELabFromRGB(88f, 68f, 13f, 100f) },
            { "USAFA Blue",                                 CIELabFromRGB(0f, 31f, 60f, 100f) },
            { "USC Cardinal",                               CIELabFromRGB(60f, 0f, 0f, 100f) },
            { "USC Gold",                                   CIELabFromRGB(100f, 80f, 0f, 100f) },
            { "University Of Tennessee Orange",             CIELabFromRGB(97f, 50f, 0f, 100f) },
            { "Utah Crimson",                               CIELabFromRGB(83f, 0f, 25f, 100f) },
            { "Vanilla",                                    CIELabFromRGB(95f, 90f, 67f, 100f) },
            { "Vanilla Ice",                                CIELabFromRGB(95f, 56f, 66f, 100f) },
            { "Vegas Gold",                                 CIELabFromRGB(77f, 70f, 35f, 100f) },
            { "Venetian Red",                               CIELabFromRGB(78f, 3f, 8f, 100f) },
            { "Verdigris",                                  CIELabFromRGB(26f, 70f, 68f, 100f) },
            { "Vermilion",                                  CIELabFromRGB(89f, 26f, 20f, 100f) },
            { "Vermilion 2",                                CIELabFromRGB(85f, 22f, 12f, 100f) },
            { "Veronica",                                   CIELabFromRGB(63f, 13f, 94f, 100f) },
            { "Very Light Azure",                           CIELabFromRGB(45f, 73f, 98f, 100f) },
            { "Very Light Blue",                            CIELabFromRGB(40f, 40f, 100f, 100f) },
            { "Very Light Malachite Green",                 CIELabFromRGB(39f, 91f, 53f, 100f) },
            { "Very Light Tangelo",                         CIELabFromRGB(100f, 69f, 47f, 100f) },
            { "Very Pale Orange",                           CIELabFromRGB(100f, 87f, 75f, 100f) },
            { "Very Pale Yellow",                           CIELabFromRGB(100f, 100f, 75f, 100f) },
            { "Violet",                                     CIELabFromRGB(56f, 0f, 100f, 100f) },
            { "Violet (Color Wheel)",                       CIELabFromRGB(50f, 0f, 100f, 100f) },
            { "Violet (RYB)",                               CIELabFromRGB(53f, 0f, 69f, 100f) },
            { "Violet (Web)",                               CIELabFromRGB(93f, 51f, 93f, 100f) },
            { "Violet-Blue",                                CIELabFromRGB(20f, 29f, 70f, 100f) },
            { "Violet-Red",                                 CIELabFromRGB(97f, 33f, 58f, 100f) },
            { "Viridian",                                   CIELabFromRGB(25f, 51f, 43f, 100f) },
            { "Viridian Green",                             CIELabFromRGB(0f, 59f, 60f, 100f) },
            { "Vista Blue",                                 CIELabFromRGB(49f, 62f, 85f, 100f) },
            { "Vivid Amber",                                CIELabFromRGB(80f, 60f, 0f, 100f) },
            { "Vivid Auburn",                               CIELabFromRGB(57f, 15f, 14f, 100f) },
            { "Vivid Burgundy",                             CIELabFromRGB(62f, 11f, 21f, 100f) },
            { "Vivid Cerise",                               CIELabFromRGB(85f, 11f, 51f, 100f) },
            { "Vivid Cerulean",                             CIELabFromRGB(0f, 67f, 93f, 100f) },
            { "Vivid Crimson",                              CIELabFromRGB(80f, 0f, 20f, 100f) },
            { "Vivid Gamboge",                              CIELabFromRGB(100f, 60f, 0f, 100f) },
            { "Vivid Lime Green",                           CIELabFromRGB(65f, 84f, 3f, 100f) },
            { "Vivid Malachite",                            CIELabFromRGB(0f, 80f, 20f, 100f) },
            { "Vivid Mulberry",                             CIELabFromRGB(72f, 5f, 89f, 100f) },
            { "Vivid Orange",                               CIELabFromRGB(100f, 37f, 0f, 100f) },
            { "Vivid Orange Peel",                          CIELabFromRGB(100f, 63f, 0f, 100f) },
            { "Vivid Orchid",                               CIELabFromRGB(80f, 0f, 100f, 100f) },
            { "Vivid Raspberry",                            CIELabFromRGB(100f, 0f, 42f, 100f) },
            { "Vivid Red",                                  CIELabFromRGB(97f, 5f, 10f, 100f) },
            { "Vivid Red-Tangelo",                          CIELabFromRGB(87f, 38f, 14f, 100f) },
            { "Vivid Sky Blue",                             CIELabFromRGB(0f, 80f, 100f, 100f) },
            { "Vivid Tangelo",                              CIELabFromRGB(94f, 45f, 15f, 100f) },
            { "Vivid Tangerine",                            CIELabFromRGB(100f, 63f, 54f, 100f) },
            { "Vivid Vermilion",                            CIELabFromRGB(90f, 38f, 14f, 100f) },
            { "Vivid Violet",                               CIELabFromRGB(62f, 0f, 100f, 100f) },
            { "Vivid Yellow",                               CIELabFromRGB(100f, 89f, 1f, 100f) },
            { "Warm Black",                                 CIELabFromRGB(0f, 26f, 26f, 100f) },
            { "Waterspout",                                 CIELabFromRGB(64f, 96f, 98f, 100f) },
            { "Wenge",                                      CIELabFromRGB(39f, 33f, 32f, 100f) },
            { "Wheat",                                      CIELabFromRGB(96f, 87f, 70f, 100f) },
            { "White",                                      CIELabFromRGB(100f, 100f, 100f, 100f) },
            { "White Smoke",                                CIELabFromRGB(96f, 96f, 96f, 100f) },
            { "Wild Blue Yonder",                           CIELabFromRGB(64f, 68f, 82f, 100f) },
            { "Wild Orchid",                                CIELabFromRGB(83f, 44f, 64f, 100f) },
            { "Wild Strawberry",                            CIELabFromRGB(100f, 26f, 64f, 100f) },
            { "Wild Watermelon",                            CIELabFromRGB(99f, 42f, 52f, 100f) },
            { "Willpower Orange",                           CIELabFromRGB(99f, 35f, 0f, 100f) },
            { "Windsor Tan",                                CIELabFromRGB(65f, 33f, 1f, 100f) },
            { "Wine",                                       CIELabFromRGB(45f, 18f, 22f, 100f) },
            { "Wine Dregs",                                 CIELabFromRGB(40f, 19f, 28f, 100f) },
            { "Wisteria",                                   CIELabFromRGB(79f, 63f, 86f, 100f) },
            { "Wood Brown",                                 CIELabFromRGB(76f, 60f, 42f, 100f) },
            { "Xanadu",                                     CIELabFromRGB(45f, 53f, 47f, 100f) },
            { "Yale Blue",                                  CIELabFromRGB(6f, 30f, 57f, 100f) },
            { "Yankees Blue",                               CIELabFromRGB(11f, 16f, 25f, 100f) },
            { "Yellow",                                     CIELabFromRGB(100f, 100f, 0f, 100f) },
            { "Yellow (Crayola)",                           CIELabFromRGB(99f, 91f, 51f, 100f) },
            { "Yellow (Munsell)",                           CIELabFromRGB(94f, 80f, 0f, 100f) },
            { "Yellow (NCS)",                               CIELabFromRGB(100f, 83f, 0f, 100f) },
            { "Yellow (Pantone)",                           CIELabFromRGB(100f, 87f, 0f, 100f) },
            { "Yellow (Process)",                           CIELabFromRGB(100f, 94f, 0f, 100f) },
            { "Yellow (RYB)",                               CIELabFromRGB(100f, 100f, 20f, 100f) },
            { "Yellow-Green",                               CIELabFromRGB(60f, 80f, 20f, 100f) },
            { "Yellow Orange",                              CIELabFromRGB(100f, 68f, 26f, 100f) },
            { "Yellow Rose",                                CIELabFromRGB(100f, 94f, 0f, 100f) },
            { "Zaffre",                                     CIELabFromRGB(0f, 8f, 66f, 100f) },
            { "Zinnwaldite Brown",                          CIELabFromRGB(17f, 9f, 3f, 100f) },
            { "Zomp",                                       CIELabFromRGB(22f, 65f, 56f, 100f) }
        };
    }
}
