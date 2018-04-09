using System;
using UnityEditor;
using UnityEngine;

namespace ProBuilder.EditorCore
{
	public class pb_HandleGUI : IDisposable
	{
		bool m_SrgbWrite;

		public pb_HandleGUI()
		{
			Handles.BeginGUI();
			m_SrgbWrite = GL.sRGBWrite;
			GL.sRGBWrite = false;
		}

		public void Dispose()
		{
			GL.sRGBWrite = m_SrgbWrite;
			Handles.EndGUI();
		}
	}
}