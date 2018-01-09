using UnityEngine;
using NUnit.Framework;
using ProBuilder.Core;

namespace ProBuilder.EditorTests.Picking
{
	public class RectPicking
	{
		

		[Test]
		static void PickVerticesHiddenOff()
		{
			pb_Object pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			pb.transform.position = Vector3.zero;
		}
	}
}
