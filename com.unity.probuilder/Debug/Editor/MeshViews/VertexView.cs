using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
	class VertexView : MeshDebugView
	{
//		int m_VertexCount = 0;
		int m_DisplayCount = 0;
		List<int> m_Vertices = new List<int>();
		List<string> m_Content = new List<string>();
		HashSet<int> m_Used = new HashSet<int>();
		MeshArrays m_MeshArrays = MeshArrays.All;

		public MeshArrays meshArrays
		{
			get { return m_MeshArrays; }
			set { m_MeshArrays = value; }
		}

		protected override void AnythingChanged()
		{
			if(mesh == null)
				return;

			var lookup = mesh.sharedVertexLookup;
			var common = mesh.sharedVertices;
			var display = (viewState == MeshViewState.Selected ? mesh.selectedVertices : mesh.sharedVertices.Select(x => x[0])).ToArray();
			m_Used.Clear();

//			m_VertexCount = mesh.vertexCount;
			m_DisplayCount = display.Length;
			m_Vertices.Clear();
			m_Content.Clear();

			var position = mesh.positionsInternal;
			var color = mesh.colorsInternal;
			var normal = mesh.GetNormals();
			var tangent = mesh.GetTangents();
			var texture0 = mesh.GetUVs(0);
			var texture1 = mesh.GetUVs(1);
			var texture2 = mesh.GetUVs(2);
			var texture3 = mesh.GetUVs(3);

			for (int i = 0; i < m_DisplayCount; i++)
			{
				int cIndex = lookup[display[i]];
				int lIndex = display[i];

				if (!m_Used.Add(cIndex))
					continue;

				var sb = new StringBuilder();

				sb.AppendLine(string.Format("<b>{0}</b>: {1}", cIndex, GetCommonVertexString(common[lookup[display[i]]])));

				if ((m_MeshArrays & MeshArrays.Position) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Position))
						sb.AppendLine("position: " + position[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Texture0) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Texture0))
						sb.AppendLine("texture0: " + texture0[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Texture1) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Texture1))
						sb.AppendLine("texture1: " + texture1[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Texture2) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Texture2))
						sb.AppendLine("texture2: " + texture2[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Texture3) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Texture3))
						sb.AppendLine("texture3: " + texture3[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Color) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Color))
						sb.AppendLine("color: " + color[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Normal) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Normal))
						sb.AppendLine("normal: " + normal[lIndex].ToString());
				}

				if ((m_MeshArrays & MeshArrays.Tangent) > 0)
				{
					if(mesh.HasArrays(MeshArrays.Tangent))
						sb.AppendLine("tangent: " + tangent[lIndex].ToString());
				}

				m_Vertices.Add(display[i]);
				m_Content.Add(sb.ToString());
			}

			m_DisplayCount = m_Vertices.Count;
		}

		public override void OnGUI()
		{
			GUI.skin.label.richText = true;

			foreach (var str in m_Content)
				GUILayout.Label(str);
		}

		static string GetCommonVertexString(SharedVertex shared)
		{
			var str = "";
			var indexes = shared.arrayInternal;
			for (int i = 0, c = indexes.Length - 1; i < c; i++)
				str += indexes[i] + ", ";
			str += indexes[indexes.Length - 1];
			return str;
		}

		public override void Draw(SceneView view)
		{
			var positions = mesh.positionsInternal;
			var trs = mesh.transform;

			for (int i = 0; i < m_DisplayCount; i++)
			{
				var point = trs.TransformPoint(positions[m_Vertices[i]]);

				if(!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(view.camera, mesh, point))
					DrawSceneLabel(point, m_Content[i]);
			}
		}
	}
}
