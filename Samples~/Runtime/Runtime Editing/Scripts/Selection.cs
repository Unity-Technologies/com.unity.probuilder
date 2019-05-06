using System;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace ProBuilder.Examples
{
	static class Selection
	{
		static HashSet<ProBuilderMesh> s_Selection = new HashSet<ProBuilderMesh>();

		public static bool Add(ProBuilderMesh mesh)
		{
			if(mesh == null)
				throw new ArgumentNullException("mesh");

			return s_Selection.Add(mesh);
		}

		public static void Remove(ProBuilderMesh mesh)
		{
			if(mesh == null)
				throw new ArgumentNullException("mesh");

			if(s_Selection.Contains(mesh))
				s_Selection.Remove(mesh);
		}

		public static bool Contains(ProBuilderMesh mesh)
		{
			return s_Selection.Contains(mesh);
		}

		public static void Clear()
		{
			s_Selection.Clear();
		}

		public static IEnumerable<ProBuilderMesh> meshes
		{
			get { return s_Selection; }
		}
	}
}
