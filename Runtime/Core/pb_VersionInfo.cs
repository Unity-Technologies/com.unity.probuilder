using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Version information container that is comparable.
	/// </summary>
	[Serializable]
	public class pb_VersionInfo : IEquatable<pb_VersionInfo>, IComparable<pb_VersionInfo>, IComparable
	{
		[SerializeField]
		int m_Major = -1;

		[SerializeField]
		int m_Minor = -1;

		[SerializeField]
		int m_Patch = -1;

		[SerializeField]
		int m_Build = -1;

		[SerializeField]
		string m_Type;

		[SerializeField]
		string m_Metadata;

		[SerializeField]
		string m_Date;

		public int major { get { return m_Major; } }
		public int minor { get { return m_Minor; } }
		public int patch { get { return m_Patch; } }
		public int build { get { return m_Build; } }
		public string type { get { return m_Type; } }
		public string metadata { get { return m_Metadata; } }
		public string date { get { return m_Date; } }

		/// <summary>
		/// Get a new version info with just the major, minor, and patch values.
		/// </summary>
		public pb_VersionInfo MajorMinorPatch
		{
			get { return new pb_VersionInfo(major, minor, patch); }
		}

		public const string DefaultStringFormat = "M.m.p-T.b";

		public pb_VersionInfo()
		{
		}

		public pb_VersionInfo(string formatted, string date = null)
		{
			pb_VersionInfo parsed;

			m_Metadata = formatted;
			m_Date = date;

			if (TryGetVersionInfo(formatted, out parsed))
			{
				m_Major = parsed.m_Major;
				m_Minor = parsed.m_Minor;
				m_Patch = parsed.m_Patch;
				m_Build = parsed.m_Build;
				m_Type = parsed.m_Type;
				m_Metadata = parsed.metadata;
			}
			else
			{
#if PB_DEBUG
				pb_Log.Error("Failed parsing version info: " + formatted);
#endif
			}
		}

		public pb_VersionInfo(int major, int minor, int patch, int build = -1, string type = "", string date = "", string metadata = "")
		{
			m_Major = major;
			m_Minor = minor;
			m_Patch = patch;
			m_Build = build;
			m_Type = type;
			m_Metadata = metadata;
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
				if(IsValid())
				{
					hash = (hash * 7) + major.GetHashCode();
					hash = (hash * 7) + minor.GetHashCode();
					hash = (hash * 7) + patch.GetHashCode();
					hash = (hash * 7) + build.GetHashCode();
					hash = (hash * 7) + type.GetHashCode();
				}
				else
				{
					return string.IsNullOrEmpty(m_Metadata) ? m_Metadata.GetHashCode() : base.GetHashCode();
				}
			}

			return hash;
		}

		public bool Equals(pb_VersionInfo version)
		{
			if (object.ReferenceEquals(version, null))
				return false;

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
				if( string.IsNullOrEmpty(m_Metadata) || string.IsNullOrEmpty(version.m_Metadata) )
					return false;

				return m_Metadata.Equals(version.m_Metadata);
			}
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as pb_VersionInfo);
		}

		static int WrapNoValue(int value)
		{
			return value < 0 ? int.MaxValue : value;
		}

		public int CompareTo(pb_VersionInfo version)
		{
			const int GREATER = 1;
			const int EVEN = 0;
			const int LESS = -1;

			if (object.ReferenceEquals(version, null))
				return GREATER;

			if(Equals(version))
				return EVEN;

			if(major > version.major)
				return GREATER;
			if(major < version.major)
				return LESS;
			if(minor > version.minor)
				return GREATER;
			if(minor < version.minor)
				return LESS;

			// missing values in the following categories are > than existing.

			if(WrapNoValue(patch) > WrapNoValue(version.patch))
				return GREATER;
			if(WrapNoValue(patch) < WrapNoValue(version.patch))
				return LESS;
			if(string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(version.type))
				return GREATER;
			if(!string.IsNullOrEmpty(type) && string.IsNullOrEmpty(version.type))
				return LESS;
			if(WrapNoValue(build) > WrapNoValue(version.build))
				return GREATER;
			if(WrapNoValue(build) < WrapNoValue(version.build))
				return LESS;

			return EVEN;
		}

		public static bool operator ==(pb_VersionInfo left, pb_VersionInfo right)
		{
			if (object.ReferenceEquals(left, null))
				return object.ReferenceEquals(right, null);

			return left.Equals(right);
		}
		public static bool operator !=(pb_VersionInfo left, pb_VersionInfo right)
		{
			return !(left == right);
		}
		public static bool operator <(pb_VersionInfo left, pb_VersionInfo right)
		{
			if (object.ReferenceEquals(left, null))
				return !object.ReferenceEquals(right, null);

			return left.CompareTo(right) < 0;
		}
		public static bool operator >(pb_VersionInfo left, pb_VersionInfo right)
		{
			// null < null still equals false
			if (object.ReferenceEquals(left, null))
				return false;

			return left.CompareTo(right) > 0;
		}

		/// <summary>
		/// Simple formatting for a version info. The following characters are available:
		/// 'M' Major
		/// 'm' Minor
		/// 'p' Patch
		/// 'b' Build
		/// 'T' Type
		/// 'd' Date
		/// 'D' Metadata
		/// Escape characters with '\'.
		/// </summary>
		/// <example>
		/// ToString("\buil\d: T:M.m.p") returns "build: Final:2.10.1"
		/// </example>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format)
		{
			var sb = new StringBuilder();
			bool skip = false;

			foreach (char c in format.ToCharArray())
			{
				if (skip)
				{
					sb.Append(c);
					skip = false;
					continue;
				}

				if (c == '\\')
					skip = true;
				else if(c == 'M')
					sb.Append(major);
				else if(c == 'm')
					sb.Append(minor);
				else if(c == 'p')
					sb.Append(patch);
				else if(c == 'b')
					sb.Append(build);
				else if(c == 'T')
					sb.Append(type);
				else if (c == 'd')
					sb.Append(date);
				else if (c == 'D')
					sb.Append(metadata);
				else
					sb.Append(c);
			}

			return sb.ToString();
		}

		public override string ToString()
		{
			return ToString(DefaultStringFormat);
		}

		/// <summary>
		/// Create a pb_VersionInfo type from a string formatted in valid semantic versioning format.
		/// https://semver.org/
		/// Ex: TryGetVersionInfo("2.5.3-b.1", out info)
		/// </summary>
		/// <param name="input"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public static bool TryGetVersionInfo(string input, out pb_VersionInfo version)
		{
			version = new pb_VersionInfo();
			bool ret = false;

			const string k_MajorMinorPatchRegex = "^([0-9]+\\.[0-9]+\\.[0-9]+)";
			const string k_VersionReleaseRegex = "(?i)(?<=\\-)[a-z0-9\\-]+";
			const string k_VersionBuildRegex = "(?i)(?<=\\-[a-z0-9\\-]+\\.)[0-9]+";
			const string k_MetadataRegex = "(?<=\\+).+";

			try
			{
				var mmp = Regex.Match(input, k_MajorMinorPatchRegex);

				if (!mmp.Success)
					return false;

				string[] mmpSplit = mmp.Value.Split('.');

				int.TryParse(mmpSplit[0], out version.m_Major);
				int.TryParse(mmpSplit[1], out version.m_Minor);
				int.TryParse(mmpSplit[2], out version.m_Patch);

				ret = true;

				// from here down is not required
				var preReleaseVersion = Regex.Match(input, k_VersionReleaseRegex);

				if (preReleaseVersion.Success)
					version.m_Type = preReleaseVersion.Value;
				else
					version.m_Type = "";

				var preReleaseBuild = Regex.Match(input, k_VersionBuildRegex);
				version.m_Build = preReleaseBuild.Success ? GetBuildNumber(preReleaseBuild.Value) : -1;

				var meta = Regex.Match(input, k_MetadataRegex);

				if (meta.Success)
					version.m_Metadata = meta.Value;
			}
			catch
			{
				ret = false;
			}

			return ret;
		}

		static int GetBuildNumber(string input)
		{
			var number = Regex.Match(input, "[0-9]+");

			int buildNo = 0;

			if (number.Success && int.TryParse(number.Value, out buildNo))
				return buildNo;

			return -1;
		}
	}
}
