#if !UNITY_4_7

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using ProBuilder2.Common;

namespace ProBuilder2.Test
{
	public class Color32ToUint
	{
		static public Color32 WHITE_COLOR = new Color32(255, 255, 255, 255);
		const uint WHITE_UINT = 0x00FFFFFF;

		static public Color32 BLACK_COLOR = new Color32(0, 0, 0, 255);
		const uint BLACK_UINT = 0x0;

		static public Color32 MIDDLE_COLOR = new Color32(125, 125, 125, 255);
		const uint MIDDLE_UINT = 0x007D7D7D;

		static public Color32 RED_COLOR = new Color32(255, 0, 0, 255);
		const uint RED_UINT = 0x00FF0000;

		static public Color32 PINK_COLOR = new Color32(255, 0, 255, 255);
		const uint PINK_UINT = 0x00FF00FF;

		static public Color32 OFFPINK_COLOR = new Color32(254, 0, 255, 255);
		const uint OFFPINK_UINT = 0x00FE00FF;

		static public Color32 GRAY_COLOR = new Color32(1, 1, 1, 255);
		const uint GRAY_UINT = 0x00010101;

		/**
		 *	Decode RGBA to UInt
		 */

		[Test]
		public static void TestDecode_WHITE()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(WHITE_COLOR), WHITE_UINT );
		}
		
		[Test]
		public static void TestDecode_BLACK()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(BLACK_COLOR), BLACK_UINT );
		}

		[Test]
		public static void TestDecode_MIDDLE()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(MIDDLE_COLOR), MIDDLE_UINT );
		}

		[Test]
		public static void TestDecode_RED()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(RED_COLOR), RED_UINT );
		}
		
		[Test]
		public static void TestDecode_PINK()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(PINK_COLOR), PINK_UINT );
		}

		[Test]
		public static void TestDecode_OFFPINK()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(OFFPINK_COLOR), OFFPINK_UINT );
		}

		[Test]
		public static void TestDecode_GRAY()
		{
			Assert.AreEqual( pb_SelectionPicker.DecodeRGBA(GRAY_COLOR), GRAY_UINT );
		}

		/**
		 *	Encode to RGBA
		 */

		[Test]
		public static void TestEncode_WHITE()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(WHITE_UINT), WHITE_COLOR );
		}
		
		[Test]
		public static void TestEncode_BLACK()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(BLACK_UINT), BLACK_COLOR );
		}

		[Test]
		public static void TestEncode_MIDDLE()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(MIDDLE_UINT), MIDDLE_COLOR );
		}

		[Test]
		public static void TestEncode_RED()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(RED_UINT), RED_COLOR );
		}
		
		[Test]
		public static void TestEncode_PINK()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(PINK_UINT), PINK_COLOR );
		}

		[Test]
		public static void TestEncode_OFFPINK()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(OFFPINK_UINT), OFFPINK_COLOR );
		}

		[Test]
		public static void TestEncode_GRAY()
		{
			Assert.AreEqual( pb_SelectionPicker.EncodeRGBA(GRAY_UINT), GRAY_COLOR );
		}

		/**
		 *	Test off-by-one
		 */

		[Test]
		public static void TestOffByOne_WHITE()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(WHITE_COLOR), WHITE_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(WHITE_COLOR), WHITE_UINT - 1 );
			}
		}
		
		[Test]
		public static void TestOffByOne_BLACK()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(BLACK_COLOR), BLACK_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(BLACK_COLOR), BLACK_UINT - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_MIDDLE()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(MIDDLE_COLOR), MIDDLE_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(MIDDLE_COLOR), MIDDLE_UINT - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_RED()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(RED_COLOR), RED_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(RED_COLOR), RED_UINT - 1 );
			}
		}
		
		[Test]
		public static void TestOffByOne_PINK()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(PINK_COLOR), PINK_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(PINK_COLOR), PINK_UINT - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_OFFPINK()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(OFFPINK_COLOR), OFFPINK_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(OFFPINK_COLOR), OFFPINK_UINT - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_GRAY()
		{
			unchecked {
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(GRAY_COLOR), GRAY_UINT + 1 );
				Assert.AreNotEqual( pb_SelectionPicker.DecodeRGBA(GRAY_COLOR), GRAY_UINT - 1 );
			}
		}

	}
}

#endif
