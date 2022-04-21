using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using System;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Represents an extended tooltip for a MenuAction.
    /// </summary>
    [Serializable]
    public sealed class TooltipContent : IEquatable<TooltipContent>
    {
        static GUIStyle TitleStyle { get { if (_titleStyle == null) InitStyles(); return _titleStyle; } }
        static GUIStyle ShortcutStyle { get { if (_shortcutStyle == null) InitStyles(); return _shortcutStyle; } }
        static GUIStyle _titleStyle = null;
        static GUIStyle _shortcutStyle = null;

        const float k_MinWidth = 128;
        const float k_MaxWidth = 330;
        const float k_MinHeight = 0;

        static void InitStyles()
        {
            _titleStyle = new GUIStyle();
            _titleStyle.margin = new RectOffset(4, 4, 4, 4);
            _titleStyle.padding = new RectOffset(4, 4, 4, 4);
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
        /// Gets or sets the title in the tooltip window.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets a brief summary of what this menu action does.
        /// </summary>
        public string summary { get; set; }

        /// <summary>
        /// Gets or sets a text representation of the (optional) shortcut assigned to this menu item.
        /// </summary>
        public string shortcut { get; set; }

        internal static TooltipContent TempContent = new TooltipContent("", "");

        /// <summary>
        /// Creates a new tooltip with a title, a summary, and an optional array of characters for the shortcut.
        ///
        /// To specify modifier keys, use the Windows control keys. ProBuilder manages switching to Linux
        /// control keys for the macOS and Linux versions of the Unity Editor.
        /// </summary>
        /// <param name="title">The header text for this tooltip.</param>
        /// <param name="summary">The body of the tooltip text. This should be kept brief.</param>
        /// <param name="shortcut">A set of keys to be displayed as the shortcut for this action.</param>
        public TooltipContent(string title, string summary, params char[] shortcut) : this(title, summary, "")
        {
            if (shortcut != null && shortcut.Length > 0)
            {
                this.shortcut = string.Empty;

                for (int i = 0; i < shortcut.Length - 1; i++)
                {
                    if (!EditorUtility.IsUnix())
                        this.shortcut += InternalUtility.ControlKeyString(shortcut[i]) + " + ";
                    else
                        this.shortcut += shortcut[i] + " + ";
                }

                if (!EditorUtility.IsUnix())
                    this.shortcut += InternalUtility.ControlKeyString(shortcut[shortcut.Length - 1]);
                else
                    this.shortcut += shortcut[shortcut.Length - 1];
            }
        }

        /// <summary>
        /// Creates a new tooltip with a title, a summary, and an optional string for the shortcut.
        /// </summary>
        /// <param name="title">The header text for this tooltip.</param>
        /// <param name="summary">The body of the tooltip text. This should be kept brief.</param>
        /// <param name="shortcut">A set of keys to be displayed as the shortcut for this action.</param>
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
            Vector2 total = new Vector2(k_MinWidth, k_MinHeight);

            bool hastitle = !string.IsNullOrEmpty(title);
            bool hasSummary = !string.IsNullOrEmpty(summary);
            bool hasShortcut = !string.IsNullOrEmpty(shortcut);

            if (hastitle)
            {
                Vector2 ns = TitleStyle.CalcSize(UI.EditorGUIUtility.TempContent(title));

                if (hasShortcut)
                {
                    ns.x += EditorStyles.boldLabel.CalcSize(UI.EditorGUIUtility.TempContent(shortcut)).x + pad;
                }

                total.x += Mathf.Max(ns.x, 256);
                total.y += ns.y;
            }

            if (hasSummary)
            {
                if (!hastitle)
                {
                    Vector2 sumSize = EditorStyles.wordWrappedLabel.CalcSize(UI.EditorGUIUtility.TempContent(summary));
                    total.x = Mathf.Min(sumSize.x, k_MaxWidth);
                }

                float summaryHeight = EditorStyles.wordWrappedLabel.CalcHeight(UI.EditorGUIUtility.TempContent(summary), total.x);
                total.y += summaryHeight;
            }

            if (hastitle && hasSummary)
                total.y += 16;

            total.x += pad;
            total.y += pad;

            return total;
        }

        internal void Draw()
        {
            if (!string.IsNullOrEmpty(title))
            {
                if (!string.IsNullOrEmpty(shortcut))
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

            if (!string.IsNullOrEmpty(summary))
            {
                GUILayout.Label(summary, EditorStyles.wordWrappedLabel);
            }
        }

        /// <summary>
        /// Compares the <see cref="title"/> property of each tooltip to determine whether
        /// the specified tooltip is equal to this one.
        /// </summary>
        /// <param name="other">The ToolTip to compare.</param>
        /// <returns>True if the title is the same; false otherwise.</returns>
        public bool Equals(TooltipContent other)
        {
            return other != null && other.title != null && other.title.Equals(this.title);
        }

        /// <summary>
        /// Compares the <see cref="title"/> property of each tooltip to determine whether
        /// the specified tooltip is equal to this one.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the title is the same; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is TooltipContent && ((TooltipContent)obj).title.Equals(title);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return title.GetHashCode();
        }

        /// <summary>
        /// Converts a tooltip to a string.
        /// </summary>
        /// <param name="content">The Tooltip to convert.</param>
        /// <returns>The title of content.</returns>
        /// <exception cref="ArgumentNullException">content is null.</exception>
        public static explicit operator string(TooltipContent content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            return content.title;
        }

        /// <summary>
        /// Creates a new Tooltip with only the specified title.
        /// </summary>
        /// <param name="title">The title for the new Tooltip.</param>
        /// <returns>A new Tooltip with title and no content.</returns>
        public static explicit operator TooltipContent(string title)
        {
            return new TooltipContent(title, "");
        }

        /// <summary>
        /// Converts a Tooltip to a string.
        /// </summary>
        /// <returns>The title of the Tooltip.</returns>
        public override string ToString()
        {
            return title;
        }

        /// <summary>
        /// Creates a new Tooltip with only the specified title.
        /// </summary>
        /// <param name="title">The title for the new Tooltip.</param>
        /// <returns>A new Tooltip with title and no content.</returns>
        public static TooltipContent FromString(string title)
        {
            return new TooltipContent(title, "");
        }
    }
}
