using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using ProBuilder.EditorCore;
using NUnit.Framework;
using System.Threading;
using Parabox.STL;
using ProBuilder.Core;
using ProBuilder.Test;
using UnityEngine.Windows;

namespace ProBuilder.EditorTests.Export
{
	public class ExportStl
	{
		[Test]
		public static void NumbersAreCultureInvariant()
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var current = Thread.CurrentThread.CurrentCulture;
			string path = pb_TestUtility.TemporarySavedAssetsDirectory + "/ExportStl.stl";

			try
			{
				// pb_Stl is an external library, so just make sure that it respects the thread culture setting
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Assert.IsTrue(pb_Stl_Exporter.Export(path, new GameObject[] { cube }, FileType.Ascii), "Export STL");
				string contents = System.IO.File.ReadAllText(path);
				Assert.IsFalse(contents.Any(x => x.Equals(',')));
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = current;

				if(System.IO.File.Exists(path))
					System.IO.File.Delete(path);

				UnityEngine.Object.DestroyImmediate(cube);
			}
		}
	}
}
