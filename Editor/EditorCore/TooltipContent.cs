using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// An extended tooltip for use in pb_MenuAction.
	/// </summary>
	[System.Serializable]
	public class TooltipContent : System.IEquatable<TooltipContent>
	{
		static GUIStyle TitleStyle { get { if(_titleStyle == null) InitStyles(); return _titleStyle; } }
		static GUIStyle ShortcutStyle { get { if(_shortcutStyle == null) InitStyles(); return _shortcutStyle; } }
		static GUIStyle _titleStyle = null;
		static GUIStyle _shortcutStyle = null;

		const float MIN_WIDTH = 128;
		const float MAX_WIDTH = 330;
		const float MIN_HEIGHT = 0;
		const float MAX_HEIGHT = 1024;

		static void InitStyles()
		{
			_titleStyle = new GUIStyle();
			_titleStyle.margin = new RectOffset(4,4,4,4);
			_titleStyle.padding = new RectOffset(4,4,4,4);
			_titleStyle.fontSize = 14;
			_titleStyle.fontStyle = FontStyle.Bold;
			_titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
			_titleStyle.richText = true;

			_shortcutStyle = new GUIStyle(_titleStyle);
			_shortcutStyle.fontSize = 14;
			_shortcutStyle.fontStyle = FontStyle.Normal;
			_shortcutStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.5f, .5f, .5f, 1f) : new Color(.3f, .3f, .3f, 1f);

			EditorStyles.wordWrappedLabel.richText = true;
		}

		static readonly Color separatorColor = new Color(.65f, .65f, .65f, .5f);

		/// <summary>
		/// The title to show in the tooltip window.
		/// </summary>
		public string title;
		/// <summary>
		/// A brief summary of what this menu action does.
		/// </summary>
		public string summary;
		/// <summary>
		/// The shortcut assigned to this menu item.
		/// </summary>
		public string shortcut;

		internal static TooltipContent TempContent = new TooltipContent("", "");

		/// <summary>
		/// Create a new tooltip.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="summary"></param>
		/// <param name="shortcut"></param>
		public TooltipContent(string title, string summary, params char[] shortcut) : this(title, summary, "")
		{
			if(shortcut != null && shortcut.Length > 0)
			{
				this.shortcut = string.Empty;

				for(int i = 0; i < shortcut.Length - 1; i++)
				{
					if( !EditorUtility.IsUnix() )
						this.shortcut += InternalUtility.ControlKeyString(shortcut[i]) + " + ";
					else
						this.shortcut += shortcut[i] + " + ";
				}

				if( !EditorUtility.IsUnix() )
					this.shortcut += InternalUtility.ControlKeyString(shortcut[shortcut.Length - 1]);
				else
					this.shortcut += shortcut[shortcut.Length - 1];
			}
		}

		public TooltipContent(string title, string summary, string shortcut = "")
		{
			this.title = title;
			this.summary = summary;
			this.shortcut = shortcut;
		}

		/// <summary>
		/// Get the size required in GUI space to render this tooltip.
		/// </summary>
		/// <returns></returns>
		internal Vector2 CalcSize()
		{
			const float pad = 8;
			Vector2 total = new Vector2(MIN_WIDTH, MIN_HEIGHT);

			bool hastitle = !string.IsNullOrEmpty(title);
			bool hasSummary = !string.IsNullOrEmpty(summary);
			bool hasShortcut = !string.IsNullOrEmpty(shortcut);

			if(hastitle)
			{
				Vector2 ns = TitleStyle.CalcSize(UI.EditorGUIUtility.TempGUIContent(title));

				if(hasShortcut)
				{
					ns.x += EditorStyles.boldLabel.CalcSize(UI.EditorGUIUtility.TempGUIContent(shortcut)).x + pad;
				}

				total.x += Mathf.Max(ns.x, 256);
				total.y += ns.y;
			}

			if(hasSummary)
			{
				if(!hastitle)
				{
					Vector2 sumSize = EditorStyles.wordWrappedLabel.CalcSize(UI.EditorGUIUtility.TempGUIContent(summary));
					total.x = Mathf.Min(sumSize.x, MAX_WIDTH);
				}

				float summaryHeight = EditorStyles.wordWrappedLabel.CalcHeight(UI.EditorGUIUtility.TempGUIContent(summary), total.x);
				total.y += summaryHeight;
			}

			if(hastitle && hasSummary)
				total.y += 16;

			total.x += pad;
			total.y += pad;

			return total;
		}

		internal void Draw(bool hideShortcutText = false)
		{
			if(!string.IsNullOrEmpty(title))
			{
				if(!hideShortcutText && !string.IsNullOrEmpty(shortcut))
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(title, TitleStyle);
					GUILayout.FlexibleSpace();
					GUILayout.Label(shortcut, ShortcutStyle);
					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.Label(title, TitleStyle);
				}

				UI.EditorGUIUtility.DrawSeparator(1, separatorColor);
				GUILayout.Space(2);
			}

			if(!string.IsNullOrEmpty(summary))
			{
				GUILayout.Label(summary, EditorStyles.wordWrappedLabel);
			}
		}

		public bool Equals(TooltipContent tooltip)
		{
			return tooltip.title.Equals(this.title);
		}

		public override bool Equals(System.Object b)
		{
			return b is TooltipContent && ((TooltipContent)b).title.Equals(title);
		}

		public override int GetHashCode()
		{
			return title.GetHashCode();
		}

		public static explicit operator string(TooltipContent content)
		{
			return content.title;
		}

		public static explicit operator TooltipContent(string title)
		{
			return new TooltipContent(title, "");
		}
	}
}
