#if !UNITY_4_7 && !UNITY_5_0 && !PROTOTYPE
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System;

namespace ProBuilder2.Test
{
	public class TestVersionInfo
	{
		static pb_VersionInfo version_a = new pb_VersionInfo()
		{
			major = 2,
			minor = 0,
			patch = 0,
			type = VersionType.Final,
			build = 3
		};

		static pb_VersionInfo version_b = new pb_VersionInfo()
		{
			major = 3,
			minor = 2,
			patch = 0,
			type = VersionType.Final,
			build = 3
		};

		static pb_VersionInfo version_c = new pb_VersionInfo()
		{
			major = 3,
			minor = 2,
			patch = 0,
			type = VersionType.Final,
			build = 4
		};

		static pb_VersionInfo version_d = new pb_VersionInfo()
		{
			major = 3,
			minor = 2,
			patch = 2,
			type = VersionType.Final,
			build = 3
		};

		[Test]
		public static void TestComparison()
		{
			Assert.Less(version_a.CompareTo(version_b), 0);
			Assert.Greater(version_b.CompareTo(version_a), 0);
			Assert.Less(version_b.CompareTo(version_d), 0);
		}
	}
}
#endif
