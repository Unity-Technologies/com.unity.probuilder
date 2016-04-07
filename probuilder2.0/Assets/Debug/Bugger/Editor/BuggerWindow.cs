/* Todo:
	- Step through StackFrames and allow user to double click to open file to line.
	- live update toggle
	- show notifications
	- draw to scene view
*/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Parabox.Debug;
using System.Reflection;
using Parabox.DebugUtil;
using System.Linq;

[InitializeOnLoad]
public class BuggerWindow : EditorWindow
{
	const string SHOW_KEYED_LOGS = "Bugger_Show_Keyed_Logs";
	const string SHOW_REGULAR_LOGS = "Bugger_Show_Regular_Logs";

	Color PRO_PRIMARY_TEXT_COLOR = new Color(1f, 1f, 1f, .8f);
	Color BASIC_PRIMARY_TEXT_COLOR = new Color(0f, 0f, 0f, .8f);

	Color PRO_SECONDARY_TEXT_COLOR = new Color(1f, 1f, 1f, .4f);
	Color BASIC_SECONDARY_TEXT_COLOR = new Color(0f, 0f, 0f, .6f);

	Color TOOLBAR_TOGGLED_COLOR = Color.gray;

	// functionality const
	const float DOUBLE_CLICK_TIME = .3f;

	// Editor Pref strings
	const string bugger_ShowUpdatedDelta = "bugger_ShowUpdatedDelta";

	// gui const
	const int SCROLL_PIXEL_PAD = 4;

	GUIStyle stackStyle = new GUIStyle(), wordWrappedLabel = null;

	// Bugger UI icons
	Texture2D logIcon;
	Texture2D errorIcon;
	Texture2D warningIcon;

	// icon buffer values
	const int icon_width = 32;

#region Enum/Classes

	private class MethodTrace
	{
		public string methodName;
		public string fullPath;
		public int lineNumber;
	}

	private class BugLog
	{
		public BugLog()
		{
			message = "";
			stack = new List<MethodTrace>();
		}

		public BugLog(string message)
		{
			this.message = message;
			this.stack = new List<MethodTrace>();
		}

		public string message;
		public LogType logType;
		public List<MethodTrace> stack;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(message);

			for(int i = 0; i < stack.Count; i++)
			{
				sb.AppendLine(stack[i].methodName);
				sb.AppendLine(stack[i].fullPath);
				sb.AppendLine("Line: " + stack[i].lineNumber);
				
				if(i < stack.Count-1)
					sb.AppendLine("\n");
			}
			return sb.ToString();
		}
	}

	private class BugLogComparer : IEqualityComparer<BugLog>
	{
		public bool Equals(BugLog lhs, BugLog rhs)
		{
			return 	lhs.message.Equals(rhs.message) &&
					lhs.logType == rhs.logType &&
					(
						(lhs.stack.Count > 0 && rhs.stack.Count > 0 && lhs.stack[0].lineNumber == rhs.stack[0].lineNumber) ||
						(lhs.stack.Count == 0 && rhs.stack.Count == 0)
					);
		}

		public int GetHashCode(BugLog log)
		{
			return log.ToString().GetHashCode();
		}
	}

	public enum BuggerEventType
	{
		MouseUp,
		MouseDown,
		DoubleClick,
		DragSplitHandleA,
		DragSplitHandleB,
		None
	}

	public struct BuggerEvent
	{
		public BuggerEventType type;
		public Vector2 mousePosition;
		public bool shift;

		public BuggerEvent(BuggerEventType _type, Vector2 _mouse, bool _shift)
		{
			type = _type;
			mousePosition = _mouse;
			shift = _shift;
		}

		public static bool IsDrag(BuggerEventType bet)
		{
			return bet == BuggerEventType.DragSplitHandleA || bet == BuggerEventType.DragSplitHandleB;
		}
	}

	private bool _showUpdatedDelta;
	private bool showUpdatedDelta {
		 get { return _showUpdatedDelta; }
		 set { EditorPrefs.SetBool(bugger_ShowUpdatedDelta, value); _showUpdatedDelta = value; }
	}
#endregion

#region Members

	public List<string> selectedKey = new List<string>();
	public List<int> selectedLog = new List<int>();
	private List<BugLog> selectedValue = new List<BugLog>();
	private List<BugLog> logEntries;
	bool collapse = false;

	private double lastMouseUp = 0;
	public double lastLogUpdate = 0;

	Color rowColorEven,
		rowColorOdd,
		rowSelectedBlue,
		rowTextColor,
		rowTextColorWhite,
		rowSelectedTextColorWhite,
		infoPaneBackgroundColor,
		splitColor;
	GUIStyle rowTextStyle, rowBackgroundStyle, splitStyle;
#endregion

#region Init Enable Menu

	[MenuItem("Window/Bugger Window %&b")]
	public static void InitBugWindow()
	{
		EditorWindow.GetWindow<BuggerWindow>(false, "Bugger", false);
	}

	public void OnEnable()
	{
#if UNITY_WEBPLAYER || UNITY_WP8
		UnityEngine.Debug.LogWarning("YOU'RE ON UNITY_WEBPLAYER YOU DICKHEAD");
#endif
		// Application.stackTraceLogType = StackTraceLogType.Full;
#if !UNITY_4_7
		Application.logMessageReceivedThreaded += Bugger.DebugLogHandler;
#endif

		if(EditorPrefs.HasKey(bugger_ShowUpdatedDelta))
			_showUpdatedDelta = EditorPrefs.GetBool(bugger_ShowUpdatedDelta);

		showKeyedLogs = EditorPrefs.GetBool(SHOW_KEYED_LOGS);
		showRegularLogs = EditorPrefs.GetBool(SHOW_REGULAR_LOGS);

		rowColorEven = EditorGUIUtility.isProSkin ? new Color(.83f, .83f, .83f, .06f) : new Color(.83f, .83f, .83f, 1f);
		rowColorOdd = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f, .02f) : new Color(.8f, .8f, .8f, 1f);
		rowSelectedBlue = EditorGUIUtility.isProSkin ? new Color( .23f, .375f, .56f, 1f) :  new Color( 57/256f, 125f/256f, 227f/256f, 1f);
		rowTextColorWhite = new Color(1f, 1f, 1f, .8f);
		rowSelectedTextColorWhite = new Color(1f, 1f, 1f, .9f);

		MethodInfo loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		logIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"console.infoicon"} );
		errorIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"console.erroricon"} );
 		warningIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"console.warnicon"} );

		infoPaneBackgroundColor = EditorGUIUtility.isProSkin ? new Color(.2f, .2f, .2f, 1f) : new Color(.8f, .8f, .8f, 1f);

		rowTextColor = EditorGUIUtility.isProSkin ? rowTextColorWhite : Color.black;

		// Initialize GUIStyles
		rowTextStyle = new GUIStyle();
		rowTextStyle.font = Resources.Load<Font>("monkey");
		rowTextStyle.normal.textColor = Color.black;
		rowTextStyle.contentOffset = new Vector2(4, 1);
		rowTextStyle.clipping = TextClipping.Clip;
		rowTextStyle.fixedHeight = 28;
		rowTextStyle.margin = new RectOffset(0, 0, 0, 0);
		
		rowBackgroundStyle = new GUIStyle();
		rowBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;

		stackStyle = new GUIStyle();
		stackStyle.font = Resources.Load<Font>("monkey");
		stackStyle.normal.textColor = EditorGUIUtility.isProSkin ? PRO_SECONDARY_TEXT_COLOR : BASIC_SECONDARY_TEXT_COLOR;
		stackStyle.alignment = TextAnchor.MiddleLeft;
		stackStyle.contentOffset = new Vector2(4, 2);
		stackStyle.wordWrap = true;
		// stackStyle.normal.background = EditorGUIUtility.whiteTexture;

		splitStyle = new GUIStyle();
		splitStyle.normal.background = EditorGUIUtility.whiteTexture;

		centeredLabel = new GUIStyle();
		centeredLabel.font = Resources.Load<Font>("monkey");
		centeredLabel.normal.textColor = PRO_SECONDARY_TEXT_COLOR;
		centeredLabel.alignment = TextAnchor.MiddleCenter;

		splitColor = new Color(0f, 0f, 0f, .8f);

		// Load log entries
		logEntries = LoadLogEntries();

		if(EditorPrefs.HasKey("BuggerSplitA") && EditorPrefs.HasKey("BuggerSplitB"))
		{
			splitA = EditorPrefs.GetInt("BuggerSplitA");
			splitB = EditorPrefs.GetInt("BuggerSplitB");
		}
		else
		{
			splitA = Screen.height/5;
			splitB = (Screen.height/3)*2;
		}
	}

	public void OnDisable()
	{
#if !UNITY_4_7
		Application.logMessageReceivedThreaded -= Bugger.DebugLogHandler;
#endif

		EditorPrefs.SetInt("BuggerSplitA", splitA);
		EditorPrefs.SetInt("BuggerSplitB", splitB);
	}
#endregion

#region Update / OnGUI

	private void Update()
	{
		Repaint();
	}

	BuggerEvent be = new BuggerEvent(BuggerEventType.None, Vector2.zero, false);
	Rect tabToggleRect = new Rect(0,0,0,0);

	Rect keyLogsRect = new Rect(0,0,0,0);
	Rect regLogsRect = new Rect(0,0,0,0);
	Rect infoPaneRect = new Rect(0,0,0,0);

	// split handles
	int splitSelectionHeight = 8;
	int splitA = 200;
	int splitB = 400;
	Rect splitRectA_graphics;
	Rect splitRectB_graphics;
	Rect splitRectA_selection;
	Rect splitRectB_selection;

	bool showKeyedLogs = true;
	bool showRegularLogs = true;

	const int HEADER_PREF_HEIGHT = 15;
	int curY;
	private void OnGUI()
	{
		Event e = Event.current;
		SetBuggerEvent(e);

		if(wordWrappedLabel == null)
		{
			wordWrappedLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
			wordWrappedLabel.font = Resources.Load<Font>("monkey");
			wordWrappedLabel.normal.textColor = rowTextColorWhite;
			wordWrappedLabel.contentOffset = new Vector2(4,4);
		}

		curY = 0;

		// Get rects for each display

		tabToggleRect = new Rect(0f, Screen.height-36, Screen.width-SCROLL_PIXEL_PAD, 17);

		keyLogsRect = new Rect(0f, curY, Screen.width-SCROLL_PIXEL_PAD, showRegularLogs ? splitA-3 : splitB-17);
		
		if(showKeyedLogs)
			curY += splitA-3;
		else
			splitA = curY;

		regLogsRect = new Rect(0f, splitA, Screen.width-SCROLL_PIXEL_PAD, splitB-splitA-SCROLL_PIXEL_PAD);
		infoPaneRect = new Rect(0f, splitB, Screen.width-SCROLL_PIXEL_PAD, Screen.height-splitB-SCROLL_PIXEL_PAD-tabToggleRect.height);

		if(showKeyedLogs && showRegularLogs)
			EditorGUIUtility.AddCursorRect( splitRectA_selection, MouseCursor.ResizeVertical);

		EditorGUIUtility.AddCursorRect( splitRectB_selection, MouseCursor.ResizeVertical);	

		// Dragging
		{
			// todo = implement rubberbandy drags
			if(showKeyedLogs && showRegularLogs)
			{
				if(be.type == BuggerEventType.DragSplitHandleA && e.type == EventType.Layout)
					splitA = (int)Mathf.Clamp(e.mousePosition.y, keyLogsRect.y+headerHeight+rowHeight, splitB-3-headerHeight-rowHeight);
			}

			// the info pane should always show
			if(be.type == BuggerEventType.DragSplitHandleB && e.type == EventType.Layout)
				splitB = (int)Mathf.Clamp(e.mousePosition.y, splitA+3+headerHeight+rowHeight, Screen.height-20-tabToggleRect.height);
		}

		splitRectA_selection = new Rect(0f, splitA-(splitSelectionHeight/2), Screen.width, splitSelectionHeight);
		splitRectB_selection = new Rect(0f, splitB-(splitSelectionHeight/2), Screen.width, splitSelectionHeight);

		splitRectA_graphics = new Rect(0f, splitA-1, Screen.width, 2f);
		splitRectB_graphics = new Rect(0f, splitB-1, Screen.width, 2f);

		// Draw stuff and thigns	
		// Header toggles
		DrawTabToggles(tabToggleRect);

		// Splitters
		GUI.backgroundColor = splitColor;
		if(showKeyedLogs && showRegularLogs)
			GUI.Box(splitRectA_graphics, "", splitStyle);
		GUI.Box(splitRectB_graphics, "", splitStyle);
		GUI.backgroundColor = Color.white;

		if(	be.type == BuggerEventType.MouseUp && !e.shift && (keyLogsRect.Contains(be.mousePosition) || regLogsRect.Contains(be.mousePosition)) )
		{		
			selectedKey.Clear();
			selectedLog.Clear();
		}

		if(Bugger.keyedLogs != null && showKeyedLogs)
			DrawKeyedLogs(keyLogsRect, be);

		DrawInfoPane(infoPaneRect, be);

		if(showRegularLogs)
			DrawLogs(regLogsRect, be);

		if(	be.type == BuggerEventType.MouseUp ||
			be.type == BuggerEventType.DragSplitHandleA ||
			be.type == BuggerEventType.DragSplitHandleB)
			Repaint();
	
		GUI.backgroundColor = Color.white;
	}

	private void OpenSelectedInText()
	{
#if !UNITY_WEBPLAYER
#if UNITY_STANDALONE_OSX
		string filePath = Directory.GetParent(Application.dataPath) + "/" + FileUtil.GetUniqueTempPathInProject() + ".txt";
#else
		string filePath = Directory.GetParent(Application.dataPath) + "\\" + FileUtil.GetUniqueTempPathInProject() + ".txt";
#endif
		File.WriteAllText(filePath, selectedValue.ToFormattedString("\n"));
		System.Diagnostics.Process.Start( filePath );
#endif
	}

	private void CopySelectedToBuffer()
	{
		EditorGUIUtility.systemCopyBuffer = selectedValue.ToFormattedString("\n");
	}

	private void SetBuggerEvent(Event e)
	{
		be.mousePosition = e.mousePosition;
		be.shift = e.shift;

		GUI.backgroundColor = Color.white;

		switch(e.type)
		{
			case EventType.MouseUp:
				if(BuggerEvent.IsDrag(be.type))
				{
					be.type = BuggerEventType.None;
					break;
				}

				if( EditorApplication.timeSinceStartup-lastMouseUp < DOUBLE_CLICK_TIME )
					be.type = BuggerEventType.DoubleClick;
				else
				{
					lastMouseUp = EditorApplication.timeSinceStartup;
					be.type = BuggerEventType.MouseUp;
				}
				break;

			case EventType.MouseDown:
				be.type = BuggerEventType.MouseDown;

				// check what was clicked 
				if( splitRectA_selection.Contains(e.mousePosition) )
				{
					be.type = BuggerEventType.DragSplitHandleA;
					e.Use();
				}

				if( splitRectB_selection.Contains(e.mousePosition) )
				{
					be.type = BuggerEventType.DragSplitHandleB;
					e.Use();
				}

				break;

			case EventType.ContextClick:
				GenericMenu menu = new GenericMenu();
				
				menu.AddItem (new GUIContent("Open Bugger Log", ""), false, OpenBuggerLog);
				menu.AddItem (new GUIContent("Clear Bugger Log", ""), false, ClearLog);
				
				menu.AddSeparator("");
				
				menu.AddItem (new GUIContent("Copy Selected Log to Buffer", ""), false, CopySelectedToBuffer);
				menu.AddItem (new GUIContent("Open Selected in Text Editor", ""), false, OpenSelectedInText);
				
				// menu.AddSeparator("");

				// menu.AddItem (new GUIContent("Make Log", ""), false, CreateLog);
				// menu.AddItem (new GUIContent("Make Warning", ""), false, CreateWarning);
				// menu.AddItem (new GUIContent("Make Error", ""), false, CreateError);

				menu.ShowAsContext ();
				e.Use();
				break;

			case EventType.ExecuteCommand:
				if(selectedValue != null)
				{
					EditorGUIUtility.systemCopyBuffer = selectedValue.ToString();
					e.Use();
				}
				break;

			case EventType.Ignore:	
				be.type = BuggerEventType.None;
				break;

			default:
				if(be.type != BuggerEventType.DragSplitHandleA && be.type != BuggerEventType.DragSplitHandleB)
					be.type = BuggerEventType.None;

				break;
		}

	}

	private void CreateLog()
	{
		Bugger.Log("I am a long winded log.  Nothing special\nabout me!!");
	}

	private void CreateWarning()
	{
		UnityEngine.Debug.LogWarning("Farts!!\nIt's time to baaail!");
	}

	private void CreateError()
	{
		RunErrorMethod();
	}

	private void RunErrorMethod()
	{
		AnotherMethod();
	}

	void AnotherMethod()
	{
		int[] arr = new int[2];
		for(int i = 0; i < 12; i++)
		{
			arr[i] = 0;
		}
	}
#endregion

#region Draw Methods

	Rect keyColumn, messageColumn, dateColumn, row;
	private int rowHeight = 30, headerHeight = 20;
	private Vector2 mPos;
	private Vector2 keyScroll = Vector2.zero;
	private Rect keyViewRect;

	private void DrawTabToggles(Rect r)
	{
		GUI.BeginGroup(r);

		GUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			EditorGUILayout.Space();

			GUI.backgroundColor = !showKeyedLogs ? TOOLBAR_TOGGLED_COLOR : Color.white;
			if(GUILayout.Button("Keyed Logs", EditorStyles.toolbarButton))
			{
				showKeyedLogs = !showKeyedLogs;
				EditorPrefs.SetBool(SHOW_KEYED_LOGS, showKeyedLogs);
				
				if(showKeyedLogs)
					splitA = (int)keyLogsRect.y + headerHeight + rowHeight + 32; 
			}
			GUI.backgroundColor = Color.white;

			GUI.backgroundColor = !showRegularLogs ? TOOLBAR_TOGGLED_COLOR : Color.white;
			if(GUILayout.Button( "Regular Logs", EditorStyles.toolbarButton))
			{
				showRegularLogs = !showRegularLogs;
				EditorPrefs.SetBool(SHOW_REGULAR_LOGS, showRegularLogs);
				
				// if(showRegularLogs)
				// 	splitB = (int)Mathf.Clamp(e.mousePosition.y, splitA+3+headerHeight+rowHeight, Screen.height-20);

			}
			GUI.backgroundColor = Color.white;
	
			GUILayout.FlexibleSpace();


		GUILayout.EndHorizontal();
		GUI.EndGroup();
	}

	private void DrawKeyedLogs(Rect rect, BuggerEvent e)
	{
		mPos = e.mousePosition;
		mPos.y -= rect.y - keyScroll.y + headerHeight;

		int i = 0;

		GUI.BeginGroup(rect);

		keyColumn 		= new Rect(0f, 0f, Screen.width/3f, headerHeight);
		messageColumn 	= new Rect(Screen.width/3f, 0f, Screen.width/3f, headerHeight);
		dateColumn 		= new Rect((Screen.width/3f) * 2f, 0f, Screen.width/3f, headerHeight);

		GUI.Label(keyColumn, "Key", EditorStyles.toolbarButton);	
		GUI.Label(messageColumn, "Message", EditorStyles.toolbarButton);
		
		GUI.backgroundColor = showUpdatedDelta ? Color.gray : Color.white;
		if(GUI.Button(dateColumn, (showUpdatedDelta) ? "Update Delta" : "Log Time", EditorStyles.toolbarButton))
			showUpdatedDelta = !showUpdatedDelta;
		GUI.backgroundColor = Color.white;

		keyColumn.height 		= rowHeight;
		messageColumn.height 	= rowHeight;
		dateColumn.height 		= rowHeight;

		float rowOffset = 0f;

		keyViewRect = new Rect(0f, 0, rect.width-16, Bugger.keyedLogs.Count * rowHeight);
		keyScroll = GUI.BeginScrollView(new Rect(0, headerHeight-2, rect.width, rect.height-16), keyScroll, keyViewRect);

		foreach(KeyValuePair<string, Bugger.LogEntry> kvp in Bugger.keyedLogs)
		{
			keyColumn.y 	= rowOffset;	
			messageColumn.y = rowOffset;
			dateColumn.y 	= rowOffset;
			row 	 		= new Rect(0f, rowOffset, Screen.width, rowHeight);

			rowOffset += rowHeight;

			GUI.backgroundColor = i++ % 2 == 0 ? rowColorEven : rowColorOdd;			
			
			if(selectedKey.Contains(kvp.Key))
			{
				GUI.backgroundColor = rowSelectedBlue;
			    rowTextStyle.normal.textColor = rowSelectedTextColorWhite;
			}
			else
				rowTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? PRO_PRIMARY_TEXT_COLOR : BASIC_PRIMARY_TEXT_COLOR;

			GUI.Label(row, "", rowBackgroundStyle);

			GUI.Label(keyColumn, kvp.Key, rowTextStyle);

			GUI.Label(messageColumn, kvp.Value.message, rowTextStyle);

			if(showUpdatedDelta)
			{
				TimeSpan timeSpan = (DateTime.Now-kvp.Value.date);
				GUI.Label(dateColumn, timeSpan.Seconds + ":" + timeSpan.Milliseconds.ToString(), rowTextStyle);
			}
			else
				GUI.Label(dateColumn, kvp.Value.date.ToString(), rowTextStyle);

			if(kvp.Value.message.Split('\n').Length < 2)
			{
				rowTextStyle.clipping = TextClipping.Overflow;
				GUI.Label(keyColumn, "\n" + kvp.Value.formattedStackTrace, rowTextStyle);
				rowTextStyle.clipping = TextClipping.Clip;
			}

			if(be.type == BuggerEventType.MouseUp && rect.Contains(e.mousePosition) && row.Contains(mPos))
			{
				selectedKey.Add(kvp.Key);
				if(!be.shift)
					selectedValue.Clear();
				selectedValue.Add(new BugLog( kvp.Value.ToString()));
			}
		}

		GUI.backgroundColor = Color.white;

		List<string> removeList = new List<string>();
		double curTimeSinceStartup = EditorApplication.timeSinceStartup;

		foreach(KeyValuePair<string, Bugger.TempLogEntry> kvp in Bugger.tempLog)
		{
			keyColumn.y 	= rowOffset;	
			messageColumn.y = rowOffset;
			dateColumn.y 	= rowOffset;
			row 	 		= new Rect(0f, rowOffset, Screen.width, rowHeight);
			
			GUI.backgroundColor = i++ % 2 == 0 ? rowColorEven : rowColorOdd;			
			
			if(selectedKey.Contains(kvp.Key))
			{
				GUI.backgroundColor = rowSelectedBlue;
			    rowTextStyle.normal.textColor = rowSelectedTextColorWhite;
			}
			else
				rowTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? PRO_PRIMARY_TEXT_COLOR : BASIC_PRIMARY_TEXT_COLOR;

			GUI.Box(row, "", rowBackgroundStyle);

			GUI.Label(keyColumn, kvp.Key, rowTextStyle);

			GUI.Label(messageColumn, kvp.Value.message, rowTextStyle);

			double timeTilDeletion = curTimeSinceStartup - kvp.Value.logTime;

			if(showUpdatedDelta)
				GUI.Label(dateColumn, (kvp.Value.life - timeTilDeletion).ToString(), rowTextStyle);
			else
				GUI.Label(dateColumn, kvp.Value.life.ToString(), rowTextStyle);

			if(timeTilDeletion > kvp.Value.life)
				removeList.Add(kvp.Key);

			if(be.type == BuggerEventType.MouseUp && rect.Contains(be.mousePosition) && row.Contains(mPos))
				selectedKey.Add(kvp.Key);
		}

		foreach(string cheese in removeList)
			Bugger.tempLog.Remove(cheese);

		GUI.backgroundColor = Color.white;

		GUI.EndScrollView();

		GUI.EndGroup();

		if(e.type == BuggerEventType.DoubleClick && rect.Contains(be.mousePosition) && selectedKey.Count > 0 && Bugger.keyedLogs.ContainsKey(selectedKey[selectedKey.Count-1]))
		{
			GoToLine( Bugger.keyedLogs[selectedKey[selectedKey.Count-1]].stackTrace );
		}
	}

	void ClearLog()
	{
		UnityEngine.Debug.ClearDeveloperConsole ();
		Bugger.ClearLog();
	}

	Rect logRect;
	Vector2 logScroll = Vector2.zero;
	private void DrawLogs(Rect rect, BuggerEvent e)
	{
		mPos = e.mousePosition;
		mPos.y -= rect.y - logScroll.y + headerHeight;

		/* <blech> */
		rect.y -= (headerHeight-3);
		rect.width += 1;
		GUI.BeginGroup(rect);
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			if(GUILayout.Button("Clear", EditorStyles.toolbarButton))
				ClearLog();

			GUI.backgroundColor = !collapse ? Color.white : TOOLBAR_TOGGLED_COLOR;
			if(GUILayout.Button("Collapse", EditorStyles.toolbarButton))
				collapse = !collapse;

			GUILayout.Space(2);

			EditorGUI.BeginChangeCheck();

			GUI.backgroundColor = errorLogs ? Color.white : TOOLBAR_TOGGLED_COLOR;
			if(GUILayout.Button("E", EditorStyles.toolbarButton))
			{
				errorLogs = !errorLogs;
				logEntries = LoadLogEntries();
			}

			GUI.backgroundColor = warningLogs ? Color.white : TOOLBAR_TOGGLED_COLOR;
			if(GUILayout.Button("W", EditorStyles.toolbarButton))
			{
				warningLogs = !warningLogs;
				logEntries = LoadLogEntries();
			}

			GUI.backgroundColor = infoLogs ? Color.white : TOOLBAR_TOGGLED_COLOR;
			if(GUILayout.Button("I", EditorStyles.toolbarButton))
			{
				infoLogs = !infoLogs;
				logEntries = LoadLogEntries();
			}

			GUI.backgroundColor = Color.white;

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Open Log", EditorStyles.toolbarButton))
				OpenBuggerLog();
		EditorGUILayout.EndHorizontal();
		GUI.EndGroup();
		rect.y += (headerHeight-3);
		/* </blech> */

		GUI.BeginGroup(rect);

		// keyColumn 		= new Rect(0f, 0f, Screen.width/3f, headerHeight);
		// messageColumn 	= new Rect(Screen.width/3f, 0f, Screen.width/3f, headerHeight);
		// dateColumn 		= new Rect((Screen.width/3f) * 2f, 0f, Screen.width/3f, headerHeight);

		keyColumn.height 		= rowHeight;
		messageColumn.height 	= rowHeight;
		dateColumn.height 		= rowHeight;

		float rowOffset = 0;

		bool scrollToBottom = false;

		if( Bugger.lastLogEntryTime - lastLogUpdate > .01f)
		{
			if(logScroll.y + (rect.height-16) >= logRect.height)
				scrollToBottom = true;

			logEntries = LoadLogEntries();
		}

		string logMessage;

		int logCount = logEntries == null ? 0 : logEntries.Count;

		logRect = new Rect(0f, 0, rect.width-16, logCount * rowHeight);

		if(scrollToBottom)
			logScroll.y = logRect.height - (rect.height-16);


		logScroll = GUI.BeginScrollView(new Rect(0, headerHeight-2, rect.width, rect.height-16), logScroll, logRect);

		for(int i = 0; i < logCount; i++)
		{
			BugLog entry = logEntries[i];

			logMessage = entry.message;
	
			keyColumn.y 	= rowOffset;	
			messageColumn.y = rowOffset;
			dateColumn.y 	= rowOffset;
			row 	 		= new Rect(0f, rowOffset, Screen.width, rowHeight);

			rowOffset += rowHeight;

			GUI.backgroundColor = i % 2 == 0 ? rowColorEven : rowColorOdd;

			if( selectedLog.Contains(i) )
			{
				GUI.backgroundColor = rowSelectedBlue;
			    rowTextStyle.normal.textColor = rowSelectedTextColorWhite;
			}
 
			else
			{
				rowTextStyle.normal.textColor = rowTextColor;
			}
			
			GUI.Box(row, "", rowBackgroundStyle);
			
			Color og = GUI.color;

				Texture2D icon;
				switch(entry.logType)
				{
					case LogType.Warning:
					case LogType.Assert:
						icon = warningIcon;
						break;

					case LogType.Error:
					case LogType.Exception:
						icon = errorIcon;
						break;

					default:
						icon = logIcon;
						break;
				}

				if(icon != null)
					GUI.DrawTexture(new Rect(0, row.yMin, icon_width, icon_width), icon, ScaleMode.ScaleToFit, true, 0f);

				Rect rowMod = new Rect(row.xMin + 30, row.yMin, row.width, row.height);
				GUI.Label(rowMod, logMessage, rowTextStyle);

				/* */
				PRO_SECONDARY_TEXT_COLOR = new Color(1f, 1f, 1f, .5f);
				string logMethod = (entry.stack.Count > 0) ? entry.stack[0].methodName : "";
				int logLine = (entry.stack.Count > 0) ? entry.stack[0].lineNumber : 0;
				GUI.color = EditorGUIUtility.isProSkin ? PRO_SECONDARY_TEXT_COLOR : BASIC_SECONDARY_TEXT_COLOR;
				if (!logMessage.Contains("\n"))
				{
					GUI.Label(rowMod, "\n" +logMethod + " : " + logLine + "  - " + entry.logType,  rowTextStyle);
				}
				/* */

			GUI.color = og;

			if(be.type == BuggerEventType.MouseUp && rect.Contains(e.mousePosition) && row.Contains(mPos))
			{
				selectedLog.Add(i);
				if(!be.shift)
					selectedValue.Clear();
				selectedValue.Add(logEntries[i]);
			}
		}

		GUI.backgroundColor = Color.white;

		GUI.EndScrollView();

		GUI.EndGroup();


		if(e.type == BuggerEventType.DoubleClick && rect.Contains(be.mousePosition) && selectedValue.Count > 0)
		{
			if( selectedValue[selectedValue.Count-1].stack.Count > 0 )
				GoToLine( selectedValue[selectedValue.Count-1].stack[0].fullPath, selectedValue[selectedValue.Count-1].stack[0].lineNumber);
		}
	}

	GUIContent infoContent;
	int selstack = -1;
	int sellog = -1;
	string stackIndent = "";
	Vector2 infoScroll = Vector2.zero;
	GUIStyle centeredLabel;// = new GUIStyle();

	private void DrawInfoPane(Rect rect, BuggerEvent e)
	{		

		rect.height = rect.height - 16;

		Color og = GUI.backgroundColor;

		GUI.backgroundColor = infoPaneBackgroundColor;
			GUI.Box(rect, "", rowBackgroundStyle);
		GUI.backgroundColor = Color.white;

		if(selectedValue.Count < 1) return;

		int[] contentHeight = new int[selectedValue.Count];

		// Set up GUIContent for render - need this to correctly calculate height for view portion of scrollvieew
		int lineHeight = (int)Mathf.Ceil(EditorGUIUtility.singleLineHeight);
		// int lineHeight = (int)Mathf.Ceil(EditorStyles.label.lineHeight);
		
		int viewHeight = 0;
		for(int i = 0; i < selectedValue.Count; i++)
		{
			contentHeight[i] = (int)Mathf.Ceil(stackStyle.CalcHeight(new GUIContent(selectedValue[i].message, ""), rect.width));
			viewHeight += contentHeight[i];
			viewHeight += (int)Mathf.Ceil(selectedValue[i].stack.Count * lineHeight+1);
		}

		viewHeight += 32;

		Vector2 mPos = e.mousePosition;
		mPos.y -= rect.y - infoScroll.y - lineHeight;
		Rect messageRect = new Rect(0, 0, rect.width, contentHeight[0]);
		int curHeight = 0;
		Rect stackRectBackground;

		infoScroll = GUI.BeginScrollView(rect, infoScroll, new Rect(0, 0, rect.width, viewHeight));
		{
			int j = 0;
			foreach(BugLog selectedLog in selectedValue)
			{
				messageRect.y = curHeight;
				messageRect.height = contentHeight[j];

				GUI.Label(messageRect, selectedLog.message, stackStyle);
				curHeight += contentHeight[j++];

				messageRect.y = curHeight;
				messageRect.height = lineHeight;
				curHeight += lineHeight;
				
				GUI.Label(messageRect, " - - - - - - - - - -", centeredLabel);

				stackIndent = "";
				Rect stackRect = new Rect(2f, curHeight, rect.width, lineHeight);
				for(int i = 0; i < selectedLog.stack.Count; i++)
				{	
					stackIndent += ">";
					GUI.backgroundColor = selstack == i && sellog == j ? rowSelectedBlue : Color.clear;
					stackRectBackground = stackRect;
					stackRectBackground.height += 2;
					GUI.Box(stackRectBackground, "", rowBackgroundStyle);
			
					GUI.Label(stackRect, stackIndent + " " + selectedLog.stack[i].methodName + " : " + selectedLog.stack[i].lineNumber);
					
					stackRect.y += (int)lineHeight;

					if(e.type == BuggerEventType.MouseUp && rect.Contains(e.mousePosition) && stackRect.Contains(mPos))
					{
						sellog = j;
						selstack = i;
						GoToLine(selectedLog.stack[i].fullPath, selectedLog.stack[i].lineNumber);
					}
				}
				curHeight = (int)(stackRect.y + stackRect.height);
			}

			GUI.backgroundColor = og;
		}
		GUI.EndScrollView();
	}
#endregion

#region Event Checks

	public bool ignoreMouse { get { return infoPaneRect.Contains(Event.current.mousePosition); } }
#endregion

#region Utility

	enum ParsingStatus {
		None,
		Message,
		LogType,
		MethodName,
		MethodPath,
		MethodLineNumber
	}

	bool infoLogs = true;
	bool errorLogs = true;
	bool warningLogs = false;

	private List<BugLog> LoadLogEntries()
	{
		ParsingStatus parseStatus = ParsingStatus.None;

		lastLogUpdate = EditorApplication.timeSinceStartup;

		if(!File.Exists(Bugger.LogPath))
			return new List<BugLog>();

		string json = File.ReadAllText(Bugger.LogPath);
		json += " ] }";

		JsonTextReader reader = new JsonTextReader(new StringReader(json));
		
		List<BugLog> entries = new List<BugLog>();
		int index = -1, stackIndex = -1;
		while (reader.Read())
		{
			switch(parseStatus)
			{
				case ParsingStatus.Message:
					entries.Add( new BugLog() );
					entries[++index].message = reader.Value.ToString();
					stackIndex = -1;
					break;

				case ParsingStatus.LogType:
					int lt = 0;
					int.TryParse(reader.Value.ToString(), out lt);
					entries[index].logType = (UnityEngine.LogType)lt;
					// UnityEngine.Debug.Log(reader.Value);
					break;

				case ParsingStatus.MethodName:
					entries[index].stack.Add( new MethodTrace() );
					entries[index].stack[++stackIndex].methodName = reader.Value.ToString();
					break;

				case ParsingStatus.MethodPath:
					entries[index].stack[stackIndex].fullPath = reader.Value.ToString();
					break;

				case ParsingStatus.MethodLineNumber:
					int line = -1;
					int.TryParse(reader.Value.ToString(), out line);
					entries[index].stack[stackIndex].lineNumber = line;
					break;
			}

			switch(reader.TokenType)
			{
				case JsonToken.PropertyName:
					switch(reader.Value.ToString())
					{
						case "message":
							parseStatus = ParsingStatus.Message;
							break;

						case "logtype":
							parseStatus = ParsingStatus.LogType;
							break;

						case "method":
							parseStatus = ParsingStatus.MethodName;
							break;

						case "path":
							parseStatus = ParsingStatus.MethodPath;
							break;

						case "lineNumber":
							parseStatus = ParsingStatus.MethodLineNumber;
							break;

						default:
							parseStatus = ParsingStatus.None;
							break;
					}
					break;
				
				default:
					parseStatus = ParsingStatus.None;
					break;
			}
		}
			
		// Todo!
		entries = entries.FindAll(x => 
			(infoLogs && x.logType == UnityEngine.LogType.Log) ||
			(warningLogs && x.logType == UnityEngine.LogType.Warning) ||
			(errorLogs && (x.logType == UnityEngine.LogType.Error || x.logType == UnityEngine.LogType.Assert || x.logType == UnityEngine.LogType.Exception))
			);

		if(collapse)
		{
			entries = entries.Distinct(new BugLogComparer()).ToList();
		}

		return entries;
	}

	public static void OpenBuggerLog()
	{
		if(File.Exists(Bugger.LogPath))
			System.Diagnostics.Process.Start( Bugger.LogPath );
	}

	public static void GoToLine(StackTrace stack)
	{
		StackFrame frame = stack.GetFrame(2);

		string filePathRel;
		if( !Bugger.RelativeFilePath(frame.GetFileName(), out filePathRel) )
			return;

		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(filePathRel, typeof(TextAsset));
		int lineNumber = frame.GetFileLineNumber();

		AssetDatabase.OpenAsset(obj, lineNumber);
	}

	public static void GoToLine(string path, int line)
	{
		string relPath;
		if( !Bugger.RelativeFilePath(path, out relPath))
			return;

		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(relPath, typeof(TextAsset));
		AssetDatabase.OpenAsset(obj, line);
	}

	public static void GoToLine(string fileNameWithNumber)
	{
		string[] split = fileNameWithNumber.Split(':');
		if(split.Length < 2) return;

		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(split[0], typeof(TextAsset));
		int line;
		if(int.TryParse(split[1], out line))
			AssetDatabase.OpenAsset(obj, line);
	}
#endregion
}
