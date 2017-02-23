using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	[InitializeOnLoad]
	static class pb_UpdateCheck
	{
		const string PROBUILDER_VERSION_URL = "http://parabox.co/probuilder/current_probuilder_version.txt";
		static WWW updateQuery;

		static pb_UpdateCheck()
		{
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbCheckForProBuilderUpdates))
				CheckForUpdate();
		}

		public static void CheckForUpdate()
		{
			if(updateQuery == null)
			{
				updateQuery = new WWW(PROBUILDER_VERSION_URL);
				EditorApplication.update += Update;
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
					pb_VersionInfo version;
					string changelog;

					pb_AboutWindow.FormatChangelog(updateQuery.text, out version, out changelog);
					pb_UpdateAvailable.Init(version, changelog);
					updateQuery = null;
				}
			}

			EditorApplication.update -= Update;
		}
	}
}
