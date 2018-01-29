using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using System;
using ProBuilder.EditorCore;

namespace ProBuilder.RuntimeTests.Type
{
	public class TestVersionInfo
	{
		static pb_VersionInfo version_a = new pb_VersionInfo(2, 0, 0, 3, VersionType.Final);
		static pb_VersionInfo version_b = new pb_VersionInfo(3, 2, 0, 3, VersionType.Final);
		static pb_VersionInfo version_c = new pb_VersionInfo(3, 2, 0, 3, VersionType.Beta);
		static pb_VersionInfo version_d = new pb_VersionInfo(3, 2, 2, 3, VersionType.Final);

		[Test]
		public static void ParseMajorMinorPatchOnly()
		{
			var info = new pb_VersionInfo("1.0.0");
			var expected = new pb_VersionInfo(1, 0, 0, 0, VersionType.Development);
			Assert.AreEqual(info, expected);
		}

		[Test]
		public static void ParseMajorMinorPatchWithJunkAfter()
		{
			pb_VersionInfo info;

			var expected = new pb_VersionInfo(1, 0, 0, 12, VersionType.Development);
			Assert.IsTrue(pb_VersionInfo.TryGetVersionInfo("1.0.0-12text", out info));
			Assert.AreEqual(expected, info);

			expected = new pb_VersionInfo(1, 0, 0, 12, VersionType.Beta);
			Assert.IsTrue(pb_VersionInfo.TryGetVersionInfo("1.0.0-beta-12gibberish", out info));
			Assert.AreEqual(expected, info);
		}

		static void ParseBuildType(string input, pb_VersionInfo expected)
		{
			pb_VersionInfo info;
			Assert.IsTrue(pb_VersionInfo.TryGetVersionInfo(input, out info));
			Assert.AreEqual(expected, info, "input: " + input);
		}

		[Test]
		public static void ParseGoodInput()
		{
			pb_VersionInfo k_Alpha = new pb_VersionInfo(1, 2, 3, 4, VersionType.Alpha);
			pb_VersionInfo k_Beta = new pb_VersionInfo(1, 2, 3, 4, VersionType.Beta);
			pb_VersionInfo k_Patch = new pb_VersionInfo(1, 2, 3, 4, VersionType.Patch);
			pb_VersionInfo k_Final = new pb_VersionInfo(1, 2, 3, 4, VersionType.Final);

			// test all correct variations
			ParseBuildType("1.2.3-a.4", k_Alpha);
			ParseBuildType("1.2.3-alpha.4", k_Alpha);

			ParseBuildType("1.2.3-p.4", k_Patch);
			ParseBuildType("1.2.3-patch.4", k_Patch);

			ParseBuildType("1.2.3-b.4", k_Beta);
			ParseBuildType("1.2.3-beta.4", k_Beta);

			ParseBuildType("1.2.3-f.4", k_Final);
			ParseBuildType("1.2.3-final.4", k_Final);
		}

		[Test]
		public static void ParseWeirdlyFormattedInput()
		{
			// input that is good enough to be considered somewhat valid
			ParseBuildType("1.0.0-final", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0-f", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0-f.3", new pb_VersionInfo(1, 0, 0, 3, VersionType.Final));
			ParseBuildType("1.0.0-final.3", new pb_VersionInfo(1, 0, 0, 3, VersionType.Final));
			ParseBuildType("1.0.0-final-4", new pb_VersionInfo(1, 0, 0, 4, VersionType.Final));
			ParseBuildType("1.0.0-final+more_metadata", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0-final+more_metadata.3", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0-finished", new pb_VersionInfo(1, 0, 0, 0, VersionType.Development));
		}

		[Test]
		public static void ParseTechnicallyWrongButGoodEnoughInput()
		{
			ParseBuildType("1.0.0final", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0f", new pb_VersionInfo(1, 0, 0, 0, VersionType.Final));
			ParseBuildType("1.0.0f3", new pb_VersionInfo(1, 0, 0, 3, VersionType.Final));
			ParseBuildType("1.0.0f2033", new pb_VersionInfo(1, 0, 0, 2033, VersionType.Final));
		}

		[Test]
		public static void TestComparison()
		{
			Assert.Less(version_a.CompareTo(version_b), 0);
			Assert.Greater(version_b.CompareTo(version_a), 0);
			Assert.Less(version_b.CompareTo(version_d), 0);
			Assert.Less(version_c.CompareTo(version_b), 0);
		}
	}
}
#endif
