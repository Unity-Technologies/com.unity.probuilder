using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/**
 *	Extract first changelog entry from changelog
 */
public static class GenerateCurrentTxt
{
	static int Main(string[] args)
	{
		if(args == null)
		{
			// mono gen_cur_txt.exe -f docs/changelog.txt > current.txt
			System.Console.WriteLine("Cannot invoke with no arguments");
			return 1;
		}

		// The text to regex on
		string m_Text = null;

		for(int i = 0; i < args.Length; i++)
		{
			if( args[i].Equals("-f") )
			{
				try {
					m_Text = File.ReadAllText(args[(++i)]);

				} catch {
					Console.WriteLine("Could not open file: " + args[(++i)]);
				}
			}
		}

		if(string.IsNullOrEmpty(m_Text))
		{
			Console.WriteLine("Couldn't open the changelog");
			return 1;
		}

		string[] split = Regex.Split(m_Text, "(?mi)^#\\s", RegexOptions.Multiline);

		Console.WriteLine(string.Format("# {0}", split[1]));

		return 0;
	}
}


