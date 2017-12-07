using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ProBuilder.Core
{
	public enum VersionType
	{
		Development = 0,
		Patch = 1,
		Beta = 2,
		Final = 3,
	}

	/// <summary>
	/// Version information container that is comparable.
	/// </summary>
	[Serializable]
	public class pb_VersionInfo : System.IEquatable<pb_VersionInfo>, System.IComparable<pb_VersionInfo>
	{
		int m_Major = -1;
		int m_Minor = -1;
		int m_Patch = -1;
		int m_Build = -1;
		VersionType m_Type;
		string m_Text;
		string m_Date;

		public int major { get { return m_Major; } }
		public int minor { get { return m_Minor; } }
		public int patch { get { return m_Patch; } }
		public int build { get { return m_Build; } }
		public VersionType type { get { return m_Type; } }
		public string text { get { return m_Text; } }
		public string date { get { return m_Date; } }

		public pb_VersionInfo()
		{
		}

		public pb_VersionInfo(int major, int minor, int patch, int build = 0, VersionType type = VersionType.Development, string date = "")
		{
			m_Major = major;
			m_Minor = minor;
			m_Patch = patch;
			m_Build = build;
			m_Type = type;
			m_Text = null;
			m_Date = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("en-US: MM/dd/yyyy") : date;
		}

		public bool IsValid()
		{
			return major != -1 &&
			       minor != -1 &&
			       patch != -1;
		}

		public override bool Equals(object o)
		{
			return o is pb_VersionInfo && this.Equals((pb_VersionInfo) o);
		}

		public override int GetHashCode()
		{
			int hash = 13;

			unchecked
			{
				if(IsValid() || string.IsNullOrEmpty(text))
				{
					hash = (hash * 7) + major.GetHashCode();
					hash = (hash * 7) + minor.GetHashCode();
					hash = (hash * 7) + patch.GetHashCode();
					hash = (hash * 7) + build.GetHashCode();
					hash = (hash * 7) + type.GetHashCode();
				}
				else
				{
					return text.GetHashCode();
				}
			}

			return hash;
		}

		public bool Equals(pb_VersionInfo version)
		{
			if(IsValid() != version.IsValid())
				return false;

			if(IsValid())
			{
				return 	major == version.major &&
						minor == version.minor &&
						patch == version.patch &&
						type == version.type &&
						build == version.build;
			}
			else
			{
				if( string.IsNullOrEmpty(text) || string.IsNullOrEmpty(version.text) )
					return false;

				return text.Equals(version.text);
			}
		}

		public int CompareTo(pb_VersionInfo version)
		{
			const int GREATER = 1;
			const int LESS = -1;

			if(this.Equals(version))
				return 0;
			else if(major > version.major)
				return GREATER;
			else if(major < version.major)
				return LESS;
			else if(minor > version.minor)
				return GREATER;
			else if(minor < version.minor)
				return LESS;
			else if(patch > version.patch)
				return GREATER;
			else if(patch < version.patch)
				return LESS;
			else if((int)type > (int)version.type)
				return GREATER;
			else if((int)type < (int)version.type)
				return LESS;
			else if(build > version.build)
				return GREATER;
			else
				return LESS;
		}

		/// <summary>
		/// Simple formatting for a version info. The following characters are available (any non-matching chars are appended
		/// as is).
		/// 'M' Major
		/// 'm' Minor
		/// 'p' Patch
		/// 'b' Build
		/// 't' Lowercase single type (f, d, b, or p)
		/// 'T' Type
		/// 'd' Date
		/// Ex, ToString("T:M.m.p") returns "Final:2.10.1"
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format)
		{
			var sb = new StringBuilder();

			foreach (char c in format.ToCharArray())
			{
				if(c == 'M')
					sb.Append(major);
				else if(c == 'm')
					sb.Append(minor);
				else if(c == 'p')
					sb.Append(patch);
				else if(c == 'b')
					sb.Append(build);
				else if(c == 't')
					sb.Append(char.ToLower(type.ToString()[0]));
				else if(c == 'T')
					sb.Append(type);
				else if (c == 'd')
					sb.Append(date);
				else
					sb.Append(c);
			}

			return sb.ToString();
		}

		public override string ToString()
		{
			return string.Format("{5} build {0}.{1}.{2}{3}{4} {6}", major, minor, patch, type.ToString().ToLower()[0], build, type, date);
		}

		/// <summary>
		/// Create a pb_VersionInfo type from a string.
		/// Ex: TryGetVersionInfo("2.5.3b1", out info)
		/// </summary>
		/// <param name="str"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public static bool TryGetVersionInfo(string str, out pb_VersionInfo version)
		{
			version = new pb_VersionInfo();
			version.m_Text = str;

			try
			{
				string[] split = Regex.Split(str, @"[\.A-Za-z]");
				Match type = Regex.Match(str, @"A-Za-z");
				int.TryParse(split[0], out version.m_Major);
				int.TryParse(split[1], out version.m_Minor);
				int.TryParse(split[2], out version.m_Patch);
				int.TryParse(split[3], out version.m_Build);
				version.m_Type = GetVersionType(type.Success ? type.Value : "");
				return true;
			}
			catch
			{
				return false;
			}
		}

		static VersionType GetVersionType(string type)
		{
			if( type.Equals("b") || type.Equals("B") )
				return VersionType.Beta;

			if( type.Equals("p") || type.Equals("P") )
				return VersionType.Patch;

			if( type.Equals("d") || type.Equals("D") )
				return VersionType.Development;

			return VersionType.Final;
		}
	}
}
