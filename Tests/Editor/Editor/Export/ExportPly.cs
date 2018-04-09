using System.Globalization;
using System.Linq;
using UnityEngine;
using ProBuilder.EditorCore;
using NUnit.Framework;
using System.Threading;
using ProBuilder.Core;

namespace ProBuilder.EditorTests.Export
{
	public class ExportPly
	{
		[Test]
		public static void NumbersAreCultureInvariant()
		{
			var cube = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			var current = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

				string ply;

				if (pb_Ply.Export(new pb_Object[] { cube }, out ply))
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
