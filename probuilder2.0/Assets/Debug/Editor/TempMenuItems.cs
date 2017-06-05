using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Reflection;
using KdTree;
using KdTree.Math;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;

/**
 *	KdTree DLL compiled from https://github.com/procore3d/KdTree
 */
public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		IEnumerable<pb_Model> models = pb_Selection.Top().Select(x => new pb_Model(x.msh, x.GetComponent<MeshRenderer>().sharedMaterial));

		foreach(pb_Object pb in pb_Selection.Top())
		{
			string obj, mat;
			pb_Obj.Export(models, out obj, out mat);
			pb_FileUtil.WriteFile("Assets/test.obj", obj);
		}

		AssetDatabase.Refresh();

		// foreach(pb_Object pb in pb_Selection.Top())
		// {
		// 	pb.ToMesh();
		// 	pb.RefreshUV(pb.SelectedFaces);
		// 	pb_Log.PushLogLevel(pb_LogLevel.None);
		// 	pb.Refresh();
		// 	pb_Log.PopLogLevel();
		// }

		// EditorWindow.GetWindow<TempMenuItems>(false, "KD Tree", true).Show();
	}

	static float rand { get { return UnityEngine.Random.Range(-m_PointRange, m_PointRange); } }

	KdTree<float, int> tree;
	float[][] points;
	static int m_PointRange = 256;
	Texture2D dot;
	int m_SampleCount = 24700;
	float neighborRadius = 32f;
	Vector2 center = Vector2.zero;

	void OnEnable()
	{
		dot = EditorGUIUtility.whiteTexture;
		Rebuild();
		center = position.size * .5f;
	}

	void Rebuild()
	{
		tree = new KdTree<float, int>(2, new FloatMath(), AddDuplicateBehavior.Update);
		points = new float[m_SampleCount][];
		
		for(int i = 0; i < m_SampleCount; i++)
		{
			points[i] = new float[2] { rand, rand };
			tree.Add(points[i], i);
		}
	}

	void OnGUI()
	{
		if(GUILayout.Button("Rebuild"))
			Rebuild();

		m_SampleCount = EditorGUILayout.IntField("Sample Count", m_SampleCount);
		m_PointRange = EditorGUILayout.IntField("Point Range", m_PointRange);
		neighborRadius = EditorGUILayout.Slider("Nearest Neighbor Radius", neighborRadius, .01f, 256f);

		if(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
		{
			center = Event.current.mousePosition;
			Repaint();
		}

		Rect r = new Rect(0,0,3,3);
		Vector2 extents = position.size * .5f;
		extents.y += GUILayoutUtility.GetLastRect().y - 16;

		Vector2 size = new Vector2(m_PointRange, m_PointRange);
		Vector2 topLeft = extents - size;
		Vector2 topRight = new Vector2(extents.x + size.x, extents.y - size.y);
		Vector2 bottomLeft = new Vector2(extents.x - size.x, extents.y + size.y);
		Vector2 bottomRight = new Vector2(extents.x + size.x, extents.y + size.y);

		Vector2 topCenter = new Vector2(extents.x, extents.y - size.y);
		Vector2 bottomCenter = new Vector2(extents.x, extents.y + size.y);
		Vector2 leftCenter = new Vector2(extents.x - size.x, extents.y);
		Vector2 rightCenter = new Vector2(extents.x + size.x, extents.y);

		Handles.color = new Color(.8f, .8f, .8f, 1f);
		Handles.DrawLine(topLeft, topRight);
		Handles.DrawLine(topLeft, bottomLeft);
		Handles.DrawLine(bottomLeft, bottomRight);
		Handles.DrawLine(bottomRight, topRight);
		Handles.color = new Color(.3f, .3f, .3f, 1f);
		Handles.DrawLine(topCenter, bottomCenter);
		Handles.DrawLine(leftCenter, rightCenter);
		Handles.color = Color.gray;

		for(int i = 0; i < points.Length; i++)
		{
			r.x = points[i][0] + extents.x;
			r.y = points[i][1] + extents.y;

			GUI.color = Color.gray;
			GUI.DrawTexture(r, dot, ScaleMode.ScaleToFit);
			GUI.color = Color.white;
		}

		KdTreeNode<float, int>[] neighbors = tree.RadialSearch(new float[2] { center.x - extents.x, center.y - extents.y }, neighborRadius, m_SampleCount);

		GUILayout.Label("neighbors: " + neighbors.Length);

		for(int i = 0; i < neighbors.Length; i++)
		{
			r.x = neighbors[i].Point[0] + extents.x;
			r.y = neighbors[i].Point[1] + extents.y;

			GUI.color = Color.green;
			GUI.DrawTexture(r, dot, ScaleMode.ScaleToFit);
			GUI.color = Color.white;
		}

		Handles.color = new Color(0f, .8f, 1f, 1f);
		const float targetSize = 16f;
		Handles.DrawLine(center - Vector2.right * targetSize, center + Vector2.right * targetSize);
		Handles.DrawLine(center - Vector2.up * targetSize, center + Vector2.up * targetSize);
		Handles.color = Color.gray;
		// just a wrapper around Handles.CircleCap that works across multiple Unity versions
		pb_Handles.CircleCap(-1, center, Quaternion.identity, neighborRadius);
		Handles.color = Color.white;
	}
}
