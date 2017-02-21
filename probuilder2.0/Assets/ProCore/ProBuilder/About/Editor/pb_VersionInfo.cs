using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	public enum VersionType
	{
		Final,
		Beta,
		Patch
	}

	[System.Serializable]
	public struct pb_VersionInfo
	{
		public int major;
		public int minor;
		public int patch;
		public int build;
		public VersionType type;
		public string text;
		public bool valid;

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2} {3} {4}", major, minor, patch, type, build);
		}

		public static pb_VersionInfo FromString(string str)
		{
			pb_VersionInfo version = new pb_VersionInfo();
			version.text = str;

			try
			{
				string[] split = Regex.Split(str, @"[\.A-Za-z]");
				Match type = Regex.Match(str, @"A-Za-z");
				int.TryParse(split[0], out version.major);
				int.TryParse(split[1], out version.minor);
				int.TryParse(split[2], out version.patch);
				int.TryParse(split[3], out version.build);
				version.type = GetVersionType(type != null && type.Success ? type.Value : "");
				version.valid = true;
			}
			catch
			{
				version.valid = false;
			}

			return version;
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
