// http://netpl.blogspot.com/2011/10/reading-revision-number-of-local-copy.html

using System.IO;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;

public class SvnManager
{
#if UNITY_EDITOR && !UNITY_WEBPLAYER
	public static string ParentFolder { get; private set; }

	const string ENTRIES = "entries";

 	// if there is a .svn folder at every folder
	public static string RevisionNumber_Mac()
	{
		string SvnSubfolder = ParentFolder + "/.svn";

		if ( Directory.Exists( SvnSubfolder ) )
		{
			string EntriesFile = Directory.GetFiles( SvnSubfolder, ENTRIES ).FirstOrDefault();

			if ( !string.IsNullOrEmpty( EntriesFile ) )
			{
				string[] Lines = File.ReadAllLines( EntriesFile );
				if ( Lines.Length > 3 )
					return Lines[3];
			}
		}

		return string.Empty;
	}
	 
	const string DB      = "wc.db";
	// const string PATTERN = "/!svn/ver/(?'version'[0-9]*)/";
	const string PATTERN = "/!svn/rvr/(?'version'[0-9]*)/";
	// const string PATTERN = "/!svn/([a-z]*)/(?'version'[0-9]*)/";
	public static string RevisionNumber()
	{
		#if UNITY_STANDALONE_OSX
		string SvnSubfolder = ParentFolder + "/.svn";
		#else
		string SvnSubfolder = ParentFolder + "\\.svn";
 		#endif	

		if ( Directory.Exists( SvnSubfolder ) )
		{
			int maxVer = int.MinValue;
			string EntriesFile = Directory.GetFiles( SvnSubfolder, DB ).FirstOrDefault();

			if ( !string.IsNullOrEmpty( EntriesFile ) )
			{
				byte[] fileData;
				try {
					fileData = File.ReadAllBytes( EntriesFile );
				} catch {
					Debug.LogWarning("Please close Tortoise SVN.");
					return " KILL THE TORTOISE (SVN)";
				}

				string fileDataString = System.Text.Encoding.Default.GetString( fileData );
				Regex regex = new Regex( PATTERN );

				foreach ( Match match in regex.Matches( fileDataString ) )
				{
					string version = match.Groups["version"].Value;

					int curVer;
					if ( int.TryParse( version, out curVer ) )
						if ( curVer > maxVer )
							maxVer = curVer;
				}
				
				if ( maxVer > int.MinValue )
					return maxVer.ToString();
			}
		}

		return string.Empty;
	}

	public static string GetRevisionNumber()
	{
		if(ParentFolder == "" || ParentFolder == null)
			ParentFolder = GetSVNParentDirectory();
// #if UNITY_STANDALONE_OSX
		// return RevisionNumber_Mac();
// #else
		return RevisionNumber();
// #endif
	}

	public static string GetSVNParentDirectory()
	{
		bool noSvnFolder = true;
		string dir =  System.IO.Directory.GetParent( Application.dataPath ).ToString();
		string parentDirectory = "";
		int i = 0; // only go ten folders deep.
		while(noSvnFolder || i < 10)
		{
			if(System.IO.Directory.Exists(dir + "/.svn"))
			{
				parentDirectory = dir.ToString();
				return parentDirectory;
			}
			else
			{
				dir = System.IO.Directory.GetParent( dir ).ToString();
			}
			i++;
		}
		return parentDirectory;
	}
#endif

}
