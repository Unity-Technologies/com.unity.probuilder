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

		internal static void DrawGizmo(Vector3 position, Quaternion rotation, float size = -1f)
		{
			var p = position;
			size = HandleUtility.GetHandleSize(p) * size < 0f ? .5f : size;

			using (var lineDrawer = new LineDrawingScope(Color.green, -1f, CompareFunction.Always))
			{
				lineDrawer.DrawLine(p, p + rotation * Vector3.up * size);
				lineDrawer.color = Color.red;
				lineDrawer.DrawLine(p, p + rotation * Vector3.right * size);
				lineDrawer.color = Color.blue;
				lineDrawer.DrawLine(p, p + rotation * Vector3.forward * size);
			}
		}

		internal static void DrawGizmo(Vector3 position, Matrix4x4 matrix, float size = -1f)
		{
			var p = matrix.MultiplyPoint3x4(position);
			size = HandleUtility.GetHandleSize(p) * size < 0f ? .2f : size;

			using (var lineDrawer = new LineDrawingScope(Color.green, -1f, CompareFunction.Always))
			{
				lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.up) * size);
				lineDrawer.color = Color.red;
				lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.right) * size);
				lineDrawer.color = Color.blue;
				lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.forward) * size);
			}
		}

		internal class LineDrawingScope : IDisposable
		{
			bool m_Wire;
			Color m_Color;
			float m_Thickness;
			CompareFunction m_ZTest;
			bool m_IsDisposed;

			public Color color
			{
				get { return m_Color; }

				set
				{
					if(!m_Wire)
						End();

					m_Color = value;

					if(!m_Wire)
						Begin();
				}
			}

			public float thickness
			{
				get { return m_Thickness; }

				set
				{
					End();
					if (value < Mathf.Epsilon)
						m_Thickness = s_EdgeLineSize;
					else
						m_Thickness = value;
					Begin();
				}
			}

			public CompareFunction zTest
			{
				get { return m_ZTest; }

				set
				{
					End();
					m_ZTest = value;
					Begin();
				}
			}

			public LineDrawingScope(Color color, float thickness = -1f, CompareFunction zTest = CompareFunction.LessEqual)
			{
				Init();
				m_Color = color;
				m_Thickness = thickness < 0f ? s_EdgeLineSize : thickness;
				m_ZTest = zTest;
				Begin();
			}

			void Begin()
			{
				m_Wire = thickness < .01f || !BuiltinMaterials.geometryShadersSupported;

				if (!m_Wire)
				{
					s_LineMaterial.SetColor("_Color", color);
					s_LineMaterial.SetFloat("_Scale", thickness * EditorGUIUtility.pixelsPerPoint);
					s_LineMaterial.SetInt("_HandleZTest", (int) zTest);
				}

				if (m_Wire || !s_LineMaterial.SetPass(0))
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
							throw new Exception("Failed to find wire material, stopping draw lines.");
					}

					s_ApplyWireMaterialArgs[0] = zTest;
					s_ApplyWireMaterial.Invoke(null, s_ApplyWireMaterialArgs);
				}

				GL.PushMatrix();
				GL.Begin(GL.LINES);
			}

			void End()
			{
				GL.End();
				GL.PopMatrix();
			}

			public void DrawLine(Vector3 a, Vector3 b)
			{
				if(m_Wire)
					GL.Color(color);

				GL.Vertex(a);
				GL.Vertex(b);
			}

			public void Dispose()
			{
				if (m_IsDisposed)
					return;
				m_IsDisposed = true;

				End();
			}
		}

		internal class TriangleDrawingScope : IDisposable
		{
			Color m_Color;
			CompareFunction m_ZTest;
			bool m_IsDisposed;

			public Color color
			{
				get { return m_Color; }
				set
				{
					End();
					m_Color = value;
					Begin();
				}
			}

			public CompareFunction zTest
			{
				get { return m_ZTest; }

				set
				{
					End();
					m_ZTest = value;
					Begin();
				}
			}

			public TriangleDrawingScope(Color color, CompareFunction zTest = CompareFunction.LessEqual)
			{
				Init();
				m_Color = color;
				m_ZTest = zTest;
				Begin();
			}

			void Begin()
			{
				s_FaceMaterial.SetColor("_Color", color);
				s_FaceMaterial.SetInt("_HandleZTest", (int) zTest);

				if (!s_FaceMaterial.SetPass(0))
					throw new Exception("Failed initializing face material.");

				GL.PushMatrix();
				GL.Begin(GL.TRIANGLES);
			}

			void End()
			{
				GL.End();
				GL.PopMatrix();
			}

			public void Dispose()
			{
				if (m_IsDisposed)
					return;
				m_IsDisposed = true;
				End();
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
