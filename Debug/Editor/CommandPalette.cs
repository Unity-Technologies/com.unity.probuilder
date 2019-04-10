using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
	/// <remarks>
	/// https://www.dotnetperls.com/levenshtein
	/// </remarks>
	static class LevenshteinDistance
	{
		/// <summary>
		/// Compute the distance between two strings.
		/// </summary>
		public static int Compute(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = System.Math.Min(
						System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}

	static class DiceCoefficientExtensions
	{
		/// <summary>
		/// Dice Coefficient based on bigrams. <br />
		/// A good value would be 0.33 or above, a value under 0.2 is not a good match, from 0.2 to 0.33 is iffy.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="comparedTo"></param>
		/// <returns></returns>
		public static double DiceCoefficient(this string input, string comparedTo)
		{
			var ngrams = input.ToBiGrams();
			var compareToNgrams = comparedTo.ToBiGrams();
			return ngrams.DiceCoefficient(compareToNgrams);
		}

		/// <summary>
		/// Dice Coefficient used to compare nGrams arrays produced in advance.
		/// </summary>
		/// <param name="nGrams"></param>
		/// <param name="compareToNGrams"></param>
		/// <returns></returns>
		public static double DiceCoefficient(this string[] nGrams, string[] compareToNGrams)
		{
			int matches = 0;
			foreach (var nGram in nGrams)
			{
				if (compareToNGrams.Any(x => x == nGram)) matches++;
			}
			if (matches == 0) return 0.0d;
			double totalBigrams = nGrams.Length + compareToNGrams.Length;
			return (2 * matches) / totalBigrams;
		}

		public static string[] ToBiGrams(this string input)
		{
			// nLength == 2
			//   from Jackson, return %j ja ac ck ks so on n#
			//   from Main, return #m ma ai in n#
			input = SinglePercent + input + SinglePound;
			return ToNGrams(input, 2);
		}

		public static string[] ToTriGrams(this string input)
		{
			// nLength == 3
			//   from Jackson, return %%j %ja jac ack cks kso son on# n##
			//   from Main, return ##m #ma mai ain in# n##
			input = DoublePercent + input + DoublePount;
			return ToNGrams(input, 3);
		}

		private static string[] ToNGrams(string input, int nLength)
		{
			int itemsCount = input.Length - 1;
			string[] ngrams = new string[input.Length - 1];
			for (int i = 0; i < itemsCount; i++) ngrams[i] = input.Substring(i, nLength);
			return ngrams;
		}

		private const string SinglePercent = "%";
		private const string SinglePound = "#";
		private const string DoublePercent = "&&";
		private const string DoublePount = "##";
	}

	struct MenuActionAndFuzzyScore: IComparable<MenuActionAndFuzzyScore>
	{
		public MenuAction action;
		int m_Levenshtein;
		float m_DiceCoefficient;

		public float distance
		{
			get { return m_Levenshtein; }// * m_DiceCoefficient; }
		}

		public MenuActionAndFuzzyScore(MenuAction input, string compare)
		{
			var inp = input.menuTitle.ToLower();
			var cmp = compare.ToLower();
			m_Levenshtein = LevenshteinDistance.Compute(inp, cmp);
			m_DiceCoefficient = (float) DiceCoefficientExtensions.DiceCoefficient(inp, cmp);
			action = input;
		}

		public int CompareTo(MenuActionAndFuzzyScore other)
		{
			return distance.CompareTo(other.distance);
		}

		public bool ScoreIsAboveThreshold()
		{
			return m_Levenshtein < 20 && m_DiceCoefficient > .15f;
		}

		public override string ToString()
		{
#if DEBUG_FUZZY_SEARCH
			return m_Levenshtein + " (" + m_DiceCoefficient + ")  " + action.menuTitle;
#else
			return action.menuTitle;
#endif
		}
	}

	sealed class CommandPalette : EditorWindow
	{
		string m_SearchContent;
		List<MenuActionAndFuzzyScore> m_Results = new List<MenuActionAndFuzzyScore>();
		int m_SelectedIndex;
		const string k_SearchFieldControlName = "fuzzySearchField";
		bool m_FocusInitialized;
		Vector2 m_Scroll;
		static readonly Color k_SelectedColor = new Color(64/255f, 134/255f, 181/255f, .99f);

		static class Styles
		{
			static bool s_Initialized;
			public static GUIStyle selected;

			public static void Init()
			{
				if (s_Initialized)
					return;
				s_Initialized = true;
				selected = new GUIStyle(GUI.skin.label)
				{
					stretchWidth = true,
					normal = new GUIStyleState()
					{
						background = EditorGUIUtility.whiteTexture
					}
				};
			}
		}

		[MenuItem("Window/Command Palette %#o")]
		static void OpenCommandPalette()
		{
			GetWindow<CommandPalette>(true);
		}

		void OnEnable()
		{
			wantsMouseMove = true;
		}

		void OnGUI()
		{
			Styles.Init();

			var evt = Event.current;

			switch (evt.type)
			{
				case EventType.KeyUp:
					HandleKeyUp(evt);
					break;
			}

			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(k_SearchFieldControlName);
			m_SearchContent = EditorGUILayout.TextField(m_SearchContent);
			if (EditorGUI.EndChangeCheck())
				UpdateSearchResults();

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			for (int i = 0, c = m_Results.Count; i < c; i++)
			{
				if (m_SelectedIndex == i)
				{
					GUI.backgroundColor = k_SelectedColor;
					GUILayout.Label(m_Results[i].ToString(), Styles.selected);
					GUI.backgroundColor = Color.white;
				}
				else
				{
					GUILayout.Label(m_Results[i].ToString());
				}

				if (evt.isMouse)
				{
					if (GUILayoutUtility.GetLastRect().Contains(evt.mousePosition))
					{
						if (evt.type == EventType.MouseUp)
						{
							m_SelectedIndex = i;
							DoSelectedAction();
						}
						else if (evt.type == EventType.MouseMove)
						{
							m_SelectedIndex = i;
						}
					}

					Repaint();
				}
			}

			EditorGUILayout.EndScrollView();

			if (!m_FocusInitialized)
			{
				m_FocusInitialized = true;
				EditorGUI.FocusTextInControl(k_SearchFieldControlName);
			}
		}

		void UpdateSearchResults()
		{
			var results = EditorToolbarLoader.GetActions().Select(x => new MenuActionAndFuzzyScore(x, m_SearchContent));
			m_Results = results.Where(x => x.ScoreIsAboveThreshold()).ToList();
			m_Results.Sort();
		}

		void DoSelectedAction()
		{
			if (m_SelectedIndex > -1 && m_SelectedIndex < m_Results.Count)
			{
				var action = m_Results[m_SelectedIndex].action;
				EditorUtility.ShowNotification(action.DoAction().notification);
			}

			Close();
		}

		void HandleKeyUp(Event evt)
		{
			if (evt.keyCode == KeyCode.Escape)
				Close();
			if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
				DoSelectedAction();

			if (evt.keyCode == KeyCode.UpArrow)
			{
				if (m_SelectedIndex < 0 || m_SelectedIndex > m_Results.Count - 1)
					m_SelectedIndex = m_Results.Count - 1;
				else
					m_SelectedIndex = Math.Max(0, m_SelectedIndex - 1);
			}

			if (evt.keyCode == KeyCode.DownArrow)
			{
				if (m_SelectedIndex < 0 || m_SelectedIndex > m_Results.Count - 1)
					m_SelectedIndex = 0;
				else
					m_SelectedIndex = Math.Min(m_SelectedIndex + 1, m_Results.Count - 1);
			}

			Repaint();
		}
	}
}
