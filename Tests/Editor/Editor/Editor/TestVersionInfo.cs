using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.EditorTests.Type
{
	public class TestVersionInfo
	{
		static readonly VersionInfo k_VersionA = new VersionInfo(2, 0, 0, 3);
		static readonly VersionInfo k_VersionB = new VersionInfo(3, 2, 0, 3);
		static readonly VersionInfo k_VersionC = new VersionInfo(3, 2, 0, 3, "preview");
		static readonly VersionInfo k_VersionD = new VersionInfo(3, 2, 2, 3);

		[Test]
		public static void ParseMajorMinorPatchOnly()
		{
			var info = new VersionInfo("1.0.0");
			var expected = new VersionInfo(1, 0, 0);
			Assert.AreEqual(info, expected);
		}

		[Test]
		public static void ParseMajorMinorPatchWithJunkAfter()
		{
			VersionInfo info;

			var expected = new VersionInfo(1, 0, 0, -1, "12text");
			Assert.IsTrue(VersionInfo.TryGetVersionInfo("1.0.0-12text", out info));
			Assert.AreEqual(expected, info);

			expected = new VersionInfo(1, 0, 0, 3, "preview");
			Assert.IsTrue(VersionInfo.TryGetVersionInfo("1.0.0-preview.3+extra-meta-data-here", out info));
			Assert.AreEqual(expected, info);
		}

		static void ParseBuildType(string input, VersionInfo expected)
		{
			VersionInfo info;
			Assert.IsTrue(VersionInfo.TryGetVersionInfo(input, out info));
			Assert.AreEqual(expected, info, "input: " + input);
		}

		static VersionInfo V(string version)
		{
			return new VersionInfo(version);
		}

		[Test]
		public static void CompareVersionInfo()
		{
			Assert.Less(V("3.0.0-b.0"), V("3.0.0"));
			Assert.Greater(V("3.0.1"), V("3.0.0"));
			Assert.AreEqual(V("3.0.0"), V("3.0.0"));
			Assert.Less(V("3.0.0"), V("4.0.0-b.1"));
		}

		static readonly VersionInfo k_Alpha = new VersionInfo(1, 2, 3, 4, "alpha");
		static readonly VersionInfo k_Beta = new VersionInfo(1, 2, 3, 4, "beta");
		static readonly VersionInfo k_Patch = new VersionInfo(1, 2, 3, 4, "patch");
		static readonly VersionInfo k_Final = new VersionInfo(1, 2, 3);

		[Test]
		public static void ParseGoodInput()
		{
			// test all correct variations
			ParseBuildType("1.2.3-alpha.4", k_Alpha);
			ParseBuildType("1.2.3-patch.4", k_Patch);
			ParseBuildType("1.2.3-beta.4", k_Beta);
			ParseBuildType("1.2.3", k_Final);
		}

		[Test]
		public static void ParseWeirdlyFormattedInput()
		{
			// input that is good enough to be considered valid
			ParseBuildType("1.0.0-final", new VersionInfo(1, 0, 0, -1, "final"));
			ParseBuildType("1.0.0-f", new VersionInfo(1, 0, 0, -1, "f"));
			ParseBuildType("1.0.0-f.3", new VersionInfo(1, 0, 0, 3, "f"));
			ParseBuildType("1.0.0-final.3", new VersionInfo(1, 0, 0, 3, "final"));
			ParseBuildType("1.0.0-final-4", new VersionInfo(1, 0, 0, -1, "final-4"));
			ParseBuildType("1.0.0-final+more_metadata", new VersionInfo(1, 0, 0, -1, "final"));
			ParseBuildType("1.0.0-final+more_metadata.3", new VersionInfo(1, 0, 0, -1, "final"));
			ParseBuildType("1.0.0-finished", new VersionInfo(1, 0, 0, -1, "finished"));
		}

		[Test]
		public static void ParseBadInput()
		{
			ParseBuildType("1.0.0final", new VersionInfo(1, 0, 0));
			ParseBuildType("1.0.0f", new VersionInfo(1, 0, 0));
			ParseBuildType("1.0.0f3", new VersionInfo(1, 0, 0));
			ParseBuildType("1.0.0f2033", new VersionInfo(1, 0, 0));
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
