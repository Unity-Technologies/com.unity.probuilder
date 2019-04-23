using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

namespace UnityEngine.ProBuilder
{
    [Serializable]
    class ChangelogEntry
    {
        [SerializeField]
        SemVer m_VersionInfo;

        [SerializeField]
        string m_ReleaseNotes;

        public SemVer versionInfo
        {
            get { return m_VersionInfo; }
        }

        public string releaseNotes
        {
            get { return m_ReleaseNotes; }
        }

        public ChangelogEntry(SemVer version, string releaseNotes)
        {
            m_VersionInfo = version;
            m_ReleaseNotes = releaseNotes;
        }

        public override string ToString()
        {
            return m_VersionInfo.ToString() + "\n\n" + m_ReleaseNotes;
        }
    }

    [Serializable]
    class Changelog
    {
        const string k_ChangelogEntryPattern = @"(##\s\[[0-9]+\.[0-9]+\.[0-9]+(\-[a-zA-Z]+(\.[0-9]+)*)*\])";
        const string k_VersionInfoPattern = @"(?<=##\s\[).*(?=\])";
        const string k_VersionDatePattern = @"(?<=##\s\[.*\]\s-\s)[0-9-]*";

        [SerializeField]
        List<ChangelogEntry> m_Entries;

        public ReadOnlyCollection<ChangelogEntry> entries
        {
            get { return new ReadOnlyCollection<ChangelogEntry>(m_Entries); }
        }

        public Changelog(string log)
        {
            string version = string.Empty;
            StringBuilder contents = null;
            m_Entries = new List<ChangelogEntry>();
            ChangelogEntry entry;

            foreach (var line in log.Split('\n'))
            {
                if (Regex.Match(line, k_ChangelogEntryPattern).Success)
                {
                    if ((entry = CreateEntry(version, contents != null ? contents.ToString() : "")) != null)
                        m_Entries.Add(entry);

                    version = line;
                    contents = new StringBuilder();
                }
                else
                {
                    if (contents != null)
                        contents.AppendLine(line);
                }
            }

            if ((entry = CreateEntry(version, contents.ToString())) != null)
                m_Entries.Add(entry);
        }

        ChangelogEntry CreateEntry(string version, string contents)
        {
            var mark = Regex.Match(version, k_VersionInfoPattern);
            var date = Regex.Match(version, k_VersionDatePattern);

            if (mark.Success)
                return new ChangelogEntry(new SemVer(mark.Value, date.Value), contents.Trim());

            return null;
        }
    }
}
