using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
	class SharedVertexView : MeshDebugView
	{
		int m_VertexCount = 0;
		int[] m_Vertexes;
		string[] m_Content;

		protected override void AnythingChanged()
		{
			if(mesh == null)
				return;

			var lookup = mesh.sharedVertexLookup;
			var common = mesh.sharedVertexes;
			var display = (viewState == MeshViewState.Selected ? mesh.selectedVertexes : mesh.sharedVertexes.Select(x => x[0])).ToArray();

			m_VertexCount = display.Length;
			m_Vertexes = new int[m_VertexCount];
			m_Content = new string[m_VertexCount];

			for (int i = 0; i < m_VertexCount; i++)
			{
				m_Vertexes[i] = display[i];
				m_Content[i] = string.Format("<b>{0}</b>: {1}", lookup[display[i]], GetCommonVertexString(common[lookup[display[i]]]));
			}
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

			for (int i = 0; i < m_VertexCount; i++)
			{
				DrawSceneLabel(trs.TransformPoint(positions[m_Vertexes[i]]), m_Content[i]);
			}
		}
	}
}