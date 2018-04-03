using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Contains information that the pb_AboutEntry.txt file holds.
	/// </summary>
	[System.Serializable]
	class pb_AboutEntry
	{
		public string name;
		public string identifier;
		public string version;
		public string date;
		public string changelogPath;

		public const string KEY_NAME = "name: ";
		public const string KEY_IDENTIFIER = "identifier: ";
		public const string KEY_VERSION = "version: ";
		public const string KEY_DATE = "date: ";
		public const string KEY_CHANGELOG = "changelog: ";
	}

	/// <summary>
	/// Utility methods for finding and extracting version & changelog information.
	/// </summary>
	static class pb_VersionUtil
	{
		/// <summary>
		/// Is the current version of ProBuilder greater than or equal to the passed version?
		/// </summary>
		/// <param name="major"></param>
		/// <param name="minor"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		public static bool IsGreaterThanOrEqualTo(int major, int minor, int patch)
		{
			pb_VersionInfo version = pb_Version.Current;

			if(version.major > major)
				return true;
			else if(version.major < major)
				return false;
			else if(version.minor > minor)
				return true;
			else if(version.minor < minor)
				return false;
			else if(version.patch > patch)
				return true;
			else if(version.patch < patch)
				return false;

			// are equal
			return true;
		}

		public static pb_VersionInfo GetVersionFromChangelog()
		{
			var changelogAsset = pb_FileUtil.LoadInternalAsset<TextAsset>("About/changelog.txt");

			if (changelogAsset == null)
				return new pb_VersionInfo("changelog not found", "");

			Match m = Regex.Match(changelogAsset.text, "(?<=# ProBuilder )[0-9\\-a-zA-Z.]*", RegexOptions.Multiline);

			pb_VersionInfo version;
			pb_VersionInfo.TryGetVersionInfo(m.Value, out version);
			return version;
		}

		/// <summary>
		/// Extracts and formats the latest changelog entry into rich text.  Also grabs the version.
		/// </summary>
		/// <param name="raw"></param>
		/// <param name="version"></param>
		/// <param name="formattedChangelog"></param>
		/// <returns></returns>
		public static bool FormatChangelog(string raw, out pb_VersionInfo version, out string formattedChangelog)
		{
			bool success = true;

			// get first version entry
			string[] split = Regex.Split(raw, "(?mi)^#\\s", RegexOptions.Multiline);

			// get the version info
			try
			{
				Match versionMatch = Regex.Match(split[1], @"(?<=^ProBuilder\s).[0-9]*\.[0-9]*\.[0-9]*[A-Z|a-z|\-]*\.[0-9]*");
				success = pb_VersionInfo.TryGetVersionInfo(versionMatch.Success ? versionMatch.Value : split[1].Split('\n')[0], out version);
			}
			catch
			{
				version = new pb_VersionInfo();
				success = false;
			}

			try
			{
				StringBuilder sb = new StringBuilder();
				string[] newLineSplit = split[1].Trim().Split('\n');
				for(int i = 2; i < newLineSplit.Length; i++)
					sb.AppendLine(newLineSplit[i]);

				formattedChangelog = sb.ToString();
				formattedChangelog = Regex.Replace(formattedChangelog, "^-", "\u2022", RegexOptions.Multiline);
				formattedChangelog = Regex.Replace(formattedChangelog, @"(?<=^##\\s).*", "<size=16><b>${0}</b></size>", RegexOptions.Multiline);
				formattedChangelog = Regex.Replace(formattedChangelog, @"^##\ ", "", RegexOptions.Multiline);
			}
			catch
			{
				formattedChangelog = "";
				success = false;
			}

			return success;
		}

		static pb_AboutEntry ParseAboutEntry(TextAsset aboutTextAsset)
		{
			if(aboutTextAsset == null)
				return null;

			pb_AboutEntry about = new pb_AboutEntry();

			foreach(string str in aboutTextAsset.text.Replace("\r\n", "\n").Split('\n'))
			{
				if(str.StartsWith(pb_AboutEntry.KEY_NAME))
					about.name = str.Replace(pb_AboutEntry.KEY_NAME, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_IDENTIFIER))
					about.identifier = str.Replace(pb_AboutEntry.KEY_IDENTIFIER, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_VERSION))
					about.version = str.Replace(pb_AboutEntry.KEY_VERSION, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_DATE))
					about.date = str.Replace(pb_AboutEntry.KEY_DATE, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_CHANGELOG))
					about.changelogPath = str.Replace(pb_AboutEntry.KEY_CHANGELOG, "").Trim();
			}

			return about;
		}
	}
}
