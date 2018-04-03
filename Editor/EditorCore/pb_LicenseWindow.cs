using UnityEditor;
using UnityEngine;
using System;

namespace ProBuilder.EditorCore
{
	class pb_LicenseWindow : EditorWindow
	{
		const string k_LicenseTextPath = "About/ThirdPartyLicenses.txt";
		const int k_MaxStringLength = 60000;
		TextAsset m_LicenseText;
		string[] m_LicenseContents;
		Vector2 m_Scroll;

		void OnEnable()
		{
			m_LicenseText = pb_FileUtil.LoadInternalAsset<TextAsset>(k_LicenseTextPath);
			int charCount = m_LicenseText.text.Length;
			int stringCount = charCount / k_MaxStringLength + 1;
			m_LicenseContents = new string[stringCount];
			int index = 0;
			for (int i = 0; i < stringCount; i++)
			{
				int length = Math.Min(k_MaxStringLength, charCount - index);
				m_LicenseContents[i] = m_LicenseText.text.Substring(index, length);
				index += length;
			}
		}

		void OnGUI()
		{
			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			for(int i = 0, c = m_LicenseContents.Length; i < c; i++)
				GUILayout.Label(m_LicenseContents[i], EditorStyles.wordWrappedLabel);

			EditorGUILayout.EndScrollView();
		}
	}
}