using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.EditorTests.Picking
{
	static class ColorEncoding
	{
		static readonly Color32 k_ColorWhite = new Color32(255, 255, 255, 255);
		const uint k_HexColorWhite = 0x00FFFFFF;

		static readonly Color32 k_ColorBlack = new Color32(0, 0, 0, 255);
		const uint k_HexColorBlack = 0x0;

		static readonly Color32 k_ColorMiddle = new Color32(125, 125, 125, 255);
		const uint k_HexColorMiddle = 0x007D7D7D;

		static readonly Color32 k_ColorRed = new Color32(255, 0, 0, 255);
		const uint k_HexColorRed = 0x00FF0000;

		static readonly Color32 k_ColorPink = new Color32(255, 0, 255, 255);
		const uint k_HexColorPink = 0x00FF00FF;

		static readonly Color32 k_ColorOffPink = new Color32(254, 0, 255, 255);
		const uint k_HexColorOffPink = 0x00FE00FF;

		static readonly Color32 k_ColorGray = new Color32(1, 1, 1, 255);
		const uint k_HexColorGray = 0x00010101;

		[Test]
		public static void TestDecode_WHITE()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorWhite), k_HexColorWhite );
		}

		[Test]
		public static void TestDecode_BLACK()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorBlack), k_HexColorBlack );
		}

		[Test]
		public static void TestDecode_MIDDLE()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorMiddle), k_HexColorMiddle );
		}

		[Test]
		public static void TestDecode_RED()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorRed), k_HexColorRed );
		}

		[Test]
		public static void TestDecode_PINK()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorPink), k_HexColorPink );
		}

		[Test]
		public static void TestDecode_OFFPINK()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorOffPink), k_HexColorOffPink );
		}

		[Test]
		public static void TestDecode_GRAY()
		{
			Assert.AreEqual( SelectionPicker.DecodeRGBA(k_ColorGray), k_HexColorGray );
		}

		[Test]
		public static void TestEncode_WHITE()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorWhite), k_ColorWhite );
		}

		[Test]
		public static void TestEncode_BLACK()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorBlack), k_ColorBlack );
		}

		[Test]
		public static void TestEncode_MIDDLE()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorMiddle), k_ColorMiddle );
		}

		[Test]
		public static void TestEncode_RED()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorRed), k_ColorRed );
		}

		[Test]
		public static void TestEncode_PINK()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorPink), k_ColorPink );
		}

		[Test]
		public static void TestEncode_OFFPINK()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorOffPink), k_ColorOffPink );
		}

		[Test]
		public static void TestEncode_GRAY()
		{
			Assert.AreEqual( SelectionPicker.EncodeRGBA(k_HexColorGray), k_ColorGray );
		}

		[Test]
		public static void TestOffByOne_WHITE()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorWhite), k_HexColorWhite + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorWhite), k_HexColorWhite - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_BLACK()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorBlack), k_HexColorBlack + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorBlack), k_HexColorBlack - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_MIDDLE()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorMiddle), k_HexColorMiddle + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorMiddle), k_HexColorMiddle - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_RED()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorRed), k_HexColorRed + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorRed), k_HexColorRed - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_PINK()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorPink), k_HexColorPink + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorPink), k_HexColorPink - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_OFFPINK()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorOffPink), k_HexColorOffPink + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorOffPink), k_HexColorOffPink - 1 );
			}
		}

		[Test]
		public static void TestOffByOne_GRAY()
		{
			unchecked {
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorGray), k_HexColorGray + 1 );
				Assert.AreNotEqual( SelectionPicker.DecodeRGBA(k_ColorGray), k_HexColorGray - 1 );
			}
		}

	}
}
