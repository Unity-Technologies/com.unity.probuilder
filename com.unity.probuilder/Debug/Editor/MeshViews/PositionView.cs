using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
	class PositionView : MeshDebugView
	{
		int m_ContentCount = 0;
		int m_VertexCount = 0;
		int[] m_Vertices;
		string[] m_Content;
		HashSet<int> m_Used = new HashSet<int>();

		protected override void AnythingChanged()
		{
			if(mesh == null)
				return;

			var lookup = mesh.sharedVertexLookup;
			var display = (viewState == MeshViewState.Selected ? mesh.selectedVertices : mesh.sharedVertices.Select(x => x[0])).ToArray();
			m_Used.Clear();

			m_ContentCount = display.Length;
			m_Vertices = new int[m_ContentCount];
			m_Content = new string[m_ContentCount];
			var positions = mesh.positionsInternal;
			m_VertexCount = mesh.vertexCount;

			for (int i = 0; i < m_ContentCount; i++)
			{
				int cIndex = lookup[display[i]];
				int lIndex = display[i];

				if (!m_Used.Add(cIndex))
					continue;
				m_Vertices[i] = lIndex;
				m_Content[i] = string.Format("<b>{0}</b> {1}", cIndex, positions[lIndex].ToString());
			}
		}

		public override void OnGUI()
		{
			GUI.skin.label.richText = true;

			GUILayout.Label("Vertex Count: " + m_VertexCount);

			foreach (var str in m_Content)
				GUILayout.Label(str);
		}

		public override void Draw(SceneView view)
		{
			var positions = mesh.positionsInternal;
			var trs = mesh.transform;

			for (int i = 0; i < m_ContentCount; i++)
			{
				var point = trs.TransformPoint(positions[m_Vertices[i]]);

				if(!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(view.camera, mesh, point))
					DrawSceneLabel(point, m_Content[i]);
			}
		}
	}
}
