using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NUnit.Framework;

public class Test_UVEditor
{
	struct GraphSettings
	{
		public string name;
		public Vector2 GraphCenter;
		public Vector2 GraphOffset;
		public float GraphScale;
		public int GraphPixels;

		public GraphSettings(string name, Vector2 GraphCenter, Vector2 GraphOffset, float GraphScale, int GraphPixels)
		{
			this.name 			= name;
			this.GraphCenter	= GraphCenter;
			this.GraphOffset	= GraphOffset;
			this.GraphScale		= GraphScale;
			this.GraphPixels	= GraphPixels;
		}
	}

	[Test]
	public static void Test_UV_Conversions()
	{
		List<Vector4> uv_points = new List<Vector4>()
		{
			new Vector4(0f, 0f, 0f, 0f),
			new Vector4(.5f, 0f, 0f, 0f),
			new Vector4(0f, 0.4f, 0f, 0f),
			new Vector4(.5f, -.2f, 0f, .3f),
			new Vector4(1.0f, 201f, -.3f, .2f),
		};

		List<GraphSettings> graph_settings = new List<GraphSettings>()
		{
			new GraphSettings("Default", Vector2.zero, Vector2.zero, 1f, 256),
			new GraphSettings("Center", new Vector2(230, 20), Vector2.zero, 1f, 256),
			new GraphSettings("Offset", Vector2.zero, new Vector2(34, 610), 1f, 256),
			new GraphSettings("Scale", Vector2.zero, Vector2.zero, 3.4f, 256),
			new GraphSettings("Pixel Size", Vector2.zero, Vector2.zero, 1f, 512)
		};

		foreach(GraphSettings gs in graph_settings)
		{
			for(int i = 0; i < uv_points.Count; i++)
			{
				Vector4 a = pb_Handle_Utility.UVToGUIPoint(uv_points[i], gs.GraphCenter, gs.GraphOffset, gs.GraphScale, gs.GraphPixels);
				Vector4 b = pb_Handle_Utility.GUIToUVPoint(a, gs.GraphCenter, gs.GraphOffset, gs.GraphScale, gs.GraphPixels);
				Assert.AreEqual(uv_points[i].ToString("F4"), b.ToString("F4"), gs.name + " (" + i + ")");
			}
		}
	}
}
