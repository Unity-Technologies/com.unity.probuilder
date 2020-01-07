using NUnit.Framework;
using System.Collections;
using UnityEngine.ProBuilder;

class SemVerTests
{
	class TestData
	{
		public static IEnumerable Comparison
		{
			get
			{
				yield return new TestCaseData(new SemVer(), new SemVer()).Returns(0);
				yield return new TestCaseData(new SemVer(), new SemVer(1,0,0)).Returns(-1);
				yield return new TestCaseData(new SemVer(1,0,0), new SemVer()).Returns( 1);

				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(1,0,0)).Returns( 0);

				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(2,0,0)).Returns(-1);
				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(0,0,0)).Returns( 1);

				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(1,1,0)).Returns(-1);
				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(0,1,0)).Returns( 1);

				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(1,0,1)).Returns(-1);
				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(0,0,1)).Returns( 1);

				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(1,0,0,1)).Returns(1);
				yield return new TestCaseData(new SemVer(1,0,0), new SemVer(0,0,0,0)).Returns(1);

				yield return new TestCaseData(new SemVer("1.0.0-preview.1"), new SemVer("1.0.0-preview.1")).Returns(0);
				yield return new TestCaseData(new SemVer("1.0.0-preview.1"), new SemVer("1.0.0-preview.0")).Returns(1);
				yield return new TestCaseData(new SemVer("1.0.0-preview.0"), new SemVer("1.0.0-preview.1")).Returns(-1);

				yield return new TestCaseData(new SemVer("1.0.0-preview.1"), new SemVer()).Returns(1);
				yield return new TestCaseData(new SemVer(), new SemVer("1.0.0-preview.1")).Returns(-1);
			}
		}
	}

	[Test, TestCaseSource(typeof(TestData), "Comparison")]
	public int TestComparisonOperators(SemVer left, SemVer right)
	{
		return left.CompareTo(right);
	}
}
