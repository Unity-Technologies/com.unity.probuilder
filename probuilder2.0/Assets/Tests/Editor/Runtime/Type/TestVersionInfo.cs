using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using System;

namespace ProBuilder.RuntimeTests.Type
{
	public class TestVersionInfo
	{
		static readonly pb_VersionInfo k_VersionA = new pb_VersionInfo(2, 0, 0, 3, VersionType.Final);
		static readonly pb_VersionInfo k_VersionB = new pb_VersionInfo(3, 2, 0, 3, VersionType.Final);
		static readonly pb_VersionInfo k_VersionC = new pb_VersionInfo(3, 2, 0, 3, VersionType.Beta);
		static readonly pb_VersionInfo k_VersionD = new pb_VersionInfo(3, 2, 2, 3, VersionType.Final);

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

		static readonly pb_VersionInfo k_Alpha = new pb_VersionInfo(1, 2, 3, 4, VersionType.Alpha);
		static readonly pb_VersionInfo k_Beta = new pb_VersionInfo(1, 2, 3, 4, VersionType.Beta);
		static readonly pb_VersionInfo k_Patch = new pb_VersionInfo(1, 2, 3, 4, VersionType.Patch);
		static readonly pb_VersionInfo k_Final = new pb_VersionInfo(1, 2, 3, 4, VersionType.Final);

		[Test]
		public static void ParseGoodInput()
		{
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
			Assert.Less(k_VersionA.CompareTo(k_VersionB), 0);
			Assert.Greater(k_VersionB.CompareTo(k_VersionA), 0);
			Assert.Less(k_VersionB.CompareTo(k_VersionD), 0);
			Assert.Less(k_VersionC.CompareTo(k_VersionB), 0);
		}
	}
}
