using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	[InitializeOnLoad]
	static class pb_ProBuilderUpdateCheck
	{
		const string PROBUILDER_VERSION_URL = "http://parabox.co/probuilder/current_probuilder_version.txt";
		static WWW updateQuery;

		static pb_ProBuilderUpdateCheck()
		{
			updateQuery = new WWW(PROBUILDER_VERSION_URL);
			EditorApplication.update += Update;
		}

		enum VersionType
		{
			Final,
			Beta,
			Patch
		}

		struct VersionInfo
		{
			public int major;
			public int minor;
			public int patch;
			public int build;
			public VersionType type;

			public override string ToString()
			{
				return string.Format("{0}.{1}.{2} {3} {4}", major, minor, patch, type, build);
			}
		}

		static void Update()
		{
			if (updateQuery != null)
			{
				if (!updateQuery.isDone)
					return;

				if (string.IsNullOrEmpty(updateQuery.error) || !Regex.IsMatch(updateQuery.text, "404 not found", RegexOptions.IgnoreCase) )
				{
					VersionInfo version;
					GetVersionInfo(updateQuery.text, out version);
					Debug.Log("Current Version: " + version.ToString());
				}
			}

			EditorApplication.update -= Update;
		}

		static bool GetVersionInfo(string str, out VersionInfo version)
		{
			version = new VersionInfo();

			string[] split = Regex.Split(str, @"[\.A-Za-z]");

			if(split.Length < 4)
				return false;

			Match type = Regex.Match(str, @"A-Za-z");

			int.TryParse(split[0], out version.major);
			int.TryParse(split[1], out version.minor);
			int.TryParse(split[2], out version.patch);
			int.TryParse(split[3], out version.build);
			version.type = GetVersionType(type != null && type.Success ? type.Value : "");

			return true;
		}

		static VersionType GetVersionType(string type)
		{
			if( type.Equals("b") || type.Equals("B") )
				return VersionType.Beta;
			else if( type.Equals("p") || type.Equals("P") )
				return VersionType.Patch;

			return VersionType.Final;
		}
	}
}
