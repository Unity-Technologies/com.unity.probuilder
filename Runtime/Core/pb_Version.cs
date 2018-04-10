#if DEBUG || DEVELOPMENT
using System;
using System.Reflection;
#endif

namespace ProBuilder.Core
{
	public static class pb_Version
	{
#if DEBUG || DEVELOPMENT
		static pb_VersionInfo s_LoadedVersion = null;

		public static pb_VersionInfo Current
		{
			get
			{
				if (s_LoadedVersion != null)
					return s_LoadedVersion;

				// messy reflection to avoid editor dependency, and this is only in debug code anyways
				// iterate assemblies because the ProBuilder.EditorCore namespace can be in a couple different
				// places (UnityEditor, ProBuilder.EditorCore, future changes, etc)
				foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					var versionUtilType = assembly.GetType("ProBuilder.EditorCore.pb_VersionUtil");

					if (versionUtilType != null)
					{
						var getVersion = versionUtilType.GetMethod("GetVersionFromChangelog", BindingFlags.Public | BindingFlags.Static);

						if (getVersion != null)
						{
							var v = (pb_VersionInfo) getVersion.Invoke(null, null);

							// manually set to dev build
							return s_LoadedVersion = new pb_VersionInfo(
								v.major,
								v.minor,
								v.patch,
								v.build,
								"preview",
								System.DateTime.Now.ToString("en-US: MM/dd/yyyy"));
						}

					}
				}

				return s_LoadedVersion = new pb_VersionInfo("4.0.0-preview.0", "null");
			}
		}
#else
		public static readonly pb_VersionInfo Current = new pb_VersionInfo("4.0.0-preview.0", "en-US: 03/29/2018");
#endif
	}
}
