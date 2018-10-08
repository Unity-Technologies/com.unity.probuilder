using System.Linq;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Lightmapping = UnityEditor.Lightmapping;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.Tests.Slow
{
	class LightmapUVsAreValid
	{
		const string k_LightmapUnitTestsDir = "Assets/LightmappingUnitTests";
		const string k_LightmapUnitTestsScene = "Assets/LightmappingUnitTests/LightmappingUnitTestScene.unity";

		static void Setup()
		{
			if (!Directory.Exists(k_LightmapUnitTestsDir))
				Directory.CreateDirectory(k_LightmapUnitTestsDir);

			AssetDatabase.Refresh();

			var scene = SceneManagement.SceneManager.GetActiveScene();
			EditorSceneManager.SaveScene(scene, k_LightmapUnitTestsScene, false);
		}

		static void Cleanup()
		{
			Directory.Delete(k_LightmapUnitTestsDir, true);
			File.Delete(k_LightmapUnitTestsDir + ".meta");
			AssetDatabase.Refresh();
		}

		bool s_FinishedBaking;

		[UnityTest]
		public IEnumerator DefaultUnwrapParamsDoNotOverlap()
		{
			var lightmapMode = Lightmapping.giWorkflowMode;

			try
			{
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
				Lightmapping.started += () => { s_FinishedBaking = false; };
				Lightmapping.completed += () => { s_FinishedBaking = true; };

				Setup();

				float x = -10f;

				using (var shapes = new TestUtility.BuiltInPrimitives())
				{
					SceneModeUtility.SetStaticFlags(shapes.Select(it => it.gameObject).ToArray(), (int)StaticEditorFlags.LightmapStatic, true);

					foreach (ProBuilderMesh mesh in shapes)
					{
						mesh.transform.position = new Vector3(x, 0f, 0f);
						x += mesh.mesh.bounds.size.x + .5f;
						mesh.Optimize(true);
					}

					Lightmapping.BakeAsync();

					while (!s_FinishedBaking)
						yield return null;

					LogAssert.NoUnexpectedReceived();
				}
			}
			finally
			{
				Lightmapping.giWorkflowMode = lightmapMode;
				Cleanup();
			}
		}
	}
}
