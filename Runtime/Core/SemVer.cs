using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Version information container that is comparable.
    /// </summary>
    [Serializable]
    sealed class SemVer : IEquatable<SemVer>, IComparable<SemVer>, IComparable
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
        public string type { get { return m_Type != null ? m_Type : ""; } }
        public string metadata { get { return m_Metadata != null ? m_Metadata : ""; } }
        public string date { get { return m_Date != null ? m_Date : ""; } }

        /// <summary>
        /// Get a new version info with just the major, minor, and patch values.
        /// </summary>
        public SemVer MajorMinorPatch
        {
            get { return new SemVer(major, minor, patch); }
        }

        public const string DefaultStringFormat = "M.m.p-t.b";

        public SemVer()
        {
            m_Major = 0;
            m_Minor = 0;
            m_Patch = 0;
            m_Build = -1;
            m_Type = null;
            m_Date = null;
            m_Metadata = null;
        }

        public SemVer(string formatted, string date = null)
        {
            SemVer parsed;

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
#if PB_DEBUG
            else
            {
                Log.Error("Failed parsing version info: " + formatted);
            }
#endif
        }

        public SemVer(int major, int minor, int patch, int build = -1, string type = null, string date = null, string metadata = null)
        {
            m_Major = major;
            m_Minor = minor;
            m_Patch = patch;
            m_Build = build;
            m_Type = type;
            m_Metadata = metadata;
            m_Date = date;
        }

        public bool IsValid()
        {
            return major != -1 &&
                minor != -1 &&
                patch != -1;
        }

        public override bool Equals(object o)
        {
            return o is SemVer && Equals((SemVer)o);
        }

        public override int GetHashCode()
        {
            int hash = 13;

            unchecked
            {
                if (IsValid())
                {
                    hash = (hash * 7) + major.GetHashCode();
                    hash = (hash * 7) + minor.GetHashCode();
                    hash = (hash * 7) + patch.GetHashCode();
                    hash = (hash * 7) + build.GetHashCode();
                    hash = (hash * 7) + type.GetHashCode();
                }
                else
                {
                    return string.IsNullOrEmpty(metadata) ? metadata.GetHashCode() : base.GetHashCode();
                }
            }

            return hash;
        }

        public bool Equals(SemVer version)
        {
            if (object.ReferenceEquals(version, null))
                return false;

            if (IsValid() != version.IsValid())
                return false;

            if (IsValid())
            {
                return major == version.major &&
                    minor == version.minor &&
                    patch == version.patch &&
                    type.Equals(version.type) &&
                    build.Equals(version.build);
            }
            else
            {
                if (string.IsNullOrEmpty(metadata) || string.IsNullOrEmpty(version.metadata))
                    return false;

                return metadata.Equals(version.metadata);
            }
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as SemVer);
        }

        static int WrapNoValue(int value)
        {
            return value < 0 ? int.MaxValue : value;
        }

        public int CompareTo(SemVer version)
        {
            const int GREATER = 1;
            const int EVEN = 0;
            const int LESS = -1;

            if (object.ReferenceEquals(version, null))
                return GREATER;

            if (Equals(version))
                return EVEN;

            if (major > version.major)
                return GREATER;
            if (major < version.major)
                return LESS;
            if (minor > version.minor)
                return GREATER;
            if (minor < version.minor)
                return LESS;

            // missing values in the following categories are > than existing.

            if (WrapNoValue(patch) > WrapNoValue(version.patch))
                return GREATER;
            if (WrapNoValue(patch) < WrapNoValue(version.patch))
                return LESS;
            if (string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(version.type))
                return GREATER;
            if (!string.IsNullOrEmpty(type) && string.IsNullOrEmpty(version.type))
                return LESS;
            if (WrapNoValue(build) > WrapNoValue(version.build))
                return GREATER;
            if (WrapNoValue(build) < WrapNoValue(version.build))
                return LESS;

            return EVEN;
        }

        public static bool operator==(SemVer left, SemVer right)
        {
            if (object.ReferenceEquals(left, null))
                return object.ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator!=(SemVer left, SemVer right)
        {
            return !(left == right);
        }

        public static bool operator<(SemVer left, SemVer right)
        {
            if (object.ReferenceEquals(left, null))
                return !object.ReferenceEquals(right, null);

            return left.CompareTo(right) < 0;
        }

        public static bool operator>(SemVer left, SemVer right)
        {
            // null < null still equals false
            if (object.ReferenceEquals(left, null))
                return false;

            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(SemVer left, SemVer right)
        {
            return left == right || left < right;
        }

        public static bool operator>=(SemVer left, SemVer right)
        {
            return left == right || left > right;
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
                else if (c == 'M')
                    sb.Append(major);
                else if (c == 'm')
                    sb.Append(minor);
                else if (c == 'p')
                    sb.Append(patch);
                else if (c == 'b')
                    sb.Append(build);
                else if (c == 'T' || c == 't')
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

        /// <summary>
        /// Returns a string with all the information that this version contains, including date.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(ToString("M.m.p"));

            if (!string.IsNullOrEmpty(type))
            {
                sb.Append("-");
                sb.Append(type);

                if (build > -1)
                {
                    sb.Append(".");
                    sb.Append(build.ToString());
                }
            }

            if (!string.IsNullOrEmpty(date))
            {
                sb.Append(" ");
                sb.Append(date);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a pb_VersionInfo type from a string formatted in valid semantic versioning format.
        /// https://semver.org/
        /// Ex: TryGetVersionInfo("2.5.3-b.1", out info)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool TryGetVersionInfo(string input, out SemVer version)
        {
            version = new SemVer();
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
