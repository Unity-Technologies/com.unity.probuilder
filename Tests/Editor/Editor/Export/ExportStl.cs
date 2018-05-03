using System.Globalization;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Threading;
using Parabox.STL;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.EditorTests.Export
{
	class ExportStl : TemporaryAssetTest
	{
		[Test]
		public static void NumbersAreCultureInvariant()
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var current = Thread.CurrentThread.CurrentCulture;
			string path = TestUtility.TemporarySavedAssetsDirectory + "/ExportStl.stl";

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
