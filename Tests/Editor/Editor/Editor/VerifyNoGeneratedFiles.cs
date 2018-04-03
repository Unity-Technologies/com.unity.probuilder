using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.Test;
using UnityEngine.TestTools;
using ProBuilder.EditorCore;

namespace ProBuilder.EditorTests.Editor
{
#if !PB_DEBUG
	public class VerifyNoGeneratedFiles
	{
		[Test]
		public static void NoPreferencesAsset()
		{
			var prefs = pb_FileUtil.FindAssetOfType<pb_PreferenceDictionary>();
			Assert.IsTrue(prefs == null);
		}

		[Test]
		public static void NoColorPaletteAsset()
		{
			var palette = pb_FileUtil.FindAssetOfType<pb_ColorPalette>();
			Assert.IsTrue(palette == null);
		}

		[Test]
		public static void NoMaterialPalette()
		{
			var palette = pb_FileUtil.FindAssetOfType<pb_MaterialPalette>();
			Assert.IsTrue(palette == null);
		}
	}
#endif
}
