using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.TestTools;
using Lightmapping = UnityEditor.Lightmapping;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.Tests.Slow
{
	static class LightmapUVsAreValid
	{
		[Test]
		public static void DefaultUnwrapParamsDoNotOverlap()
		{
			var lightmapMode = Lightmapping.giWorkflowMode;
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
			float x = -10f;

			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				SceneModeUtility.SetStaticFlags(shapes.Select(it => it.gameObject).ToArray(), (int) StaticEditorFlags.LightmapStatic, true);

				foreach (ProBuilderMesh mesh in shapes)
				{
					mesh.transform.position = new Vector3(x, 0f, 0f);
					x += mesh.mesh.bounds.size.x + .5f;
					mesh.Optimize(true);
				}

				Lightmapping.Bake();
				LogAssert.NoUnexpectedReceived();
			}

			Lightmapping.giWorkflowMode = lightmapMode;
		}
	}
}
