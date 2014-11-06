set unity_path="C:\Program Files (x86)\Unity 3.5.7\Editor\Unity.exe"
set unity_path_4="C:\Program Files (x86)\Unity 4.3.0\Editor\Unity.exe"
set editor_debug="%CD%\probuilder2.0\Assets\ProCore\ProBuilder\Editor\Debug"
echo This assumes you have .NET 3.5 installed (Unity doesn't support 4 yet)

:: Clean out Upgrade project

svn update

echo UNITY PATH IS %unity_path%

:: clean out temp directory.
rd /s /q bin\temp\
rd /s /q probuilder2.0\Library\
md bin\temp

:: Create resources pack (ExportReleaseResources dumps the pack in bin/temp)
%unity_path% -quit -batchMode -projectPath %CD%\probuilder2.0 -executeMethod AutomatedExport.ExportReleaseResources ignore:Tools;MaterialSelection.cs;Geometry\ConnectEdges;Geometry\DetachDeleteFace;Geometry\FlipFaces;Geometry\VertexMergeWeld;Geometry\Bridge;API;MenuItems\Window;Actions\TriangulatePbObject.cs;Actions\DeleteNoDrawFaces.cs;Actions\ExportObj.cs;Actions\MakeAsset.cs;Actions\StripProBuilderScripts.cs;Geometry\ConformNormals.cs -logFile %CD%/prototype-release-resources-log.txt


:: md %CD%\probuilder-staging\Assets
rd /S /Q %CD%\probuilder-staging

%unity_path% -quit -batchMode -createProject %CD%\probuilder-staging

md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor
md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes

:: Copy AutomatedExport script into project
xcopy %editor_debug%\AutomatedExport.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Copy SvnManager to temp project
xcopy %editor_debug%\SvnManager.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Copy Ionic.Zip DLL into project
xcopy %editor_debug%\Ionic.Zip.dll %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Copy plist file to temp project
xcopy %editor_debug%\plist.txt %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Import ProBuilder resources
%unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -importPackage %CD%\bin\temp\ProBuilder2(Resources).unitypackage

:: ================================ BUILD 3.5 + LIBRARIES ================================ {

:: Build Core - (post-build script places dll in staging project)
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;" /t:Clean,Build "%CD%\visual studio\ProBuilderCore\ProBuilderCore.sln"
xcopy "%CD%\visual studio\ProBuilderCore\ProBuilderCore\bin\Debug\ProBuilderCore.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build mesh editing classes
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;" /t:Clean,Build "%CD%\visual studio\ProBuilderMeshOps\ProBuilderMeshOps.sln"
xcopy "%CD%\visual studio\ProBuilderMeshOps\ProBuilderMeshOps\bin\Debug\ProBuilderMeshOps.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build Editor Core
:: Compile with ErrorsOnly flag because we're building against 4.3 Editor, meaning all the Undo crap is going to 
:: throw crazy warnings.
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;" /v:q /clp:ErrorsOnly /t:Clean,Build "%CD%\visual studio\ProBuilderEditor\ProBuilderEditor.sln"
xcopy "%CD%\visual studio\ProBuilderEditor\ProBuilderEditor\bin\Debug\ProBuilderEditor.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"


:: %unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.Refresh
echo "GO INTO UNITY AND MANUALLY RENAME PROBUILDER FOLDER"
pause

:: Export release pack for Unity 3.5 +
%unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:..\..\bin\temp\ packageName:Prototype define:PROTOTYPE folderRootName:Prototype ignore:plist.txt;pb_Profiler postfix:-unity35 generateVersionInfo:TRUE -logFile %CD%/prototype.5.7-compile-log.txt

:: ================================ END   3.5 + LIBRARIES ================================ }

:: ================================ BUILD 4.3 + LIBRARIES ================================ {

:: Build Core - (post-build script places dll in staging project)
:: %SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;" /t:Clean,Build "%CD%\visual studio\ProBuilderCore\ProBuilderCore.sln"
:: xcopy /y "%CD%\visual studio\ProBuilderCore\ProBuilderCore\bin\Debug\ProBuilderCore.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build mesh editing classes
:: %SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;" /t:Clean,Build "%CD%\visual studio\ProBuilderMeshOps\ProBuilderMeshOps.sln"
:: xcopy "%CD%\visual studio\ProBuilderMeshOps\ProBuilderMeshOps\bin\Debug\ProBuilderMeshOps.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build Editor Core
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;UNITY_4_3" /t:Clean,Build "%CD%\visual studio\ProBuilderEditor\ProBuilderEditor.sln"
xcopy /y "%CD%\visual studio\ProBuilderEditor\ProBuilderEditor\bin\Debug\ProBuilderEditor.dll" "%CD%\probuilder-staging\Assets\ProCore\Prototype\Editor\"

:: Export release pack for Unity 4.3 +
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:..\..\bin\temp\ packageName:Prototype define:PROTOTYPE folderRootName:Prototype ignore:plist.txt;pb_Profiler postfix:-unity43 generateVersionInfo:TRUE

:: ================================ END   4.3 + LIBRARIES ================================ }


:: Clear out staging project again, but this time repopulate it with a built pack + install stuff
rd /S /Q %CD%\probuilder-staging

%unity_path% -quit -batchMode -createProject %CD%\probuilder-staging

md %CD%\probuilder-staging\Assets\ProCore\Prototype\Install\Editor\
md %CD%\probuilder-staging\Assets\ProCore\Prototype\Install\Packs\

xcopy %editor_debug%\AutomatedExport.cs %CD%\probuilder-staging\Assets\ProCore\Prototype\Editor\Debug\
xcopy %editor_debug%\SvnManager.cs %CD%\probuilder-staging\Assets\ProCore\Prototype\Editor\Debug\
xcopy %editor_debug%\Ionic.Zip.dll %CD%\probuilder-staging\Assets\ProCore\Prototype\Editor\Debug\
xcopy %editor_debug%\plist.txt %CD%\probuilder-staging\Assets\ProCore\Prototype\Editor\Debug\


:: Bring in upgrade packages and QuickStart script -- (this brings in all packages preesent in temp)
:: Export using old 6by7 path so that the QuickStart overwrites the old one.  Also allows us to move 
:: the root to ProCore since ProCore doesn't exist yet
xcopy %CD%\bin\temp\Prototype-v*.unitypackage %CD%\probuilder-staging\Assets\ProCore\Prototype\Install\Packs\
xcopy %CD%\probuilder2.0\Assets\ProCore\ProBuilder\Install\Editor\QuickStart2.cs %CD%\probuilder-staging\Assets\ProCore\Prototype\Install\Editor\

%unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease exportFolderPath:"Assets/ProCore" installDir:..\..\bin\Debug\ packageName:Prototype define:PROTOTYPE generateAbout:FALSE generateZip:TRUE

pause
