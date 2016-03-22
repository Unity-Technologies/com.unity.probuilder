/*
Permission is hereby granted, free of charge, to any person  obtaining a copy of this software and associated documentation  files (the "Software"), to deal in the Software without  restriction, including without limitation the rights to use,  copy, modify, merge, publish, distribute, sublicense, and/or sell  copies of the Software, and to permit persons to whom the  Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

/// <summary> 
/// <para>A small data structure class hold values for making Doxygen config files </para>
/// </summary>
public class DoxygenConfig
{
	public string Project = PlayerSettings.productName;
	public string Synopsis = "";
	public string Version = "";
	public string ScriptsDirectory = Application.dataPath;
	public string DocDirectory = Application.dataPath.Replace("Assets", "Docs");
	public string PathtoDoxygen = "";

}

/// <summary>
/// <para> A Editor Plugin for automatic doc generation through Doxygen</para>
/// <para> Author: Jacob Pennock (http://Jacobpennock.com)</para>
/// <para> Version: 1.0</para>	 
/// </summary>
public class DoxygenWindow : EditorWindow 
{
	public static DoxygenWindow Instance;
	public enum WindowModes{Generate,Configuration,About}
	public string UnityProjectID = PlayerSettings.productName+":";
	public string AssestsFolder = Application.dataPath;
	public string[] Themes = new string[3] {"Default", "Dark and Colorful", "Light and Clean"};
	public int SelectedTheme = 1;
	WindowModes DisplayMode = WindowModes.Generate;
	static DoxygenConfig Config;
	static bool DoxyFileExists = false;
	StringReader reader;
	TextAsset basefile;
	float DoxyfileCreateProgress = -1.0f;
	float DoxyoutputProgress = -1.0f;
	string CreateProgressString = "Creating Doxyfile..";
	public string BaseFileString = null;
	public string DoxygenOutputString = null;
	public string CurentOutput = null;
	DoxyThreadSafeOutput DoxygenOutput = null; 
	List<string> DoxygenLog = null;
	bool ViewLog = false;
	Vector2 scroll;
	bool DocsGenerated = false;

	[MenuItem( "Window/Documentation with Doxygen" )]
	public static void Init()
	{
		Instance = (DoxygenWindow)EditorWindow.GetWindow( typeof( DoxygenWindow ), false, "Documentation" );
		Instance.minSize = new Vector2( 420, 245 );
		Instance.maxSize = new Vector2( 420, 720 );

	}

	void OnEnable()
	{
		LoadConfig();
		DoxyoutputProgress = 0;
	}

	void OnDisable()
	{
		DoxyoutputProgress = 0;
		DoxygenLog = null;
	}

	void OnGUI()
	{
		DisplayHeadingToolbar();
		switch(DisplayMode)
		{
			case WindowModes.Generate:
				GenerateGUI();
			break;

			case WindowModes.Configuration:
				ConfigGUI();
			break;

			case WindowModes.About:
				AboutGUI();
			break;
		}
	}

	void Update()
	{
		if(DoxygenOutput != null)
		{
			this.Repaint();

			if(DoxygenOutput.isStarted() && !DoxygenOutput.isFinished())
			{
				CurentOutput = DoxygenOutput.ReadLine();
				DoxyoutputProgress = DoxyoutputProgress + 0.1f;
				if(DoxyoutputProgress >= 0.9f)
					DoxyoutputProgress = 0.75f;
			}
        	if(DoxygenOutput.isFinished())
        	{
				SetTheme(SelectedTheme);
        		DoxygenLog = DoxygenOutput.ReadFullLog();
        		DoxyoutputProgress = -1.0f;
        		DoxygenOutput = null;
    			DocsGenerated = true;
    			EditorPrefs.SetBool(UnityProjectID+"DocsGenerated",DocsGenerated);
        	}
		}
	}

	void DisplayHeadingToolbar()
	{
		GUIStyle normalButton = new GUIStyle( EditorStyles.toolbarButton );
		normalButton.fixedWidth = 140;
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
		{
			GUILayout.FlexibleSpace();
			if( GUILayout.Toggle( DisplayMode == WindowModes.Generate, "Generate Documentation", normalButton ) )
			{
				DoxyfileCreateProgress = -1;
				DisplayMode = WindowModes.Generate;
			}
			if( GUILayout.Toggle( DisplayMode == WindowModes.Configuration, "Settings/Configuration", normalButton ) )
			{
				DisplayMode = WindowModes.Configuration;
			}
			if( GUILayout.Toggle( DisplayMode == WindowModes.About, "About", normalButton ) )
			{
				DoxyfileCreateProgress = -1;
				DisplayMode = WindowModes.About;
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}	

	void ConfigGUI()
	{
		GUILayout.Space (10);
		if(Config.Project == "Enter your Project name (Required)" || Config.Project == "" || Config.PathtoDoxygen == "" )
			GUI.enabled = false;
		if(GUILayout.Button ("Save Configuration and Build new DoxyFile", GUILayout.Height(40)))
		{
			MakeNewDoxyFile(Config);
		}
		if(DoxyfileCreateProgress >= 0)
		{
			Rect r = EditorGUILayout.BeginVertical();
			EditorGUI.ProgressBar(r, DoxyfileCreateProgress, CreateProgressString);
			GUILayout.Space(16);
			EditorGUILayout.EndVertical();
		}
		GUI.enabled = true;

		GUILayout.Space (20);
		GUILayout.Label("Set Path to Doxygen Install",EditorStyles.boldLabel);
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.PathtoDoxygen = EditorGUILayout.TextField("Doxygen.exe : ",Config.PathtoDoxygen);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.PathtoDoxygen = EditorUtility.OpenFilePanel  ("Where is doxygen.exe installed?","", "");
		EditorGUILayout.EndHorizontal();


		GUILayout.Space (20);
		GUILayout.Label("Provide some details about the project",EditorStyles.boldLabel);
		GUILayout.Space (5);
		Config.Project = EditorGUILayout.TextField("Project Name: ",Config.Project);
		Config.Synopsis = EditorGUILayout.TextField("Project Brief: ",Config.Synopsis);
		Config.Version = EditorGUILayout.TextField("Project Version: ",Config.Version);
		
		GUILayout.Space (15);
		GUILayout.Label("Select Theme",EditorStyles.boldLabel);
		GUILayout.Space (5);
		SelectedTheme = EditorGUILayout.Popup(SelectedTheme,Themes) ;		

		GUILayout.Space (20);
		GUILayout.Label("Setup the Directories",EditorStyles.boldLabel);
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.ScriptsDirectory = EditorGUILayout.TextField("Scripts folder: ",Config.ScriptsDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.ScriptsDirectory = EditorUtility.OpenFolderPanel("Select your scripts folder", Config.ScriptsDirectory, "");
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.DocDirectory = EditorGUILayout.TextField("Output folder: ",Config.DocDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.DocDirectory = EditorUtility.OpenFolderPanel("Select your ouput Docs folder", Config.DocDirectory, "");
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (5);
		GUILayout.Space (30);
		GUILayout.Label("By default Doxygen will search through your whole Assets folder for C# script files to document. Then it will output the documentation it generates into a folder called \"Docs\" that is placed in your project folder next to the Assets folder. If you would like to set a specific script or output folder you can do so above. ",EditorStyles.wordWrappedMiniLabel);
		GUILayout.Space (30);
		EditorGUILayout.EndHorizontal();
	}

	void AboutGUI()
	{
		GUIStyle CenterLable = new GUIStyle(EditorStyles.largeLabel);
		GUIStyle littletext = new GUIStyle(EditorStyles.miniLabel) ;
		CenterLable.alignment = TextAnchor.MiddleCenter;
		GUILayout.Space (20);
		GUILayout.Label( "Automatic C# Documentation Generation through Doxygen",CenterLable);
		GUILayout.Label( "Version: 1.1",CenterLable);
		GUILayout.Label( "By: Jacob Pennock",CenterLable);

		GUILayout.Space (20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Follow me for more Unity tips and tricks",littletext);
		GUILayout.Space (15);
		if(GUILayout.Button( "twitter"))
			Application.OpenURL("http://twitter.com/@JacobPennock");
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Visit my site for more plugins and tutorials",littletext);
		if(GUILayout.Button( "JacobPennock.com"))
			Application.OpenURL("http://www.jacobpennock.com/Blog/?cat=19");
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
	}

	void GenerateGUI()
	{
		if(DoxyFileExists)
		{
			GUILayout.Space (5);
			if(!DocsGenerated)
				GUI.enabled = false;
			if(GUILayout.Button ("Browse Documentation", GUILayout.Height(40)))
				Application.OpenURL("File://"+Config.DocDirectory+Path.DirectorySeparatorChar+"html"+Path.DirectorySeparatorChar+"annotated.html");
			GUI.enabled = true;	

			if(DoxygenOutput == null)
			{
				if(GUILayout.Button ("Run Doxygen", GUILayout.Height(40)))
				{
					DocsGenerated = false;
					RunDoxygen();
				}
					
				if(DocsGenerated && DoxygenLog != null)
				{
					if(GUILayout.Button( "View Doxygen Log",EditorStyles.toolbarDropDown))
						ViewLog = !ViewLog;
					if(ViewLog)
					{
						scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
						foreach(string logitem in DoxygenLog)
						{
							EditorGUILayout.SelectableLabel(logitem,EditorStyles.miniLabel,GUILayout.ExpandWidth(true));
						}
		            	EditorGUILayout.EndScrollView();
					}
				}
			}
			else
			{
				if(DoxygenOutput.isStarted() && !DoxygenOutput.isFinished())
				{
					Rect r = EditorGUILayout.BeginVertical();
					EditorGUI.ProgressBar(r, DoxyoutputProgress,CurentOutput );
					GUILayout.Space(40);
					EditorGUILayout.EndVertical();
				}
			}
		}
		else
		{
			GUIStyle ErrorLabel = new GUIStyle(EditorStyles.largeLabel);
			ErrorLabel.alignment = TextAnchor.MiddleCenter;
			GUILayout.Space(20);
			GUI.contentColor = Color.red;
			GUILayout.Label("You must set the path to your Doxygen install and \nbuild a new Doxyfile before you can generate documentation",ErrorLabel);
		}
	}

	public void readBaseConfig()
	{
		basefile = (TextAsset)Resources.Load("BaseDoxyfile", typeof(TextAsset));
		reader = new StringReader(basefile.text);
		if ( reader == null )
		   UnityEngine.Debug.LogError("BaseDoxyfile not found or not readable");
		else
		   BaseFileString = reader.ReadToEnd();
	}

	public void MakeNewDoxyFile(DoxygenConfig config)
	{
		SaveConfigtoEditor(config);
		CreateProgressString = "Creating Output Folder";
		DoxyfileCreateProgress = 0.1f;
		System.IO.Directory.CreateDirectory(config.DocDirectory);

		DoxyfileCreateProgress = 0.1f;
		string newfile = BaseFileString.Replace("PROJECT_NAME           =", "PROJECT_NAME           = "+"\""+config.Project+"\"");
		DoxyfileCreateProgress = 0.2f;
		newfile = newfile.Replace("PROJECT_NUMBER         =", "PROJECT_NUMBER         = "+config.Version);
		DoxyfileCreateProgress = 0.3f;
		newfile = newfile.Replace("PROJECT_BRIEF          =", "PROJECT_BRIEF          = "+"\""+config.Synopsis+"\"");
		DoxyfileCreateProgress = 0.4f;
		newfile = newfile.Replace("OUTPUT_DIRECTORY       =", "OUTPUT_DIRECTORY       = "+"\""+config.DocDirectory+"\"");
		DoxyfileCreateProgress = 0.5f;
		newfile = newfile.Replace("INPUT                  =", "INPUT                  = "+"\""+config.ScriptsDirectory+"\"");
		DoxyfileCreateProgress = 0.6f;

		switch(SelectedTheme)
		{
			case 0:
				newfile = newfile.Replace("GENERATE_TREEVIEW      = NO", "GENERATE_TREEVIEW      = YES");
			break;
			case 1:
				newfile = newfile.Replace("SEARCHENGINE           = YES", "SEARCHENGINE           = NO");
				newfile = newfile.Replace("CLASS_DIAGRAMS         = YES", "CLASS_DIAGRAMS         = NO");
			break;
		}

		CreateProgressString = "New Options Set";

		StringBuilder sb = new StringBuilder();
		sb.Append(newfile);
        StreamWriter NewDoxyfile = new StreamWriter(Path.Combine(config.DocDirectory, "Doxyfile"));
        
        NewDoxyfile.Write(sb.ToString());
        NewDoxyfile.Close();
        DoxyfileCreateProgress = 1.0f;
        CreateProgressString = "New Doxyfile Created!";
        DoxyFileExists = true;
        EditorPrefs.SetBool(UnityProjectID+"DoxyFileExists",DoxyFileExists);
	}

	void SaveConfigtoEditor(DoxygenConfig config)
	{
		EditorPrefs.SetString(UnityProjectID+"DoxyProjectName",config.Project);
		EditorPrefs.SetString(UnityProjectID+"DoxyProjectNumber",config.Version);
		EditorPrefs.SetString(UnityProjectID+"DoxyProjectBrief",config.Synopsis);
		EditorPrefs.SetString(UnityProjectID+"DoxyProjectFolder",config.ScriptsDirectory);
		EditorPrefs.SetString(UnityProjectID+"DoxyProjectOutput",config.DocDirectory);
		EditorPrefs.SetString("DoxyEXE", config.PathtoDoxygen);
		EditorPrefs.SetInt(UnityProjectID+"DoxyTheme", SelectedTheme);
	}

	void LoadConfig()
	{
		if(BaseFileString == null)
			readBaseConfig();
		if(Config == null)
		{
			if(!LoadSavedConfig())
				Config = new DoxygenConfig();
		}	
		if(EditorPrefs.HasKey(UnityProjectID+"DoxyFileExists"))
			DoxyFileExists = EditorPrefs.GetBool(UnityProjectID+"DoxyFileExists");
		if(EditorPrefs.HasKey(UnityProjectID+"DocsGenerated"))
			DocsGenerated = EditorPrefs.GetBool(UnityProjectID+"DocsGenerated");
		if(EditorPrefs.HasKey(UnityProjectID+"DoxyTheme"))
			SelectedTheme = EditorPrefs.GetInt(UnityProjectID+"DoxyTheme");
		if(EditorPrefs.HasKey("DoxyEXE"))
			Config.PathtoDoxygen = EditorPrefs.GetString("DoxyEXE");
	}

	bool LoadSavedConfig()
	{
		if( EditorPrefs.HasKey (UnityProjectID+"DoxyProjectName"))
		{
			Config = new DoxygenConfig();
			Config.Project = EditorPrefs.GetString(UnityProjectID+"DoxyProjectName");
			Config.Version = EditorPrefs.GetString(UnityProjectID+"DoxyProjectNumber");
			Config.Synopsis = EditorPrefs.GetString(UnityProjectID+"DoxyProjectBrief");
			Config.DocDirectory = EditorPrefs.GetString(UnityProjectID+"DoxyProjectOutput");
			Config.ScriptsDirectory = EditorPrefs.GetString(UnityProjectID+"DoxyProjectFolder");				
			return true;
		}
		return false;
	}

	public static void OnDoxygenFinished(int code)
	{
		if(code != 0)
		{
			UnityEngine.Debug.LogError("Doxygen finsished with Error: return code " + code +"\nCheck the Doxgen Log for Errors.\nAlso try regenerating your Doxyfile,\nyou will new to close and reopen the\ndocumentation window before regenerating.");
		}
	}

	void SetTheme(int theme)
	{
		char sep = Path.DirectorySeparatorChar;
		try
		{
			switch(theme)
			{
				case 1:
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"DarkTheme"+sep+"doxygen.css", Config.DocDirectory+sep+"html"+sep+"doxygen.css");
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"DarkTheme"+sep+"tabs.css", Config.DocDirectory+sep+"html"+sep+"tabs.css");
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"DarkTheme"+sep+"img_downArrow.png", Config.DocDirectory+sep+"html"+sep+"img_downArrow.png");
				break;
				case 2:
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"LightTheme"+sep+"doxygen.css", Config.DocDirectory+sep+"html"+sep+"doxygen.css");
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"LightTheme"+sep+"tabs.css", Config.DocDirectory+sep+"html"+sep+"tabs.css");
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"LightTheme"+sep+"img_downArrow.png", Config.DocDirectory+sep+"html"+sep+"img_downArrow.png");
	    			FileUtil.ReplaceFile(AssestsFolder + sep+"Doxygen"+sep+"Editor"+sep+"Resources"+sep+"LightTheme"+sep+"background_navigation.png", Config.DocDirectory+sep+"html"+sep+"background_navigation.png");
				break;
			}
		}
		catch (IOException ex)
		{
		    UnityEngine.Debug.LogError("Doxygen:Error: Unable to set custom theme, using default theme instead. Did you move the Doxygen folder away from the root of your project?\n"+ex);
		}
	}

	public void RunDoxygen()
	{
		string[] Args = new string[1];
		Args[0] = Path.Combine(Config.DocDirectory, "Doxyfile");

      	DoxygenOutput = new DoxyThreadSafeOutput();
      	DoxygenOutput.SetStarted();

      	Action<int> setcallback = (int returnCode) => OnDoxygenFinished(returnCode);

      	DoxyRunner Doxygen = new DoxyRunner(Config.PathtoDoxygen,Args,DoxygenOutput,setcallback);

      	Thread DoxygenThread = new Thread(new ThreadStart(Doxygen.RunThreadedDoxy));
      	DoxygenThread.Start();
	}

}

/// <summary>
///  This class spawns and runs Doxygen in a separate thread, and could serve as an example of how to create 
///  plugins for unity that call a command line application and then get the data back into Unity safely.	 
/// </summary>
public class DoxyRunner
{
	DoxyThreadSafeOutput SafeOutput;
	public Action<int> onCompleteCallBack;
	List<string> DoxyLog = new List<string>();
	public string EXE = null;
	public string[] Args;
	static string WorkingFolder;

	public DoxyRunner(string exepath, string[] args,DoxyThreadSafeOutput safeoutput,Action<int> callback)
	{
		EXE = exepath;
		Args = args;
		SafeOutput = safeoutput;
		onCompleteCallBack = callback;
		WorkingFolder = FileUtil.GetUniqueTempPathInProject();
		System.IO.Directory.CreateDirectory(WorkingFolder);
	}

	public void updateOuputString(string output)
	{
		SafeOutput.WriteLine(output);
		DoxyLog.Add(output);
	}

	public void RunThreadedDoxy()
	{
		Action<string> GetOutput = (string output) => updateOuputString(output);
		int ReturnCode = Run(GetOutput,null,EXE,Args);
		SafeOutput.WriteFullLog(DoxyLog);
		SafeOutput.SetFinished();
		onCompleteCallBack(ReturnCode);
	}

    /// <summary>
    /// Runs the specified executable with the provided arguments and returns the process' exit code.
    /// </summary>
    /// <param name="output">Recieves the output of either std/err or std/out</param>
    /// <param name="input">Provides the line-by-line input that will be written to std/in, null for empty</param>
    /// <param name="exe">The executable to run, may be unqualified or contain environment variables</param>
    /// <param name="args">The list of unescaped arguments to provide to the executable</param>
    /// <returns>Returns process' exit code after the program exits</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'
    public static int Run(Action<string> output, TextReader input, string exe, params string[] args)
    {
        if (String.IsNullOrEmpty(exe))
            throw new FileNotFoundException();
        if (output == null)
            throw new ArgumentNullException("output");

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute = false;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardInput = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        psi.ErrorDialog = false;
        psi.WorkingDirectory = WorkingFolder;
        psi.FileName = FindExePath(exe); 
        psi.Arguments = EscapeArguments(args); 

        using (Process process = Process.Start(psi))
        using (ManualResetEvent mreOut = new ManualResetEvent(false),
               mreErr = new ManualResetEvent(false))
        {
            process.OutputDataReceived += (o, e) => { if (e.Data == null) mreOut.Set(); else output(e.Data); };
        process.BeginOutputReadLine();
            process.ErrorDataReceived += (o, e) => { if (e.Data == null) mreErr.Set(); else output(e.Data); };
        process.BeginErrorReadLine();

            string line;
            while (input != null && null != (line = input.ReadLine()))
                process.StandardInput.WriteLine(line);

            process.StandardInput.Close();
            process.WaitForExit();

            mreOut.WaitOne();
            mreErr.WaitOne();
            return process.ExitCode;
        }
    }

    /// <summary>
    /// Quotes all arguments that contain whitespace, or begin with a quote and returns a single
    /// argument string for use with Process.Start().
    /// </summary>
    /// <param name="args">A list of strings for arguments, may not contain null, '\0', '\r', or '\n'</param>
    /// <returns>The combined list of escaped/quoted strings</returns>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'</exception>
    public static string EscapeArguments(params string[] args)
    {
        StringBuilder arguments = new StringBuilder();
        Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
        Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
        Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
        for (int carg = 0; args != null && carg < args.Length; carg++)
        {
            if (args[carg] == null)
            {
                throw new ArgumentNullException("args[" + carg + "]");
            }
            if (invalidChar.IsMatch(args[carg]))
            {
                throw new ArgumentOutOfRangeException("args[" + carg + "]");
            }
            if (args[carg] == String.Empty)
            {
                arguments.Append("\"\"");
            }
            else if (!needsQuotes.IsMatch(args[carg]))
            {
                arguments.Append(args[carg]);
            }
            else
            {
                arguments.Append('"');
                arguments.Append(escapeQuote.Replace(args[carg], m =>
                                                     m.Groups[1].Value + m.Groups[1].Value +
                                                     (m.Groups[2].Value == "\"" ? "\\\"" : "")
                                                    ));
                arguments.Append('"');
            }
            if (carg + 1 < args.Length)
                arguments.Append(' ');
        }
        return arguments.ToString();
    }


    /// <summary>
    /// Expands environment variables and, if unqualified, locates the exe in the working directory
    /// or the evironment's path.
    /// </summary>
    /// <param name="exe">The name of the executable file</param>
    /// <returns>The fully-qualified path to the file</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    public static string FindExePath(string exe)
    {
        exe = Environment.ExpandEnvironmentVariables(exe);
        if (!File.Exists(exe))
        {
            if (Path.GetDirectoryName(exe) == String.Empty)
            {
                foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                {
                    string path = test.Trim();
                    if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                        return Path.GetFullPath(path);
                }
            }
            throw new FileNotFoundException(new FileNotFoundException().Message, exe);
        }
        return Path.GetFullPath(exe);
    }	
}	


/// <summary>
///  This class encapsulates the data output by Doxygen so it can be shared with Unity in a thread share way.	 
/// </summary>
public class DoxyThreadSafeOutput
{
   private ReaderWriterLockSlim outputLock = new ReaderWriterLockSlim();
   private string CurrentOutput = "";  
   private List<string> FullLog = new List<string>();
   private bool Finished = false;
   private bool Started = false;

   public string ReadLine( )
   {
        outputLock.EnterReadLock();
        try
        {
            return CurrentOutput;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public void SetFinished( )
   {
        outputLock.EnterWriteLock();
        try
        {
            Finished = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public void SetStarted( )
   {
        outputLock.EnterWriteLock();
        try
        {
            Started = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public bool isStarted( )
   {
        outputLock.EnterReadLock();
        try
        {
            return Started;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public bool isFinished( )
   {
        outputLock.EnterReadLock();
        try
        {
            return Finished;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }
   
   public List<string> ReadFullLog()
   {
        outputLock.EnterReadLock();
        try
        {
            return FullLog;
        }
        finally
        {
            outputLock.ExitReadLock();
        } 
   }

   public void WriteFullLog(List<string> newLog)
   {
        outputLock.EnterWriteLock();
        try
        {
           FullLog = newLog;
        }
        finally
        {
            outputLock.ExitWriteLock();
        } 
   }

   public void WriteLine(string newOutput)
    {
        outputLock.EnterWriteLock();
        try
        {
            CurrentOutput = newOutput;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }
}


