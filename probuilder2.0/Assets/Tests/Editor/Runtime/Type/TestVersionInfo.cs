#if !UNITY_4_7 && !UNITY_5_0 && !PROTOTYPE
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
