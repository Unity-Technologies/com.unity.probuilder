using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace ProBuilder.Examples
{
	class Handles : MonoBehaviour
    {
        static Handles s_Instance;
		static bool s_Initialized;
#pragma warning disable 649
        public Shader m_FaceHighlight;
        public Shader m_LineBillboard;
        public Shader m_PointBillboard;
        public Shader m_VertexShader;
#pragma warning restore 649
        static Material s_EdgeMaterial;
		static Material s_VertMaterial;
		static Material s_FaceMaterial;
		static Face[] s_FaceArray = new Face[1];

        void Awake()
        {
            s_Instance = this;
        }

        static void Init()
        {
            if (s_Instance == null)
                Debug.LogError("No Handles object found in scene");

			if (s_Initialized)
				return;

			s_Initialized = true;

			var lineShader = BuiltinMaterials.geometryShadersSupported ? s_Instance.m_LineBillboard : s_Instance.m_FaceHighlight;
			var vertShader = BuiltinMaterials.geometryShadersSupported ? s_Instance.m_PointBillboard : s_Instance.m_VertexShader;

			s_EdgeMaterial = new Material(lineShader);
			s_VertMaterial = new Material(vertShader);
			s_FaceMaterial = new Material(s_Instance.m_FaceHighlight);
			s_FaceMaterial.SetFloat("_Dither", 1f);
        }

		public static Material edgeMaterial
		{
			get
			{
				Init();
				return s_EdgeMaterial;
			}
		}

		public static Material vertMaterial
		{
			get
			{
				Init();
				return s_VertMaterial;
			}
		}

		public static Material faceMaterial
		{
			get
			{
				Init();
				return s_FaceMaterial;
			}
		}

		public static void Draw(ProBuilderMesh mesh, Face face, Color color)
		{
			s_FaceArray[0] = face;
			Draw(mesh, s_FaceArray, color);
		}

		public static void Draw(ProBuilderMesh mesh, IEnumerable<Face> faces, Color color, CompareFunction compareFunction = CompareFunction.LessEqual)
		{
			if (mesh == null)
				return;

			faceMaterial.SetColor("_Color", color);
			faceMaterial.SetInt("_HandleZTest", (int) compareFunction);

			if (!faceMaterial.SetPass(0))
				return;

			GL.PushMatrix();
			GL.Begin(GL.TRIANGLES);
			GL.MultMatrix(mesh.transform.localToWorldMatrix);

			var positions = mesh.positions;

			foreach (var face in faces)
			{
				if (face == null)
					continue;

				var indices = face.indexes;

				for (int i = 0, c = indices.Count; i < c; i += 3)
				{
					GL.Vertex(positions[indices[i+0]]);
					GL.Vertex(positions[indices[i+1]]);
					GL.Vertex(positions[indices[i+2]]);
				}
			}

			GL.End();
			GL.PopMatrix();
		}

		public static void DrawLine(Vector3 a, Vector3 b, Color color, CompareFunction compareFunction = CompareFunction.LessEqual)
		{
			edgeMaterial.SetColor("_Color", color);
			edgeMaterial.SetInt("_HandleZTest", (int) compareFunction);
			if (BuiltinMaterials.geometryShadersSupported)
				edgeMaterial.SetFloat("_Scale", .2f);
			if (!edgeMaterial.SetPass(0))
				return;

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			GL.Vertex(a);
			GL.Vertex(b);

			GL.End();
			GL.PopMatrix();
		}
	}
}
