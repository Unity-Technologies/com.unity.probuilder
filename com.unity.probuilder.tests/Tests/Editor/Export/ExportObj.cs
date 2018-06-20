using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder.Test;
using UnityEngine.TestTools;
using UnityEditor.ProBuilder;
using NUnit.Framework;
using System.Threading;

namespace UnityEngine.ProBuilder.EditorTests.Export
{
	class ExportObj : TemporaryAssetTest
	{
		[Test]
		public static void NumbersAreCultureInvariant()
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var model = new Model("Cube", cube.GetComponent<MeshFilter>().sharedMesh, cube.GetComponent<MeshRenderer>().sharedMaterial);
			var current = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

				string obj, mtl;
				List<string> textures;

				if (ObjExporter.Export("Cube Test", new Model[] { model }, out obj, out mtl, out textures))
				{
					Assert.IsFalse(obj.Any(x => x.Equals(',')));
					Assert.IsFalse(mtl.Any(x => x.Equals(',')));
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
