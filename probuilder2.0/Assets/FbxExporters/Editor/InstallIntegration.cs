using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace FbxExporters.Editor
{
    class Integrations
    {
        private const string MODULE_FILENAME = "unityoneclick";
        private const string PACKAGE_NAME = "FbxExporters";
        private const string VERSION_FILENAME = "README.txt";
        private const string VERSION_FIELD = "**Version**";
        private const string VERSION_TAG = "{Version}";
        private const string PROJECT_TAG = "{UnityProject}";

        public class MayaException : System.Exception {
            public MayaException() { }
            public MayaException(string message) : base(message) { }
            public MayaException(string message, System.Exception inner) : base(message, inner) { }
        }

        public class MayaVersion {

            /// <summary>
            /// Find the Maya installation that has your desired version, or
            /// the newest version if the 'desired' is an empty string.
            ///
            /// If MAYA_LOCATION is set, the desired version is ignored.
            /// </summary>
            public MayaVersion(string desiredVersion = "") {
                // If the location is given by the environment, use it.
                Location = System.Environment.GetEnvironmentVariable ("MAYA_LOCATION");
                if (!string.IsNullOrEmpty(Location)) {
                    Location = Location.TrimEnd('/');
                    Debug.Log("Using maya set by MAYA_LOCATION: " + Location);
                    return;
                }

                // List that directory and find the right version:
                // either the newest version, or the exact version we wanted.
                string mayaRoot = "";
                string bestVersion = "";
                var adskRoot = new System.IO.DirectoryInfo(AdskRoot);
                foreach(var productDir in adskRoot.GetDirectories()) {
                    var product = productDir.Name;

                    // Only accept those that start with 'maya' in either case.
                    if (!product.StartsWith("maya", StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }
                    // Reject MayaLT -- it doesn't have plugins.
                    if (product.StartsWith("mayalt", StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }
                    // Parse the version number at the end. Check if it matches,
                    // or if it's newer than the best so far.
                    string thisNumber = product.Substring("maya".Length);
                    if (thisNumber == desiredVersion) {
                        mayaRoot = product;
                        bestVersion = thisNumber;
                        break;
                    } else if (thisNumber.CompareTo(bestVersion) > 0) {
                        mayaRoot = product;
                        bestVersion = thisNumber;
                    }
                }
                if (!string.IsNullOrEmpty(desiredVersion) && bestVersion != desiredVersion) {
                    throw new MayaException(string.Format(
                                "Unable to find maya {0} in its default installation path. Set MAYA_LOCATION.", desiredVersion));
                } else if (string.IsNullOrEmpty(bestVersion)) {
                    throw new MayaException(string.Format(
                                "Unable to find any version of maya. Set MAYA_LOCATION."));
                }

                Location = AdskRoot + "/" + mayaRoot;
                if (string.IsNullOrEmpty(desiredVersion)) {
                    Debug.Log("Using latest version of maya found in: " + Location);
                } else {
                    Debug.Log(string.Format("Using maya {0} found in: {1}", desiredVersion, Location));
                }
            }

            /// <summary>
            /// The path where all the different versions of Maya are installed
            /// by default. Depends on the platform.
            /// </summary>
            public const string AdskRoot =
#if UNITY_EDITOR_OSX
                "/Applications/Autodesk"
#elif UNITY_EDITOR_LINUX
                "/usr/autodesk"
#else // WINDOWS
                "C:/Program Files/Autodesk"
#endif
            ;

            /// <summary>
            /// The value that you might set MAYA_LOCATION to if you wanted to
            /// use this version of Maya.
            /// </summary>
            public string Location { get; private set; }

            /// <summary>
            /// The path of the Maya executable.
            /// </summary>
            public string MayaExe {
                get {
#if UNITY_EDITOR_OSX
                    // MAYA_LOCATION on mac is set by Autodesk to be the
                    // Contents directory. But let's make it easier on people
                    // and allow just having it be the app bundle or a
                    // directory that holds the app bundle.
                    if (Location.EndsWith(".app/Contents")) {
                        return Location + "/MacOS/Maya";
                    } else if (Location.EndsWith(".app")) {
                        return Location + "/Contents/MacOS/Maya";
                    } else {
                        return Location + "/Maya.app/Contents/MacOS/Maya";
                    }
#elif UNITY_EDITOR_LINUX
                    return Location + "/bin/maya";
#else // WINDOWS
                    return Location + "/bin/maya.exe";
#endif
                }
            }

            /// <summary>
            /// The version number.
            ///
            /// This may involve running Maya so it can be expensive (a few
            /// seconds).
            /// </summary>
            public string Version {
                get {
                    if (string.IsNullOrEmpty(m_version)) {
                        m_version = AskVersion(MayaExe);
                    }
                    return m_version;
                }
            }
            string m_version;

            /// <summary>
            /// Ask the version number by running maya.
            /// </summary>
            static string AskVersion(string exePath) {
                System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
                myProcess.StartInfo.FileName = exePath;
                myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.Arguments = "-v";
                myProcess.EnableRaisingEvents = true;
                myProcess.Start();
                string resultString = myProcess.StandardOutput.ReadToEnd();
                myProcess.WaitForExit();

                // Output is like: Maya 2018, Cut Number 201706261615
                // We want the stuff after 'Maya ' and before the comma.
                // TODO: less brittle! Consider also the mel command "about -version".
                var commaIndex = resultString.IndexOf(',');
                return resultString.Substring(0, commaIndex).Substring("Maya ".Length);
            }
        };

        // Use string to define escaped quote
        // Windows needs the backslash
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        private const string ESCAPED_QUOTE = "\"";
#else
        private const string ESCAPED_QUOTE = "\\\"";
#endif

        private static string MAYA_COMMANDS { get {
            return string.Format("configureUnityOneClick {0}{1}{0} {0}{2}{0} {0}{3}{0} {4}; scriptJob -idleEvent quit;",
                    ESCAPED_QUOTE, GetProjectPath(), GetAppPath(), GetTempSavePath(), (IsHeadlessInstall()?1:0));
        }}
        private static Char[] FIELD_SEPARATORS = new Char[] {':'};

        private const string MODULE_TEMPLATE_PATH = "Integrations/Autodesk/maya"+VERSION_TAG+"/" + MODULE_FILENAME + ".txt";

#if UNITY_EDITOR_OSX
        private const string MAYA_MODULES_PATH = "Library/Preferences/Autodesk/Maya/"+VERSION_TAG+"/modules";
#elif UNITY_EDITOR_LINUX
        private const string MAYA_MODULES_PATH = "Maya/"+VERSION_TAG+"/modules";
#else
        private const string MAYA_MODULES_PATH = "maya/"+VERSION_TAG+"/modules";
#endif

        private static string GetUserFolder()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return System.Environment.GetEnvironmentVariable("HOME");
#else
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#endif
        }

        public static bool IsHeadlessInstall ()
        {
            return false;
        }

        public static string GetModulePath(string version)
        {
            string result = System.IO.Path.Combine(GetUserFolder(), MAYA_MODULES_PATH);

            return result.Replace(VERSION_TAG,version);
        }

        // GetModuleTemplatePath can support multiple versions of Maya in case
        // of changes in API.  But most versions are compatible with each
        // other, so just register one and make a mapping.
        static Dictionary<string, string> ModuleTemplateCompatibility = new Dictionary<string, string>() {
            { "2017", "2017" },
            { "2018", "2017" },
        };

        public static string GetModuleTemplatePath(string version)
        {
            string result = System.IO.Path.Combine(Application.dataPath, MODULE_TEMPLATE_PATH);
            if (!ModuleTemplateCompatibility.TryGetValue(version, out version)) {
                throw new MayaException("FbxExporters does not support Maya version " + version);
            }

            return result.Replace(VERSION_TAG,version);
        }

        public static string GetAppPath()
        {
            return EditorApplication.applicationPath.Replace("\\","/");
        }

        public static string GetProjectPath()
        {
            return System.IO.Directory.GetParent(Application.dataPath).FullName.Replace("\\","/");
        }

        public static string GetPackagePath()
        {
            return System.IO.Path.Combine(Application.dataPath, PACKAGE_NAME);
        }

        public static string GetTempSavePath()
        {
            return System.IO.Path.Combine(Application.dataPath, FbxExporters.Review.TurnTable.TempSavePath).Replace("\\", "/");
        }

        public static string GetPackageVersion()
        {
            string result = null;

            try {
                string FileName = System.IO.Path.Combine(GetPackagePath(), VERSION_FILENAME);

                System.IO.StreamReader sr = new System.IO.StreamReader(FileName);

                // Read the first line of text
                string line = sr.ReadLine();

                // Continue to read until you reach end of file
                while (line != null)
                {
                    if (line.StartsWith(VERSION_FIELD, StringComparison.CurrentCulture))
                    {
                        string[] fields = line.Split(FIELD_SEPARATORS);

                        if (fields.Length>1)
                        {
                            result = fields[1];
                        }
                        break;
                    }
                    line = sr.ReadLine();
                }
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Exception failed to read file containing package version ({0})", e.Message));
            }

            return result;
        }

        private static List<string> ParseTemplateFile(string FileName, Dictionary<string,string> Tokens )
        {
            List<string> lines = new List<string>();

            try
            {
                // Pass the file path and file name to the StreamReader constructor
                System.IO.StreamReader sr = new System.IO.StreamReader(FileName);

                // Read the first line of text
                string line = sr.ReadLine();

                // Continue to read until you reach end of file
                while (line != null)
                {
                    foreach(KeyValuePair<string, string> entry in Tokens)
                    {
                        line = line.Replace(entry.Key, entry.Value);
                    }
                    lines.Add(line);

                    //Read the next line
                    line = sr.ReadLine();
                }

                //close the file
                sr.Close();
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Exception reading module file template ({0})", e.Message));
            }

            return lines;
        }

        private static void WriteFile(string FileName, List<string> Lines )
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                System.IO.StreamWriter sw = new System.IO.StreamWriter(FileName);

                foreach (string line in Lines)
                {
                    //Write a line of text
                    sw.WriteLine(line);
                }

                //Close the file
                sw.Close();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                Debug.LogError(string.Format("Exception while writing module file ({0})", e.Message));
            }
        }

        public static int ConfigureMaya(MayaVersion version)
        {
             int ExitCode = 0;

             try {
                string mayaPath = version.MayaExe;
                if (!System.IO.File.Exists(mayaPath))
                {
                    Debug.LogError (string.Format ("No maya installation found at {0}", mayaPath));
                    return -1;
                }

                System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
                myProcess.StartInfo.FileName = mayaPath;
                myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.UseShellExecute = false;

#if UNITY_EDITOR_OSX
                myProcess.StartInfo.Arguments = string.Format(@"-command '{0}'", MAYA_COMMANDS);
#elif UNITY_EDITOR_LINUX
                throw new NotImplementedException();
#else // UNITY_EDITOR_WINDOWS
                myProcess.StartInfo.Arguments = string.Format("-command \"{0}\"", MAYA_COMMANDS);
#endif
                myProcess.EnableRaisingEvents = true;
                myProcess.Start();
                myProcess.WaitForExit();
                ExitCode = myProcess.ExitCode;
                Debug.Log(string.Format("Ran maya: [{0}]\nWith args [{1}]\nResult {2}",
                            mayaPath, myProcess.StartInfo.Arguments, ExitCode));
             }
             catch (Exception e)
             {
                UnityEngine.Debug.LogError(string.Format ("Exception failed to start Maya ({0})", e.Message));
                ExitCode = -1;
             }
            return ExitCode;
        }

        public static bool InstallMaya(MayaVersion version = null, bool verbose = false)
        {
            // What's happening here is that we copy the module template to
            // the module path, basically:
            // - copy the template to the user Maya module path
            // - search-and-replace its tags
            // - done.
            // But it's complicated because we can't trust any files actually exist.
            if (version == null) {
                version = new MayaVersion();
            }

            string moduleTemplatePath = GetModuleTemplatePath(version.Version);
            if (!System.IO.File.Exists(moduleTemplatePath))
            {
                Debug.LogError(string.Format("FbxExporters package doesn't have support for " + version.Version));
                return false;
            }

            // Create the {USER} modules folder and empty it so it's ready to set up.
            string modulePath = GetModulePath(version.Version);
            string moduleFilePath = System.IO.Path.Combine(modulePath, MODULE_FILENAME + ".mod");
            bool installed = false;

            if (!System.IO.Directory.Exists(modulePath))
            {
                if (verbose) { Debug.Log(string.Format("Creating Maya Modules Folder {0}", modulePath)); }

                try
                {
                    System.IO.Directory.CreateDirectory(modulePath);
                }
                catch (Exception xcp)
                {
                    Debug.LogException(xcp);
                    Debug.LogError(string.Format("Failed to create Maya Modules Folder {0}", modulePath));
                    return false;
                }

                if (!System.IO.Directory.Exists(modulePath)) {
                    Debug.LogError(string.Format("Failed to create Maya Modules Folder {0}", modulePath));
                    return false;
                }

                installed = false;
            }
            else
            {
                // detect if unityoneclick.mod is installed
                installed = System.IO.File.Exists(moduleFilePath);

                if (installed)
                {
                    // FIXME: remove this when we support parsing existing .mod files
                    try {
                        if (verbose) { Debug.Log(string.Format("Deleting module file {0}", moduleFilePath)); }
                        System.IO.File.Delete(moduleFilePath);
                        installed = false;
                    }
                    catch (Exception xcp)
                    {
                        Debug.LogException(xcp);
                        Debug.LogWarning(string.Format ("Failed to delete plugin module file {0}", moduleFilePath));
                    }
                }
            }

            // if not installed
            if (!installed)
            {
                Dictionary<string,string> Tokens = new Dictionary<string,string>()
                {
                    {VERSION_TAG, GetPackageVersion() },
                    {PROJECT_TAG, GetProjectPath() }
                 };

                // parse template, replace "{UnityProject}" with project path
                List<string> lines = ParseTemplateFile(moduleTemplatePath, Tokens);

                if (verbose) Debug.Log(string.Format("Copying plugin module file to {0}", moduleFilePath));

                // write out .mod file
                WriteFile(moduleFilePath, lines);
            }
            else
            {
                throw new NotImplementedException();

                // TODO: parse installed .mod file

                // TODO: if maya version not installed add

                // TODO: else check installation path

                // TODO: if installation path different

                // TODO: print message package already installed else where
            }

            return true;
        }
    }

    namespace Editors
    {
        class IntegrationsUI
        {
            const string MenuItemName1 = "FbxExporters/Install Maya Integration";

            [MenuItem (MenuItemName1, false, 0)]
            public static void OnMenuItem1 ()
            {
                var mayaVersion = new Integrations.MayaVersion();
                if (!Integrations.InstallMaya(mayaVersion, verbose: true)) {
                    return;
                }

                int exitCode = Integrations.ConfigureMaya (mayaVersion);

                string title, message;
                if (exitCode != 0) {
                    title = string.Format("Failed to install Maya {0} Integration.", mayaVersion.Version);
                    message = string.Format("Failed to configure Maya, please check logs (exitcode={0}).", exitCode);
                } else {
                    title = string.Format("Completed installation of Maya {0} Integration.", mayaVersion.Version);
                    message = string.Format("Enjoy the new \"Unity\" menu in Maya {0}.", mayaVersion.Version);
                }
                UnityEditor.EditorUtility.DisplayDialog (title, message, "Ok");
            }
        }
    }
}
