using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class HandleGUI : IDisposable
	{
		bool m_SrgbWrite;

		public HandleGUI()
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