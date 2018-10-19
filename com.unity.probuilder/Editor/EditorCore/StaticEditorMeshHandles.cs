using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using System.Reflection;
using UnityEngine.Rendering;

namespace UnityEditor.ProBuilder
{
	partial class EditorMeshHandles
	{
		static bool s_Initialized;

		static Material s_LineMaterial;
		static Material s_FaceMaterial;

		static Mesh s_FaceMesh;

		static void Init()
		{
			if (s_Initialized)
				return;
			s_Initialized = true;

			var shader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.lineShader : BuiltinMaterials.wireShader;
			s_LineMaterial = CreateMaterial(Shader.Find(shader), "ProBuilder::GeneralUseLineMaterial");

			s_FaceMesh = new Mesh();
			s_FaceMesh.hideFlags = HideFlags.HideAndDontSave;

			s_FaceMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::FaceMaterial");
			s_FaceMaterial.SetFloat("_Dither", (s_UseUnityColors || s_DitherFaceHandle) ? 1f : 0f);
		}

		internal static bool BeginDrawingLines(Color color, CompareFunction zTest = CompareFunction.LessEqual, float thickness = -1f)
		{
			if (Event.current.type != EventType.Repaint)
				return false;

			Init();

			if (thickness < Mathf.Epsilon)
				thickness = s_EdgeLineSize;

			s_LineMaterial.SetColor("_Color", color);
			s_LineMaterial.SetInt("_HandleZTest", (int) zTest);

			if(BuiltinMaterials.geometryShadersSupported)
				s_LineMaterial.SetFloat("_Scale", thickness * EditorGUIUtility.pixelsPerPoint);

			if (!BuiltinMaterials.geometryShadersSupported ||
				!s_LineMaterial.SetPass(0))
			{
				if (s_ApplyWireMaterial == null)
				{
					s_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
						"ApplyWireMaterial",
						BindingFlags.Static | BindingFlags.NonPublic,
						null,
						new System.Type[] { typeof(CompareFunction) },
						null);

					if (s_ApplyWireMaterial == null)
					{
						Log.Info("Failed to find wire material, stopping draw lines.");
						return false;
					}
				}

				s_ApplyWireMaterialArgs[0] = zTest;
				s_ApplyWireMaterial.Invoke(null, s_ApplyWireMaterialArgs);
			}

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			return true;
		}

		internal static void EndDrawingLines()
		{
			GL.End();
			GL.PopMatrix();
		}

		internal static void DrawLine(Vector3 a, Vector3 b)
		{
			GL.Vertex(a);
			GL.Vertex(b);
		}

		internal class FaceDrawingScope : IDisposable
		{
			Vector3[] m_Positions;

			public FaceDrawingScope(Color color, CompareFunction zTest = CompareFunction.LessEqual)
			{
				s_FaceMaterial.SetColor("_Color", color);
				s_FaceMaterial.SetInt("_HandleZTest", (int) zTest);

				if (!s_FaceMaterial.SetPass(0))
					throw new Exception("Failed initializing face material.");

				GL.PushMatrix();
				GL.Begin(GL.TRIANGLES);
			}

			public void Dispose()
			{
				GL.End();
				GL.PopMatrix();
			}

			public void Draw(Vector3 a, Vector3 b, Vector3 c)
			{
				GL.Vertex(a);
				GL.Vertex(b);
				GL.Vertex(c);
			}
		}
	}
}
