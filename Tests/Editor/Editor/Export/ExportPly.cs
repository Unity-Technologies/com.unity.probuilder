using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEditor.ProBuilder;
using NUnit.Framework;
using System.Threading;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.EditorTests.Export
{
	class ExportPly : TemporaryAssetTest
	{
		[Test]
		public static void NumbersAreCultureInvariant()
		{
			var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
			var current = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

				string ply;

				if (PlyExporter.Export(new ProBuilderMesh[] { cube }, out ply))
				{
					Assert.IsFalse(ply.Any(x => x.Equals(',')));
				}
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = current;
				UnityEngine.Object.DestroyImmediate(cube);
			}
		}
	}
}
