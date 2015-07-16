using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parabox.Debug;

public class pb_Profiler_Interface : EditorWindow
{
	/// Every other row in the times display will be drawn with this color
	Color odd_column_color = new Color(.86f, .86f, .86f, 1f);

	/**
	 * Determines how the gui displays stopwatch values.
	 */
	enum Resolution
	{
		Tick,
		Nanosecond,
		Millisecond
	}

	/// The resolution (ticks, nanoseconds, milliseconds) to display information.
	Resolution resolution = Resolution.Nanosecond;

	List<pb_Profiler> profiles
	{
		get
		{
			return pb_Profiler.activeProfilers.FindAll(x => x.GetRootSample().children.Count > 0);
		}
	}

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

	int view = 0;
	Vector2 scroll = Vector2.zero;

	Dictionary<string, bool> row_visibility = new Dictionary<string, bool>();

	void OnGUI()
	{
		string[] display = new string[profiles.Count];
		int[] values = new int[display.Length];
		for(int i = 0; i < values.Length; i++)
		{
			display[i] = profiles[i].name;
			values[i] = i;
		}

		GUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
				view = EditorGUILayout.IntPopup("Profiler", view, display, values);
			if(EditorGUI.EndChangeCheck())
				row_visibility.Clear();

			resolution = (Resolution) EditorGUILayout.EnumPopup("Resolution", resolution);
			
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

	Color color = new Color(0,0,0,1);
	const float COLOR_BLOCK_SIZE = 16f;
	const int COLOR_BLOCK_PAD = 6;

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

			Rect r = GUILayoutUtility.GetLastRect();

			color.r = sample.Percentage() / 100f;
			color.b = 1f - color.r;

			r.x = (r.width + r.x) - COLOR_BLOCK_SIZE - COLOR_BLOCK_PAD;
			r.width = COLOR_BLOCK_SIZE;
			r.y += (r.height-COLOR_BLOCK_SIZE)/2f;
			r.height = COLOR_BLOCK_SIZE;

			DrawSolidColor(r, color);

			string avg, sum, min, max, lastSample;

			switch(resolution)
			{				
				case Resolution.Nanosecond:
					avg 		= string.Format("{0} n", pb_Profiler.TicksToNanosecond(sample.average));
					sum 		= string.Format("{0} n", pb_Profiler.TicksToNanosecond(sample.sum));
					min 		= string.Format("{0} n", pb_Profiler.TicksToNanosecond(sample.min));
					max 		= string.Format("{0} n", pb_Profiler.TicksToNanosecond(sample.max));
					lastSample	= string.Format("{0} n", pb_Profiler.TicksToNanosecond(sample.lastSample));
					break;

				case Resolution.Millisecond:
					avg 		= string.Format("{0} ms", pb_Profiler.TicksToMillisecond(sample.average));
					sum 		= string.Format("{0} ms", pb_Profiler.TicksToMillisecond(sample.sum));
					min 		= string.Format("{0} ms", pb_Profiler.TicksToMillisecond(sample.min));
					max 		= string.Format("{0} ms", pb_Profiler.TicksToMillisecond(sample.max));
					lastSample	= string.Format("{0} ms", pb_Profiler.TicksToMillisecond(sample.lastSample));
					break;

				default:
				case Resolution.Tick:
					avg 		= sample.average.ToString();
					sum 		= sample.sum.ToString();
					min 		= sample.min.ToString();
					max 		= sample.max.ToString();
					lastSample	= sample.lastSample.ToString();
					break;
			}

			GUILayout.Label(sample.sampleCount.ToString(), GUILayout.MinWidth(sample_width), GUILayout.MaxWidth(sample_width));
			GUILayout.Label(sample.Percentage().ToString("F2"), GUILayout.MinWidth(percent_width), GUILayout.MaxWidth(percent_width));
			GUILayout.Label(avg, GUILayout.MinWidth(avg_width), GUILayout.MaxWidth(avg_width));
			GUILayout.Label(sum, GUILayout.MinWidth(sum_width), GUILayout.MaxWidth(sum_width));

			GUILayout.Label(min, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUILayout.Label(max, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));
			GUILayout.Label(lastSample, GUILayout.MinWidth(range_width), GUILayout.MaxWidth(range_width));

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

	private static GUIStyle _splitStyle;
	private static GUIStyle SplitStyle
	{
		get
		{
			if(_splitStyle == null)
			{
				_splitStyle = new GUIStyle();
				_splitStyle.normal.background = EditorGUIUtility.whiteTexture;
				_splitStyle.margin = new RectOffset(6,6,0,0);
			}
			return _splitStyle;
		}
	}

	/**
	 * Draw a solid color block at rect.
	 */
	public static void DrawSolidColor(Rect rect, Color col)
	{
		Color old = UnityEngine.GUI.backgroundColor;
		UnityEngine.GUI.backgroundColor = col;

		UnityEngine.GUI.Box(rect, "", SplitStyle);

		UnityEngine.GUI.backgroundColor = old;
	}
}
