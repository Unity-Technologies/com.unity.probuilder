using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parabox.Debug;

public class pb_Profiler_Interface : EditorWindow
{
	Color odd_column_color = new Color(.86f, .86f, .86f, 1f);

	List<pb_Profiler> profiles
	{
		get
		{
			return pb_Profiler.activeProfilers.FindAll(x => x.GetRootSample().children.Count > 0);
		}
	}
	// bool update_gui = true;

	[MenuItem("Window/pb_Profiler")]
	public static void MenuInitProfilerWindow()
	{
		EditorWindow.GetWindow<pb_Profiler_Interface>(false, "pb_Profiler", false).Show();
	}

	void OnEnable()
	{
		EditorApplication.update += Update;
	}

	const int UDPATE_FREQ = 1;	// 1 per frame
	int updateFreqCounter = 0;
	void Update()
	{
		if(updateFreqCounter++ > UDPATE_FREQ * 100)
		{
			updateFreqCounter = 0;
			Repaint();
		}
	}

	// int n = 0;
	int view = 0;
	Vector2 scroll = Vector2.zero;

	Dictionary<string, bool> row_visibility = new Dictionary<string, bool>();

	void OnGUI()
	{
		// odd_column_color = EditorGUILayout.ColorField("col", odd_column_color);
		// GUILayout.Label(odd_column_color.r + ", " + odd_column_color.g+ ", " + odd_column_color.b + ", " + odd_column_color.a);
		// n = EditorGUILayout.IntField("n", n);

		string[] display = new string[profiles.Count];
		int[] values = new int[display.Length];
		for(int i = 0; i < values.Length; i++)
		{
			display[i] = "Profiler: " + i;
			values[i] = i;
		}

		GUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
				view = EditorGUILayout.IntPopup("Profiler", view, display, values);
			if(EditorGUI.EndChangeCheck())
				row_visibility.Clear();

			// update_gui = EditorGUILayout.Toggle("Update", update_gui, GUILayout.MaxWidth(165));
		GUILayout.EndHorizontal();

		// DRAW

		if(view < 0 || view >= profiles.Count)
			return;

		pb_Sample root = profiles[view].GetRootSample();
		if(root.children.Count < 1) return;

		Color bg = GUI.backgroundColor;
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.Space();
			GUILayout.Label("Sample", EditorStyles.toolbarButton, GUILayout.MinWidth(name_width-6), GUILayout.MaxWidth(name_width-6));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Calls", EditorStyles.toolbarButton, GUILayout.MinWidth(sample_width), GUILayout.MaxWidth(sample_width));
			GUI.backgroundColor = bg;
			GUILayout.Label("%", EditorStyles.toolbarButton, GUILayout.MinWidth(percent_width), GUILayout.MaxWidth(percent_width));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Avg", EditorStyles.toolbarButton, GUILayout.MinWidth(avg_width), GUILayout.MaxWidth(avg_width));
			GUI.backgroundColor = bg;
			GUILayout.Label("Sum", EditorStyles.toolbarButton, GUILayout.MinWidth(sum_width), GUILayout.MaxWidth(sum_width));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Min", EditorStyles.toolbarButton, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUI.backgroundColor = bg;
			GUILayout.Label("Max", EditorStyles.toolbarButton, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Current", EditorStyles.toolbarButton, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));

			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		scroll = EditorGUILayout.BeginScrollView(scroll);

		for(int i = 0; i < root.children.Count; i++)	
			DrawSampleTree(root.children[i]);

		EditorGUILayout.EndScrollView();

		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Print"))
				Debug.Log(profiles[view].ToString());

			if( GUILayout.Button("Clear", GUILayout.MaxWidth(120)) )
				profiles[view].Reset();

		GUILayout.EndHorizontal();
	}


	int name_width = 300;
	int sample_width = 64;
	int percent_width = 64;
	int sum_width = 80;
	int avg_width = 80;
	int range_width = 80;

	void DrawSampleTree(pb_Sample sample) { DrawSampleTree(sample, 0, ""); }
	void DrawSampleTree(pb_Sample sample, int indent, string key_prefix)
	{
		string key = key_prefix + sample.name;
		int childCount = sample.children.Count;

		if(!row_visibility.ContainsKey(key))
			row_visibility.Add(key, true);

		GUILayout.BeginHorizontal();

			GUILayout.BeginHorizontal(GUILayout.MinWidth(name_width), GUILayout.MaxWidth(name_width));

				GUILayout.Space(indent * (childCount > 0 ? 10 : 22));
				
				if(childCount > 0)
					row_visibility[key] = EditorGUILayout.Foldout(row_visibility[key], sample.name);
				else
					GUILayout.Label(sample.name);

			GUILayout.EndHorizontal();

			GUILayout.Label(sample.sampleCount.ToString(), GUILayout.MinWidth(sample_width), GUILayout.MaxWidth(sample_width));
			GUILayout.Label(sample.Percentage().ToString("F2"), GUILayout.MinWidth(percent_width), GUILayout.MaxWidth(percent_width));
			GUILayout.Label(sample.average.ToString() + " ms", GUILayout.MinWidth(avg_width), GUILayout.MaxWidth(avg_width));
			GUILayout.Label(sample.sum.ToString() + " ms", GUILayout.MinWidth(sum_width), GUILayout.MaxWidth(sum_width));

			GUILayout.Label(sample.min.ToString() + " ms", GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUILayout.Label(sample.max.ToString() + " ms", GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUILayout.Label(sample.lastSample.ToString() + " ms", GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));

		GUILayout.EndHorizontal();
	
		if(row_visibility[key])
		{
			indent++;
			foreach(pb_Sample child in sample.children)
			{
				DrawSampleTree(child, indent, key);
			}
		}
	}
}
