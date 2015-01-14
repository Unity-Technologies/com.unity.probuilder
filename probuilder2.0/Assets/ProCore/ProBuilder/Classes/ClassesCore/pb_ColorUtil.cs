// using UnityEngine;
// using System.Collections.Generic;

// public static class pb_ColorUtil
// {
// 	private static bool approx(float lhs, float rhs)
// 	{
// 		return Mathf.Abs(lhs-rhs) < Mathf.Epsilon;
// 	}

// 	/**
// 	 * Hue (0,360), Saturation (0,1), Value (0,1)
// 	 */
// 	public class pb_HsvColor
// 	{
// 		public float h, s, v;


// 		public pb_HsvColor(float h, float s, float v)
// 		{
// 			this.h = h;
// 			this.s = s;
// 			this.v = v;
// 		}

// 		/**
// 		 * Wikipedia colors are from 0-100, so this constructor includes and S, V normalizes the values.
// 		 * modifier value that affects saturation and value, making it useful for any SV value range.
// 		 */
// 		public pb_HsvColor(float h, float s, float v, float sv_modifier)
// 		{
// 			this.h = h;
// 			this.s = s * sv_modifier;
// 			this.v = v * sv_modifier;
// 		}

// 		public override string ToString()
// 		{
// 			return string.Format("HSV( {0}, {1}, {2} )", h, s, v);
// 		}

// 		public float SqrDistance(pb_HsvColor InColor)
// 		{
// 			return (InColor.h/360f - this.h/360f) + (InColor.s - this.s) + (InColor.v - this.v);
// 		}
// 	}

// 	// http://www.easyrgb.com/index.php?X=MATH&H=07#text7
// 	class pb_XYZ_Color
// 	{
// 		public float x, y, z;

// 		public pb_XYZ_Color(float x, float y, float z)
// 		{
// 			this.x = x;
// 			this.y = y;
// 			this.z = z;
// 		}

// 		public static pb_XYZ_Color FromRGB(Color col)
// 		{
// 			float r = col.r, g = col.g, b = col.b;
			
// 			if ( r > 0.04045f )
// 				r = Mathf.Pow( ( ( r + 0.055f ) / 1.055f ), 2.4f);
// 			else                   
// 				r = r / 12.92f;

// 			if ( g > 0.04045f ) 
// 				g = Mathf.Pow( ( ( g + 0.055f ) / 1.055f ), 2.4f);
// 			else
// 				g = g / 12.92f;

// 			if ( b > 0.04045f )
// 				b = Mathf.Pow( ( ( b + 0.055f ) / 1.055f ), 2.4f);
// 			else
// 				b = b / 12.92f;

// 			r = r * 100f;
// 			g = g * 100f;
// 			b = b * 100f;

// 			// Observer. = 2°, Illuminant = D65
// 			float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
// 			float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
// 			float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

// 			return new pb_XYZ_Color(x, y, z);
// 		}

// 	}

// 	class pb_CIE_L_Color
// 	{
// 		public float L, a, b;

// 		public static FromXYZ(pb_XYZ_Color col)
// 		{
// 			var_X = X / ref_X          //ref_X =  95.047   Observer= 2°, Illuminant= D65
// 			var_Y = Y / ref_Y          //ref_Y = 100.000
// 			var_Z = Z / ref_Z          //ref_Z = 108.883

// 			if ( var_X > 0.008856 ) var_X = var_X ^ ( 1/3 )
// 			else                    var_X = ( 7.787 * var_X ) + ( 16 / 116 )
// 			if ( var_Y > 0.008856 ) var_Y = var_Y ^ ( 1/3 )
// 			else                    var_Y = ( 7.787 * var_Y ) + ( 16 / 116 )
// 			if ( var_Z > 0.008856 ) var_Z = var_Z ^ ( 1/3 )
// 			else                    var_Z = ( 7.787 * var_Z ) + ( 16 / 116 )

// 			CIE-L* = ( 116 * var_Y ) - 16
// 			CIE-a* = 500 * ( var_X - var_Y )
// 			CIE-b* = 200 * ( var_Y - var_Z )
// 		}
// 	}

// 	// http://www.easyrgb.com/index.php?X=DELT&H=03#text3
// 	static float DeltaE(pb_CIE_L_Color lhs, pb_CIE_L_Color rhs)
// 	{
// 		return Mathf.Sqrt(
// 			Mathf.Pow( (lhs.L - rhs.L), 2 ) +
// 			Mathf.Pow( (lhs.a - rhs.a), 2 ) +
// 			Mathf.Pow( (lhs.b - rhs.b), 2 ) );
// 	}

// 	/**
// 	 * Convert HSV to RGB.
// 	 *  http://www.cs.rit.edu/~ncs/color/t_convert.html
// 	 *	r,g,b values are from 0 to 1
// 	 *	h = [0,360], s = [0,1], v = [0,1]
// 	 *	if s == 0, then h = -1 (undefined)
// 	 */
// 	public static Color HSVtoRGB(float h, float s, float v )
// 	{
// 		float r, g, b;
// 		int i;
// 		float f, p, q, t;
// 		if( s == 0 ) {
// 			// achromatic (grey)
// 			return new Color(v, v, v, 1f);
// 		}
// 		h /= 60;			// sector 0 to 5
// 		i = (int)Mathf.Floor( h );
// 		f = h - i;			// factorial part of h
// 		p = v * ( 1 - s );
// 		q = v * ( 1 - s * f );
// 		t = v * ( 1 - s * ( 1 - f ) );

// 		switch( i )
// 		{
// 			case 0:
// 				r = v;
// 				g = t;
// 				b = p;
// 				break;
// 			case 1:
// 				r = q;
// 				g = v;
// 				b = p;
// 				break;
// 			case 2:
// 				r = p;
// 				g = v;
// 				b = t;
// 				break;
// 			case 3:
// 				r = p;
// 				g = q;
// 				b = v;
// 				break;
// 			case 4:
// 				r = t;
// 				g = p;
// 				b = v;
// 				break;
// 			default:		// case 5:
// 				r = v;
// 				g = p;
// 				b = q;
// 				break;
// 		}
		
// 		return new Color(r, g, b, 1f);
// 	}

// 	/**
// 	 * http://www.cs.rit.edu/~ncs/color/t_convert.html
// 	 * r,g,b values are from 0 to 1
// 	 * h = [0,360], s = [0,1], v = [0,1]
// 	 * 	if s == 0, then h = -1 (undefined)
// 	 */
// 	public static pb_HsvColor RGBtoHSV(Color color)
// 	{
// 		float h, s, v;
// 		float r = color.r, b = color.b, g = color.g;

// 		float min, max, delta;
// 		min = Mathf.Min( Mathf.Min(r, g), b );
// 		max = Mathf.Max( Mathf.Max(r, g), b );

// 		v = max;				// v

// 		delta = max - min;
		
// 		if( max != 0f )
// 		{
// 			s = delta / max;		// s
// 		}
// 		else
// 		{
// 			// r = g = b = 0		// s = 0, v is undefined
// 			s = 0f;
// 			h = 0f;
// 			return new pb_HsvColor(h, s, v);
// 		}

// 		if( approx(r, max) )
// 		{
// 			h = ( g - b ) / delta;		// between yellow & magenta
// 			if(float.IsNaN(h))
// 				h = 0f;
// 		}
// 		else if( approx(g, max) )
// 		{
// 			h = 2f + ( b - r ) / delta;	// between cyan & yellow
// 		}
// 		else
// 		{	
// 			h = 4f + ( r - g ) / delta;	// between magenta & cyan
// 		}

// 		h *= 60f;					// degrees
		
// 		if( h < 0 )
// 			h += 360;

// 		return new pb_HsvColor(h, s, v);
// 	}

// 	/**
// 	 * Get human readable name from a Color.
// 	 */
// 	public static string GetColorName(Color col)
// 	{
// 		pb_HsvColor hsv = RGBtoHSV(col);
// 		string name = "Unknown";
// 		float diff = Mathf.Infinity;

// 		foreach(KeyValuePair<string, pb_HsvColor> kvp in ColorNames)
// 		{
// 			float dist = Mathf.Abs(hsv.SqrDistance(kvp.Value));

// 			if( dist < diff) 
// 			{
// 				diff = dist;
// 				name = kvp.Key;
// 			}
// 		}

// 		return name;
// 	}

// 	static readonly Dictionary<string, pb_HsvColor> ColorNames = new Dictionary<string, pb_HsvColor>()
// 	{
// 		{ "Acid Green",						new pb_HsvColor(65f, 86f, 75f, .01f) 	},
// 		{ "Aero",							new pb_HsvColor(206f, 47f, 91f, .01f) 	},
// 		{ "Aero Blue",						new pb_HsvColor(151f, 21f, 100f, .01f) 	},
// 		{ "African Violet",					new pb_HsvColor(288f, 31f, 75f, .01f) 	},
// 		{ "Air Force Blue (RAF)",			new pb_HsvColor(204f, 45f, 66f, .01f) 	},
// 		{ "Air Force Blue (USAF)",			new pb_HsvColor(220f, 100f, 56f, .01f) 	},
// 		{ "Air Superiority Blue",			new pb_HsvColor(205f, 41f, 76f, .01f) 	},
// 		{ "Alabama Crimson",				new pb_HsvColor(346f, 100f, 69f, .01f) 	},
// 		{ "Alice Blue",						new pb_HsvColor(208f, 6f, 100f, .01f) 	},
// 		{ "Alizarin Crimson",				new pb_HsvColor(355f, 83f, 89f, .01f) 	},
// 		{ "Alloy Orange",					new pb_HsvColor(27f, 92f, 77f,	.01f) 	},
// 		{ "Almond",							new pb_HsvColor(30f, 14f, 94f,	.01f) 	},
// 		{ "Amaranth",						new pb_HsvColor(348f, 81f, 90f,	.01f) 	},
// 		{ "Amaranth Deep Purple",			new pb_HsvColor(342f, 77f, 67f,	.01f) 	},
// 		{ "Amaranth Pink",					new pb_HsvColor(338f, 35f, 95f,	.01f) 	},
// 		{ "Amaranth Purple",				new pb_HsvColor(342f, 77f, 67f,	.01f) 	},
// 		{ "Amaranth Red",					new pb_HsvColor(356f, 84f, 83f,	.01f) 	},
// 		{ "Amazon",							new pb_HsvColor(147f, 52f, 48f,	.01f) 	},
// 		{ "Amber",							new pb_HsvColor(45f, 100f, 100f,.01f) 	},
// 		{ "Amber (SAE/ECE)",				new pb_HsvColor(30f, 100f, 100f,.01f) 	},
// 		{ "American Rose",					new pb_HsvColor(346f, 99f, 100f,.01f) 	},
// 		{ "Amethyst",						new pb_HsvColor(270f, 50f, 80f,	.01f) 	},
// 		{ "Android Green",					new pb_HsvColor(74f, 71f, 78f,	.01f) 	},
// 		{ "Anti-Flash White",				new pb_HsvColor(210f, 1f, 96f,	.01f) 	},
// 		{ "Antique Brass",					new pb_HsvColor(22f, 43f, 80f,	.01f) 	},
// 		{ "Antique Bronze",					new pb_HsvColor(53f, 71f, 40f,	.01f) 	},
// 		{ "Antique Fuchsia",				new pb_HsvColor(316f, 37f, 57f,	.01f) 	},
// 		{ "Antique Ruby",					new pb_HsvColor(350f, 80f, 52f,	.01f) 	},
// 		{ "Antique White",					new pb_HsvColor(34f, 14f, 98f,	.01f) 	},
// 		{ "Ao (English)",					new pb_HsvColor(120f, 100f, 50f,.01f) 	},
// 		{ "Apple Green",					new pb_HsvColor(74f, 100f, 71f,	.01f) 	},
// 		{ "Apricot",						new pb_HsvColor(24f, 29f, 98f,	.01f) 	},
// 		{ "Aqua",							new pb_HsvColor(180f, 100f, 100f,.01f) 	},
// 		{ "Aquamarine",						new pb_HsvColor(160f, 50f, 100f,.01f) 	},
// 		{ "Army Green",						new pb_HsvColor(69f, 61f, 33f,	.01f) 	},
// 		{ "Arsenic",						new pb_HsvColor(206f, 21f, 29f,	.01f) 	},
// 		{ "Artichoke",						new pb_HsvColor(76f, 20f, 59f,	.01f) 	},
// 		{ "Arylide Yellow",					new pb_HsvColor(51f, 54f, 91f,	.01f) 	},
// 		{ "Ash Grey",						new pb_HsvColor(135f, 6f, 75f,	.01f) 	},
// 		{ "Asparagus",						new pb_HsvColor(93f, 37f, 66f,	.01f) 	},
// 		{ "Atomic Tangerine",				new pb_HsvColor(20f, 60f, 100f,	.01f) 	},
// 		{ "Auburn",							new pb_HsvColor(0f, 75f, 65f,	.01f) 	},
// 		{ "Aureolin",						new pb_HsvColor(56f, 100f, 99f,	.01f) 	},
// 		{ "AuroMetalSaurus",				new pb_HsvColor(183f, 14f, 50f,	.01f) 	},
// 		{ "Avocado",						new pb_HsvColor(81f, 98f, 51f,	.01f) 	},
// 		{ "Azure",							new pb_HsvColor(210f, 100f, 100f,.01f) 	},
// 		{ "Azure (Web Color)",				new pb_HsvColor(180f, 6f, 100f,	.01f) 	},
// 		{ "Azure Mist",						new pb_HsvColor(180f, 6f, 100f,	.01f) 	},
// 		{ "Azureish White",					new pb_HsvColor(206f, 10f, 96f,	.01f) 	},
// 		{ "Baby Blue",						new pb_HsvColor(199f, 43f, 94f,	.01f) 	},
// 		{ "Baby Blue Eyes",					new pb_HsvColor(209f, 33f, 95f,	.01f) 	},
// 		{ "Baby Pink",						new pb_HsvColor(0f, 20f, 96f,	.01f) 	},
// 		{ "Baby Powder",					new pb_HsvColor(60f, 2f, 100f,	.01f) 	},
// 		{ "Baker-Miller Pink",				new pb_HsvColor(344f, 43f, 100f,.01f) 	},
// 		{ "Ball Blue",						new pb_HsvColor(192f, 84f, 80f,	.01f) 	},
// 		{ "Banana Mania",					new pb_HsvColor(43f, 28f, 98f,	.01f) 	},
// 		{ "Banana Yellow",					new pb_HsvColor(51f, 79f, 100f,	.01f) 	},
// 		{ "Bangladesh Green",				new pb_HsvColor(164f, 100f, 42f,.01f)	},
// 		{ "Barbie Pink",					new pb_HsvColor(327f, 85f, 88f,	.01f)	},
// 		{ "Barn Red",						new pb_HsvColor(4f, 98f, 49f,	.01f)	},
// 		{ "Battleship Grey",				new pb_HsvColor(60f, 2f, 52f,	.01f)	},
// 		{ "Bazaar",							new pb_HsvColor(353f, 22f, 60f,	.01f)	},
// 		{ "Beau Blue",						new pb_HsvColor(206f, 18f, 90f,	.01f)	},
// 		{ "Beaver",							new pb_HsvColor(22f, 30f, 62f,	.01f)	},
// 		{ "Beige",							new pb_HsvColor(60f, 10f, 96f,	.01f)	},
// 		{ "B'dazzled Blue",					new pb_HsvColor(215f, 69f, 58f,	.01f)	},
// 		{ "Big Dip O’ruby",					new pb_HsvColor(345f, 76f, 61f,	.01f)	},
// 		{ "Bisque",							new pb_HsvColor(33f, 23f, 100f,	.01f)	},
// 		{ "Bistre",							new pb_HsvColor(24f, 49f, 24f,	.01f)	},
// 		{ "Bistre Brown",					new pb_HsvColor(43f, 85f, 59f,	.01f)	},
// 		{ "Bitter Lemon",					new pb_HsvColor(66f, 94f, 88f,	.01f)	},
// 		{ "Bitter Lime",					new pb_HsvColor(75f, 100f, 100f,.01f)	},
// 		{ "Bittersweet",					new pb_HsvColor(6f, 63f, 100f,	.01f)	},
// 		{ "Bittersweet Shimmer",			new pb_HsvColor(359f, 59f, 75f,	.01f)	},
// 		{ "Black",							new pb_HsvColor(0f, 0f, 0f,	.01f)		},
// 		{ "Black Bean",						new pb_HsvColor(10f, 97f, 24f,	.01f)	},
// 		{ "Black Leather Jacket",			new pb_HsvColor(135f, 30f, 21f,	.01f)	},
// 		{ "Black Olive",					new pb_HsvColor(70f, 10f, 24f,	.01f)	},
// 		{ "Blanched Almond",				new pb_HsvColor(36f, 20f, 100f,	.01f)	},
// 		{ "Blast-Off Bronze",				new pb_HsvColor(12f, 39f, 65f,	.01f)	},
// 		{ "Bleu De France",					new pb_HsvColor(210f, 79f, 91f,	.01f)	},
// 		{ "Blizzard Blue",					new pb_HsvColor(188f, 28f, 93f,	.01f)	},
// 		{ "Blond",							new pb_HsvColor(50f, 24f, 98f,	.01f) 	},
// 		{ "Blue",							new pb_HsvColor(240f, 100f, 100f, .01f)	},
// 		{ "Blue (Crayola)",					new pb_HsvColor(217f, 88f, 100f, .01f)	},
// 		{ "Blue (Munsell)",					new pb_HsvColor(190f, 100f, 69f, .01f)	},
// 		{ "Blue (NCS)",						new pb_HsvColor(197f, 100f, 74f, .01f)	},
// 		{ "Blue (Pantone)",					new pb_HsvColor(231f, 100f, 66f, .01f)	},
// 		{ "Blue (Pigment)",					new pb_HsvColor(240f, 67f, 60f,	.01f) 	},
// 		{ "Blue (RYB)",						new pb_HsvColor(224f, 99f, 100f, .01f) 	},
// 		{ "Blue Bell",						new pb_HsvColor(240f, 22f, 82f,	.01f) 	},
// 		{ "Blue-Gray",						new pb_HsvColor(210f, 50f, 80f,	.01f) 	},
// 		{ "Blue-Green",						new pb_HsvColor(192f, 93f, 73f,	.01f) 	},
// 		{ "Blue Lagoon",					new pb_HsvColor(193f, 41f, 63f,	.01f)	},
// 		{ "Blue-Magenta Violet",			new pb_HsvColor(261f, 64f, 57f,	.01f)	},
// 		{ "Blue Sapphire",					new pb_HsvColor(197f, 86f, 50f,	.01f)	},
// 		{ "Blue-Violet",					new pb_HsvColor(271f, 81f, 89f,	.01f)	},
// 		{ "Blue Yonder",					new pb_HsvColor(217f, 52f, 65f,	.01f)	},
// 		{ "Blueberry",						new pb_HsvColor(220f, 68f, 97f,	.01f)	},
// 		{ "Bluebonnet",						new pb_HsvColor(240f, 88f, 94f,	.01f)	},
// 		{ "Blush",							new pb_HsvColor(342f, 58f, 87f,	.01f)	},
// 		{ "Bole",							new pb_HsvColor(9f, 51f, 47f,	.01f)	},
// 		{ "Bondi Blue",						new pb_HsvColor(191f, 100f, 71f, 01f)	},
// 		{ "Bone",							new pb_HsvColor(39f, 11f, 89f,	.01f)	},
// 		{ "Boston University Red",			new pb_HsvColor(0f, 100f, 80f,	.01f)	},
// 		{ "Bottle Green",					new pb_HsvColor(164f, 100f, 42f,.01f)	},
// 		{ "Boysenberry",					new pb_HsvColor(328f, 63f, 53f,	.01f)	},
// 		{ "Brandeis Blue",					new pb_HsvColor(214f, 100f, 100f,.01f)	},
// 		{ "Brass",							new pb_HsvColor(52f, 64f, 71f,	.01f) 	},
// 		{ "Brick Red",						new pb_HsvColor(352f, 68f, 80f,	.01f) 	},
// 		{ "Bright Cerulean",				new pb_HsvColor(194f, 86f, 84f,	.01f) 	},
// 		{ "Bright Green",					new pb_HsvColor(96f, 100f, 100f,.01f) 	},
// 		{ "Bright Lavender",				new pb_HsvColor(272f, 35f, 89f,	.01f) 	},
// 		{ "Bright Lilac",					new pb_HsvColor(285f, 39f, 94f,	.01f) 	},
// 		{ "Bright Maroon",					new pb_HsvColor(346f, 83f, 76f,	.01f) 	},
// 		{ "Bright Navy Blue",				new pb_HsvColor(210f, 88f, 82f,	.01f) 	},
// 		{ "Bright Pink",					new pb_HsvColor(330f, 100f, 100f,.01f) 	},
// 		{ "Bright Turquoise",				new pb_HsvColor(177f, 97f, 91f,	.01f) 	},
// 		{ "Bright Ube",						new pb_HsvColor(281f, 31f, 91f,	.01f) 	},
// 		{ "Brilliant Azure",				new pb_HsvColor(210f, 80f, 100f,.01f) 	},
// 		{ "Brilliant Lavender",				new pb_HsvColor(290f, 27f, 100f,.01f) 	},
// 		{ "Brilliant Rose",					new pb_HsvColor(332f, 67f, 100f,.01f) 	},
// 		{ "Brink Pink",						new pb_HsvColor(348f, 62f, 98f,	.01f) 	},
// 		{ "British Racing Green",			new pb_HsvColor(154f, 100f, 26f,.01f) 	},
// 		{ "Bronze",							new pb_HsvColor(30f, 76f, 80f,	.01f) 	},
// 		{ "Bronze Yellow",					new pb_HsvColor(58f, 100f, 45f,	.01f) 	},
// 		{ "Brown (Traditional)",			new pb_HsvColor(30f, 100f, 59f,	.01f) 	},
// 		{ "Brown (Web)",					new pb_HsvColor(0f, 75f, 65f,	.01f) 	},
// 		{ "Brown-Nose",						new pb_HsvColor(28f, 67f, 42f,	.01f) 	},
// 		{ "Brown Yellow",					new pb_HsvColor(30f, 50f, 80f,	.01f) 	},
// 		{ "Brunswick Green",				new pb_HsvColor(162f, 65f, 30f,	.01f) 	},
// 		{ "Bubble Gum",						new pb_HsvColor(349f, 24f, 100f,.01f) 	},
// 		{ "Bubbles",						new pb_HsvColor(183f, 9f, 100f,	.01f) 	},
// 		{ "Buff",							new pb_HsvColor(49f, 46f, 94f,	.01f) 	},
// 		{ "Bud Green",						new pb_HsvColor(102f, 47f, 71f,	.01f) 	},
// 		{ "Bulgarian Rose",					new pb_HsvColor(359f, 92f, 28f,	.01f) 	},
// 		{ "Burgundy",						new pb_HsvColor(345f, 100f, 50f,.01f) 	},
// 		{ "Burlywood",						new pb_HsvColor(34f, 39f, 87f,	.01f) 	},
// 		{ "Burnt Orange",					new pb_HsvColor(25f, 100f, 80f,	.01f) 	},
// 		{ "Burnt Sienna",					new pb_HsvColor(14f, 65f, 91f,	.01f) 	},
// 		{ "Burnt Umber",					new pb_HsvColor(9f, 74f, 54f,	.01f) 	},
// 		{ "Byzantine",						new pb_HsvColor(311f, 73f, 74f,	.01f) 	},
// 		{ "Byzantium",						new pb_HsvColor(311f, 63f, 44f,	.01f) 	},
// 		{ "Cadet",							new pb_HsvColor(199f, 27f, 45f,	.01f) 	},
// 		{ "Cadet Blue",						new pb_HsvColor(182f, 41f, 63f,	.01f) 	},
// 		{ "Cadet Grey",						new pb_HsvColor(205f, 18f, 69f,	.01f) 	},
// 		{ "Cadmium Green",					new pb_HsvColor(154f, 100f, 42f, .01f) 	},
// 		{ "Cadmium Orange",					new pb_HsvColor(28f, 81f, 93f,	.01f) 	},
// 		{ "Cadmium Red",					new pb_HsvColor(351f, 100f, 89f, .01f) 	},
// 		{ "Cadmium Yellow",					new pb_HsvColor(58f, 100f, 100f, .01f) 	},
// 		{ "Cafe Au Lait",					new pb_HsvColor(26f, 45f, 65f,	.01f) 	},
// 		{ "Cafe Noir",						new pb_HsvColor(30f, 56f, 29f,	.01f) 	},
// 		{ "Cal Poly Green",					new pb_HsvColor(137f, 61f, 30f,	.01f) 	},
// 		{ "Cambridge Blue",					new pb_HsvColor(140f, 16f, 76f,	.01f) 	},
// 		{ "Camel",							new pb_HsvColor(33f, 45f, 76f,	.01f) 	},
// 		{ "Cameo Pink",						new pb_HsvColor(340f, 22f, 94f,	.01f) 	},
// 		{ "Camouflage Green",				new pb_HsvColor(91f, 20f, 53f,	.01f) 	},
// 		{ "Canary Yellow",					new pb_HsvColor(56f, 100f, 100f, .01f) 	},
// 		{ "Candy Apple Red",				new pb_HsvColor(2f, 100f, 100f, .01f) 	},
// 		{ "Candy Pink",						new pb_HsvColor(355f, 50f, 89f, .01f) 	},
// 		{ "Capri",							new pb_HsvColor(195f, 100f, 100f, .01f) },
// 		{ "Caput Mortuum",					new pb_HsvColor(7f, 64f, 35f, .01f) 	},
// 		{ "Cardinal",						new pb_HsvColor(350f, 85f, 77f, .01f) 	},
// 		{ "Caribbean Green",				new pb_HsvColor(165f, 100f, 80f, .01f) 	},
// 		{ "Carmine",						new pb_HsvColor(350f, 100f, 59f, .01f) 	},
// 		{ "Carmine (M&P)",					new pb_HsvColor(342f, 100f, 84f, .01f) 	},
// 		{ "Carmine Pink",					new pb_HsvColor(4f, 72f, 92f, .01f) 	},
// 		{ "Carmine Red",					new pb_HsvColor(347f, 100f, 100f, .01f) },
// 		{ "Carnation Pink",					new pb_HsvColor(336f, 35f, 100f, .01f) 	},
// 		{ "Carnelian",						new pb_HsvColor(0f, 85f, 70f, .01f) 	},
// 		{ "Carolina Blue",					new pb_HsvColor(204f, 59f, 83f, .01f) 	},
// 		{ "Carrot Orange",					new pb_HsvColor(33f, 86f, 93f, .01f) 	},
// 		{ "Castleton Green",				new pb_HsvColor(164f, 100f, 34f, .01f) 	},
// 		{ "Catalina Blue",					new pb_HsvColor(221f, 95f, 47f, .01f) 	},
// 		{ "Catawba",						new pb_HsvColor(348f, 52f, 44f, .01f) 	},
// 		{ "Cedar Chest",					new pb_HsvColor(8f, 64f, 79f, .01f) 	},
// 		{ "Ceil",							new pb_HsvColor(225f, 29f, 81f, .01f) 	},
// 		{ "Celadon",						new pb_HsvColor(123f, 24f, 88f, .01f) 	},
// 		{ "Celadon Blue",					new pb_HsvColor(196f, 100f, 65f, .01f) 	},
// 		{ "Celadon Green",					new pb_HsvColor(174f, 64f, 52f, .01f) 	},
// 		{ "Celeste",						new pb_HsvColor(180f, 30f, 100f, .01f) 	},
// 		{ "Celestial Blue",					new pb_HsvColor(205f, 65f, 82f, .01f) 	},
// 		{ "Cerise",							new pb_HsvColor(343f, 78f, 87f, .01f) 	},
// 		{ "Cerise Pink",					new pb_HsvColor(336f, 75f, 93f, .01f) 	},
// 		{ "Cerulean",						new pb_HsvColor(196f, 100f, 65f, .01f) 	},
// 		{ "Cerulean Blue",					new pb_HsvColor(224f, 78f, 75f, .01f) 	},
// 		{ "Cerulean Frost",					new pb_HsvColor(208f, 44f, 76f, .01f) 	},
// 		{ "CG Blue",						new pb_HsvColor(196f, 100f, 65f, .01f) 	},
// 		{ "CG Red",							new pb_HsvColor(4f, 78f, 88f, .01f) 	},
// 		{ "Chamoisee",						new pb_HsvColor(26f, 44f, 63f, .01f) 	},
// 		{ "Champagne",						new pb_HsvColor(37f, 17f, 97f, .01f) 	},
// 		{ "Charcoal",						new pb_HsvColor(204f, 32f, 31f, .01f) 	},
// 		{ "Charleston Green",				new pb_HsvColor(180f, 19f, 17f, .01f) 	},
// 		{ "Charm Pink",						new pb_HsvColor(340f, 38f, 90f, .01f) 	},
// 		{ "Chartreuse (Traditional)",		new pb_HsvColor(68f, 100f, 100f, .01f) 	},
// 		{ "Chartreuse (Web)",				new pb_HsvColor(90f, 100f, 100f, .01f) 	},
// 		{ "Cherry",							new pb_HsvColor(343f, 78f, 87f, .01f) 	},
// 		{ "Cherry Blossom Pink",			new pb_HsvColor(348f, 28f, 100f, .01f) 	},
// 		{ "Chestnut",						new pb_HsvColor(10f, 64f, 58f, .01f) 	},
// 		{ "China Pink",						new pb_HsvColor(333f, 50f, 87f, .01f) 	},
// 		{ "China Rose",						new pb_HsvColor(340f, 52f, 66f, .01f) 	},
// 		{ "Chinese Red",					new pb_HsvColor(11f, 82f, 67f, .01f) 	},
// 		{ "Chinese Violet",					new pb_HsvColor(296f, 29f, 53f, .01f) 	},
// 		{ "Chocolate (Traditional)",		new pb_HsvColor(31f, 100f, 48f, .01f) 	},
// 		{ "Chocolate (Web)",				new pb_HsvColor(25f, 86f, 82f, .01f) 	},
// 		{ "Chrome Yellow",					new pb_HsvColor(39f, 100f, 100f, .01f) 	},
// 		{ "Cinereous",						new pb_HsvColor(12f, 19f, 60f, .01f) 	},
// 		{ "Cinnabar",						new pb_HsvColor(5f, 77f, 89f, .01f) 	},
// 		{ "Cinnamon",						new pb_HsvColor(25f, 86f, 82f, .01f) 	},
// 		{ "Citrine",						new pb_HsvColor(54f, 96f, 89f, .01f) 	},
// 		{ "Citron",							new pb_HsvColor(64f, 82f, 66f, .01f) 	},
// 		{ "Claret",							new pb_HsvColor(343f, 82f, 50f, .01f) 	},
// 		{ "Classic Rose",					new pb_HsvColor(326f, 19f, 98f, .01f) 	},
// 		{ "Cobalt Blue",					new pb_HsvColor(215f, 100f, 67f, .01f) 	},
// 		{ "Cocoa Brown",					new pb_HsvColor(25f, 86f, 82f, .01f) 	},
// 		{ "Coconut",						new pb_HsvColor(19f, 59f, 59f, .01f) 	},
// 		{ "Coffee",							new pb_HsvColor(25f, 50f, 44f, .01f) 	},
// 		{ "Columbia Blue",					new pb_HsvColor(200f, 13f, 89f, .01f) 	},
// 		{ "Congo Pink",						new pb_HsvColor(5f, 51f, 97f, .01f) 	},
// 		{ "Cool Black",						new pb_HsvColor(212f, 100f, 38f, .01f) 	},
// 		{ "Cool Grey",						new pb_HsvColor(229f, 19f, 67f, .01f) 	},
// 		{ "Copper",							new pb_HsvColor(29f, 72f, 72f, .01f) 	},
// 		{ "Copper (Crayola)",				new pb_HsvColor(18f, 53f, 85f, .01f) 	},
// 		{ "Copper Penny",					new pb_HsvColor(5f, 39f, 68f, .01f) 	},
// 		{ "Copper Red",						new pb_HsvColor(14f, 60f, 80f, .01f) 	},
// 		{ "Copper Rose",					new pb_HsvColor(0f, 33f, 60f, .01f) 	},
// 		{ "Coquelicot",						new pb_HsvColor(13f, 100f, 100f, .01f) 	},
// 		{ "Coral",							new pb_HsvColor(16f, 69f, 100f, .01f) 	},
// 		{ "Coral Pink",						new pb_HsvColor(5f, 51f, 97f, .01f) 	},
// 		{ "Coral Red",						new pb_HsvColor(0f, 75f, 100f, .01f) 	},
// 		{ "Cordovan",						new pb_HsvColor(355f, 54f, 54f, .01f) 	},
// 		{ "Corn",							new pb_HsvColor(54f, 63f, 98f, .01f) 	},
// 		{ "Cornell Red",					new pb_HsvColor(0f, 85f, 70f, .01f) 	},
// 		{ "Cornflower Blue",				new pb_HsvColor(219f, 58f, 93f, .01f) 	},
// 		{ "Cornsilk",						new pb_HsvColor(48f, 14f, 100f, .01f) 	},
// 		{ "Cosmic Latte",					new pb_HsvColor(43f, 9f, 100f, .01f) 	},
// 		{ "Coyote Brown",					new pb_HsvColor(62f, 52f, 51f, .01f) 	},
// 		{ "Cotton Candy",					new pb_HsvColor(334f, 26f, 100f, .01f) 	},
// 		{ "Cream",							new pb_HsvColor(57f, 18f, 100f, .01f) 	},
// 		{ "Crimson",						new pb_HsvColor(348f, 91f, 86f, .01f) 	},
// 		{ "Crimson Glory",					new pb_HsvColor(344f, 100f, 75f, .01f) 	},
// 		{ "Crimson Red",					new pb_HsvColor(0f, 100f, 60f, .01f) 	},
// 		{ "Cyan",							new pb_HsvColor(180f, 100f, 100f, .01f) },
// 		{ "Cyan Azure",						new pb_HsvColor(209f, 57f, 71f, .01f) 	},
// 		{ "Cyan-Blue Azure",				new pb_HsvColor(210f, 63f, 75f, .01f) 	},
// 		{ "Cyan Cobalt Blue",				new pb_HsvColor(215f, 74f, 61f, .01f) 	},
// 		{ "Cyan Cornflower Blue",			new pb_HsvColor(199f, 88f, 76f, .01f) 	},
// 		{ "Cyan (Process)",					new pb_HsvColor(193f, 100f, 92f, .01f) 	},
// 		{ "Cyber Grape",					new pb_HsvColor(263f, 47f, 49f, .01f) 	},
// 		{ "Cyber Yellow",					new pb_HsvColor(50f, 100f, 100f, .01f) 	},
// 		{ "Daffodil",						new pb_HsvColor(60f, 81f, 100f, .01f) 	},
// 		{ "Dandelion",						new pb_HsvColor(55f, 80f, 94f, .01f) 	},
// 		{ "Dark Blue",						new pb_HsvColor(240f, 100f, 55f, .01f) 	},
// 		{ "Dark Blue-Gray",					new pb_HsvColor(240f, 33f, 60f, .01f) 	},
// 		{ "Dark Brown",						new pb_HsvColor(30f, 67f, 40f, .01f) 	},
// 		{ "Dark Brown-Tangelo",				new pb_HsvColor(24f, 43f, 53f, .01f) 	},
// 		{ "Dark Byzantium",					new pb_HsvColor(315f, 39f, 36f, .01f) 	},
// 		{ "Dark Candy Apple Red",			new pb_HsvColor(0f, 100f, 64f, .01f) 	},
// 		{ "Dark Cerulean",					new pb_HsvColor(209f, 94f, 49f, .01f) 	},
// 		{ "Dark Chestnut",					new pb_HsvColor(10f, 37f, 60f, .01f) 	},
// 		{ "Dark Coral",						new pb_HsvColor(10f, 66f, 80f, .01f) 	},
// 		{ "Dark Cyan",						new pb_HsvColor(180f, 100f, 55f, .01f) 	},
// 		{ "Dark Electric Blue",				new pb_HsvColor(206f, 31f, 47f, .01f) 	},
// 		{ "Dark Goldenrod",					new pb_HsvColor(43f, 94f, 72f, .01f) 	},
// 		{ "Dark Gray (X11)",				new pb_HsvColor(0f, 0f, 66f, .01f) 		},
// 		{ "Dark Green",						new pb_HsvColor(158f, 98f, 20f, .01f) 	},
// 		{ "Dark Green (X11)",				new pb_HsvColor(120f, 100f, 39f, .01f) 	},
// 		{ "Dark Imperial-ier Blue",			new pb_HsvColor(203f, 100f, 42f, .01f) 	},
// 		{ "Dark Imperial Blue",				new pb_HsvColor(183f, 100f, 40f, .01f) 	},
// 		{ "Dark Jungle Green",				new pb_HsvColor(162f, 28f, 14f, .01f) 	},
// 		{ "Dark Khaki",						new pb_HsvColor(56f, 43f, 74f, .01f) 	},
// 		{ "Dark Lava",						new pb_HsvColor(27f, 31f, 28f, .01f) 	},
// 		{ "Dark Lavender",					new pb_HsvColor(270f, 47f, 59f, .01f) 	},
// 		{ "Dark Liver",						new pb_HsvColor(330f, 10f, 33f, .01f) 	},
// 		{ "Dark Liver (Horses)",			new pb_HsvColor(12f, 35f, 33f, .01f) 	},
// 		{ "Dark Magenta",					new pb_HsvColor(300f, 100f, 55f, .01f) 	},
// 		{ "Dark Medium Gray",				new pb_HsvColor(0f, 0f, 66f, .01f) 		},
// 		{ "Dark Midnight Blue",				new pb_HsvColor(210f, 100f, 40f, .01f) 	},
// 		{ "Dark Moss Green",				new pb_HsvColor(80f, 62f, 36f, .01f) 	},
// 		{ "Dark Olive Green",				new pb_HsvColor(82f, 56f, 42f, .01f) 	},
// 		{ "Dark Orange",					new pb_HsvColor(33f, 100f, 100f, .01f) 	},
// 		{ "Dark Orchid",					new pb_HsvColor(280f, 75f, 80f, .01f) 	},
// 		{ "Dark Pastel Blue",				new pb_HsvColor(212f, 41f, 80f, .01f) 	},
// 		{ "Dark Pastel Green",				new pb_HsvColor(138f, 98f, 75f, .01f) 	},
// 		{ "Dark Pastel Purple",				new pb_HsvColor(263f, 48f, 84f, .01f) 	},
// 		{ "Dark Pastel Red",				new pb_HsvColor(9f, 82f, 76f, .01f) 	},
// 		{ "Dark Pink",						new pb_HsvColor(342f, 64f, 91f, .01f) 	},
// 		{ "Dark Powder Blue",				new pb_HsvColor(220f, 100f, 60f, .01f) 	},
// 		{ "Dark Puce",						new pb_HsvColor(354f, 27f, 31f, .01f) 	},
// 		{ "Dark Purple",					new pb_HsvColor(291f, 51f, 20f, .01f) 	},
// 		{ "Dark Raspberry",					new pb_HsvColor(330f, 72f, 53f, .01f) 	},
// 		{ "Dark Red",						new pb_HsvColor(0f, 100f, 55f, .01f) 	},
// 		{ "Dark Salmon",					new pb_HsvColor(15f, 48f, 91f, .01f) 	},
// 		{ "Dark Scarlet",					new pb_HsvColor(344f, 97f, 34f, .01f) 	},
// 		{ "Dark Sea Green",					new pb_HsvColor(120f, 24f, 74f, .01f) 	},
// 		{ "Dark Sienna",					new pb_HsvColor(0f, 67f, 24f, .01f) 	},
// 		{ "Dark Sky Blue",					new pb_HsvColor(199f, 35f, 84f, .01f) 	},
// 		{ "Dark Slate Blue",				new pb_HsvColor(248f, 56f, 55f, .01f) 	},
// 		{ "Dark Slate Gray",				new pb_HsvColor(180f, 41f, 31f, .01f) 	},
// 		{ "Dark Spring Green",				new pb_HsvColor(150f, 80f, 45f, .01f) 	},
// 		{ "Dark Tan",						new pb_HsvColor(45f, 44f, 57f, .01f) 	},
// 		{ "Dark Tangerine",					new pb_HsvColor(38f, 93f, 100f, .01f) 	},
// 		{ "Dark Taupe",						new pb_HsvColor(27f, 31f, 28f, .01f) 	},
// 		{ "Dark Terra Cotta",				new pb_HsvColor(353f, 62f, 80f, .01f) 	},
// 		{ "Dark Turquoise",					new pb_HsvColor(181f, 100f, 82f, .01f) 	},
// 		{ "Dark Vanilla",					new pb_HsvColor(32f, 20f, 82f, .01f) 	},
// 		{ "Dark Violet",					new pb_HsvColor(282f, 100f, 83f, .01f) 	},
// 		{ "Dark Yellow",					new pb_HsvColor(52f, 92f, 61f, .01f) 	},
// 		{ "Dartmouth Green",				new pb_HsvColor(152f, 100f, 44f, .01f) 	},
// 		{ "Davy's Grey",					new pb_HsvColor(0f, 0f, 33f, .01f) 		},
// 		{ "Debian Red",						new pb_HsvColor(339f, 95f, 84f, .01f) 	},
// 		{ "Deep Aquamarine",				new pb_HsvColor(161f, 51f, 51f, .01f) 	},
// 		{ "Deep Carmine",					new pb_HsvColor(347f, 81f, 66f, .01f) 	},
// 		{ "Deep Carmine Pink",				new pb_HsvColor(357f, 80f, 94f, .01f) 	},
// 		{ "Deep Carrot Orange",				new pb_HsvColor(19f, 81f, 91f, .01f) 	},
// 		{ "Deep Cerise",					new pb_HsvColor(330f, 77f, 85f, .01f) 	},
// 		{ "Deep Champagne",					new pb_HsvColor(35f, 34f, 98f, .01f) 	},
// 		{ "Deep Chestnut",					new pb_HsvColor(3f, 61f, 73f, .01f) 	},
// 		{ "Deep Coffee",					new pb_HsvColor(1f, 42f, 44f, .01f) 	},
// 		{ "Deep Fuchsia",					new pb_HsvColor(300f, 56f, 76f, .01f) 	},
// 		{ "Deep Green",						new pb_HsvColor(122f, 95f, 40f, .01f) 	},
// 		{ "Deep Green-Cyan Turquois",		new pb_HsvColor(165f, 89f, 49f, .01f) 	},
// 		{ "Deep Jungle Green",				new pb_HsvColor(178f, 100f, 29f, .01f) 	},
// 		{ "Deep Koamaru",					new pb_HsvColor(240f, 50f, 40f, .01f) 	},
// 		{ "Deep Lemon",						new pb_HsvColor(47f, 89f, 96f, .01f) 	},
// 		{ "Deep Lilac",						new pb_HsvColor(280f, 55f, 73f, .01f) 	},
// 		{ "Deep Magenta",					new pb_HsvColor(300f, 100f, 80f, .01f) 	},
// 		{ "Deep Maroon",					new pb_HsvColor(0f, 100f, 51f, .01f) 	},
// 		{ "Deep Mauve",						new pb_HsvColor(300f, 46f, 83f, .01f) 	},
// 		{ "Deep Moss Green",				new pb_HsvColor(129f, 44f, 37f, .01f) 	},
// 		{ "Deep Peach",						new pb_HsvColor(26f, 36f, 100f, .01f) 	},
// 		{ "Deep Pink",						new pb_HsvColor(328f, 92f, 100f, .01f) 	},
// 		{ "Deep Puce",						new pb_HsvColor(351f, 46f, 66f, .01f) 	},
// 		{ "Deep Red",						new pb_HsvColor(0f, 99f, 52f, .01f) 	},
// 		{ "Deep Ruby",						new pb_HsvColor(336f, 52f, 52f, .01f) 	},
// 		{ "Deep Saffron",					new pb_HsvColor(30f, 80f, 100f, .01f) 	},
// 		{ "Deep Sky Blue",					new pb_HsvColor(195f, 100f, 100f, .01f) },
// 		{ "Deep Space Sparkle",				new pb_HsvColor(194f, 31f, 42f, .01f) 	},
// 		{ "Deep Spring Bud",				new pb_HsvColor(82f, 56f, 42f, .01f) 	},
// 		{ "Deep Taupe",						new pb_HsvColor(356f, 25f, 49f, .01f) 	},
// 		{ "Deep Tuscan Red",				new pb_HsvColor(342f, 35f, 40f, .01f) 	},
// 		{ "Deep Violet",					new pb_HsvColor(270f, 100f, 40f, .01f) 	},
// 		{ "Deer",							new pb_HsvColor(28f, 52f, 73f, .01f) 	},
// 		{ "Denim",							new pb_HsvColor(213f, 89f, 74f, .01f) 	},
// 		{ "Desaturated Cyan",				new pb_HsvColor(180f, 33f, 60f, .01f) 	},
// 		{ "Desert",							new pb_HsvColor(33f, 45f, 76f, .01f) 	},
// 		{ "Desert Sand",					new pb_HsvColor(25f, 26f, 93f, .01f) 	},
// 		{ "Desire",							new pb_HsvColor(352f, 74f, 92f, .01f) 	},
// 		{ "Diamond",						new pb_HsvColor(191f, 27f, 100f, .01f) 	},
// 		{ "Dim Gray",						new pb_HsvColor(0f, 0f, 41f, .01f) 		},
// 		{ "Dirt",							new pb_HsvColor(29f, 46f, 61f, .01f) 	},
// 		{ "Dodger Blue",					new pb_HsvColor(210f, 88f, 100f, .01f) 	},
// 		{ "Dogwood Rose",					new pb_HsvColor(335f, 89f, 84f, .01f) 	},
// 		{ "Dollar Bill",					new pb_HsvColor(98f, 46f, 73f, .01f) 	},
// 		{ "Donkey Brown",					new pb_HsvColor(35f, 61f, 40f, .01f) 	},
// 		{ "Drab",							new pb_HsvColor(43f, 85f, 59f, .01f) 	},
// 		{ "Duke Blue",						new pb_HsvColor(240f, 100f, 61f, .01f) 	},
// 		{ "Dust Storm",						new pb_HsvColor(6f, 12f, 90f, .01f) 	},
// 		{ "Dutch White",					new pb_HsvColor(42f, 22f, 94f, .01f) 	},
// 		{ "Earth Yellow",					new pb_HsvColor(34f, 58f, 88f, .01f) 	},
// 		{ "Ebony",							new pb_HsvColor(97f, 14f, 36f, .01f) 	},
// 		{ "Ecru",							new pb_HsvColor(45f, 34f, 76f, .01f) 	},
// 		{ "Eerie Black",					new pb_HsvColor(0f, 0f, 11f, .01f) 		},
// 		{ "Eggplant",						new pb_HsvColor(329f, 34f, 38f, .01f) 	},
// 		{ "Eggshell",						new pb_HsvColor(46f, 11f, 94f, .01f) 	},
// 		{ "Egyptian Blue",					new pb_HsvColor(226f, 90f, 65f, .01f) 	},
// 		{ "Electric Blue",					new pb_HsvColor(183f, 51f, 100f, .01f) 	},
// 		{ "Electric Crimson",				new pb_HsvColor(345f, 100f, 100f, .01f) },
// 		{ "Electric Cyan",					new pb_HsvColor(180f, 100f, 100f, .01f) },
// 		{ "Electric Green",					new pb_HsvColor(120f, 100f, 100f, .01f) },
// 		{ "Electric Indigo",				new pb_HsvColor(266f, 100f, 100f, .01f) },
// 		{ "Electric Lavender",				new pb_HsvColor(290f, 27f, 100f, .01f) 	},
// 		{ "Electric Lime",					new pb_HsvColor(72f, 100f, 100f, .01f) 	},
// 		{ "Electric Purple",				new pb_HsvColor(285f, 100f, 100f, .01f) },
// 		{ "Electric Ultramarine",			new pb_HsvColor(255f, 100f, 100f, .01f) },
// 		{ "Electric Violet",				new pb_HsvColor(274f, 100f, 100f, .01f) },
// 		{ "Electric Yellow",				new pb_HsvColor(60f, 80f, 100f, .01f) 	},
// 		{ "Emerald",						new pb_HsvColor(140f, 60f, 78f, .01f) 	},
// 		{ "Eminence",						new pb_HsvColor(284f, 63f, 51f, .01f) 	},
// 		{ "English Green",					new pb_HsvColor(162f, 65f, 30f, .01f) 	},
// 		{ "English Lavender",				new pb_HsvColor(338f, 27f, 71f, .01f) 	},
// 		{ "English Red",					new pb_HsvColor(356f, 56f, 67f, .01f) 	},
// 		{ "English Violet",					new pb_HsvColor(289f, 35f, 36f, .01f) 	},
// 		{ "Eton Blue",						new pb_HsvColor(134f, 25f, 78f, .01f) 	},
// 		{ "Eucalyptus",						new pb_HsvColor(161f, 68f, 84f, .01f) 	},
// 		{ "Fallow",							new pb_HsvColor(33f, 45f, 76f, .01f) 	},
// 		{ "Falu Red",						new pb_HsvColor(0f, 81f, 50f, .01f) 	},
// 		{ "Fandango",						new pb_HsvColor(320f, 72f, 71f, .01f) 	},
// 		{ "Fandango Pink",					new pb_HsvColor(338f, 63f, 87f, .01f) 	},
// 		{ "Fashion Fuchsia",				new pb_HsvColor(320f, 100f, 96f, .01f) 	},
// 		{ "Fawn",							new pb_HsvColor(30f, 51f, 90f, .01f) 	},
// 		{ "Feldgrau",						new pb_HsvColor(143f, 17f, 36f, .01f) 	},
// 		{ "Feldspar",						new pb_HsvColor(28f, 30f, 99f, .01f) 	},
// 		{ "Fern Green",						new pb_HsvColor(106f, 45f, 47f, .01f) 	},
// 		{ "Ferrari Red",					new pb_HsvColor(9f, 100f, 100f, .01f) 	},
// 		{ "Field Drab",						new pb_HsvColor(42f, 72f, 42f, .01f) 	},
// 		{ "Firebrick",						new pb_HsvColor(0f, 81f, 70f, .01f) 	},
// 		{ "Fire Engine Red",				new pb_HsvColor(357f, 84f, 81f, .01f) 	},
// 		{ "Flame",							new pb_HsvColor(17f, 85f, 89f, .01f) 	},
// 		{ "Flamingo Pink",					new pb_HsvColor(344f, 44f, 99f, .01f) 	},
// 		{ "Flattery",						new pb_HsvColor(28f, 67f, 42f, .01f) 	},
// 		{ "Flavescent",						new pb_HsvColor(52f, 43f, 97f, .01f) 	},
// 		{ "Flax",							new pb_HsvColor(50f, 45f, 93f, .01f) 	},
// 		{ "Flirt",							new pb_HsvColor(320f, 100f, 64f, .01f) 	},
// 		{ "Floral White",					new pb_HsvColor(40f, 6f, 100f, .01f) 	},
// 		{ "Fluorescent Orange",				new pb_HsvColor(45f, 100f, 100f, .01f) 	},
// 		{ "Fluorescent Pink",				new pb_HsvColor(328f, 92f, 100f, .01f) 	},
// 		{ "Fluorescent Yellow",				new pb_HsvColor(72f, 100f, 100f, .01f) 	},
// 		{ "Folly",							new pb_HsvColor(341f, 100f, 100f, .01f) },
// 		{ "Forest Green (Traditional)",		new pb_HsvColor(149f, 99f, 27f, .01f) 	},
// 		{ "Forest Green (Web)",				new pb_HsvColor(120f, 76f, 55f, .01f) 	},
// 		{ "French Beige",					new pb_HsvColor(26f, 45f, 65f, .01f) 	},
// 		{ "French Bistre",					new pb_HsvColor(34f, 42f, 52f, .01f) 	},
// 		{ "French Blue",					new pb_HsvColor(203f, 100f, 73f, .01f) 	},
// 		{ "French Fuchsia",					new pb_HsvColor(334f, 75f, 99f, .01f) 	},
// 		{ "French Lilac",					new pb_HsvColor(290f, 32f, 56f, .01f) 	},
// 		{ "French Lime",					new pb_HsvColor(89f, 78f, 99f, .01f) 	},
// 		{ "French Mauve",					new pb_HsvColor(300f, 46f, 83f, .01f) 	},
// 		{ "French Pink",					new pb_HsvColor(339f, 57f, 99f, .01f) 	},
// 		{ "French Plum",					new pb_HsvColor(325f, 84f, 51f, .01f) 	},
// 		{ "French Puce",					new pb_HsvColor(11f, 88f, 31f, .01f) 	},
// 		{ "French Raspberry",				new pb_HsvColor(349f, 78f, 78f, .01f) 	},
// 		{ "French Rose",					new pb_HsvColor(338f, 70f, 96f, .01f) 	},
// 		{ "French Sky Blue",				new pb_HsvColor(212f, 53f, 100f, .01f) 	},
// 		{ "French Violet",					new pb_HsvColor(279f, 97f, 81f, .01f) 	},
// 		{ "French Wine",					new pb_HsvColor(344f, 83f, 67f, .01f) 	},
// 		{ "Fresh Air",						new pb_HsvColor(196f, 35f, 100f, .01f) 	},
// 		{ "Fuchsia",						new pb_HsvColor(300f, 100f, 100f, .01f) },
// 		{ "Fuchsia (Crayola)",				new pb_HsvColor(300f, 56f, 76f, .01f) 	},
// 		{ "Fuchsia Pink",					new pb_HsvColor(300f, 53f, 100f, .01f) 	},
// 		{ "Fuchsia Purple",					new pb_HsvColor(333f, 72f, 80f, .01f) 	},
// 		{ "Fuchsia Rose",					new pb_HsvColor(337f, 66f, 78f, .01f) 	},
// 		{ "Fulvous",						new pb_HsvColor(35f, 100f, 89f, .01f) 	},
// 		{ "Fuzzy Wuzzy",					new pb_HsvColor(0f, 50f, 80f, .01f) 	}
// 	};
// }