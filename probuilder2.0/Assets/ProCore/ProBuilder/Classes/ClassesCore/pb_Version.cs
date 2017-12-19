// GENERATED FILE
// See build target ProBuilderAdvanced-UPM.json
#if DEBUG
using System;
using System.Reflection;
using System.Collections.Generic;
#endif

namespace ProBuilder.Core
{
	public static class pb_Version
	{
#if DEBUG
		public static pb_VersionInfo Current
		{
			get
			{
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
							return new pb_VersionInfo(
								v.major,
								v.minor,
								v.patch,
								v.build,
								VersionType.Development,
								System.DateTime.Now.ToString("en-US: MM/dd/yyyy"));
						}

					}
				}

				return new pb_VersionInfo("null", "null");
			}
		}
#else
		public static readonly pb_VersionInfo Current = new pb_VersionInfo("2.10.1f0", "en-US: 1-1-2017");
#endif
	}
}
