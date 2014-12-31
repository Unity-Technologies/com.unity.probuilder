@echo off

set unity_path_4="C:\Program Files (x86)\Unity 4.3.0\Editor\Unity.exe"
set unity_path_5="C:\Program Files\Unity 5.0.0b16\Editor\Unity.exe"
set msbuild="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set editor_debug="%CD%\probuilder2.0\Assets\ProCore\ProBuilder\Editor\Debug"
set procore_changelog="%CD%\probuilder2.0\Assets\ProCore\ProBuilder\About\changelog.txt"

:: Clean out Upgrade project
:: rd /S /q uv-upgrade-src\

svn update

:: echo UNITY PATH IS %unity_path%

:: clean out temp directory.
rd /s /q bin\temp\

rd /s /q probuilder2.0\Library

mkdir bin\
md bin\temp
mkdir bin\Debug

:: don't gunk up pb2 directory with build logs
mkdir logs

echo ================================== EXPORT RELEASE RESOURCES ==================================

:: Create resources pack (ExportReleaseResources dumps the pack in bin/temp)
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder2.0 -executeMethod AutomatedExport.ExportReleaseResources -logFile %CD%/logs/probuilder-release-resources-log.txt

echo ================================== EXPORT SOURCE PACK ==================================

:: Create source pack
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder2.0 -executeMethod AutomatedExport.ExportSourceRelease installDir:..\..\bin\temp\ -logFile %CD%/log/probuilder-source-compile-log.txt

:: md %CD%\probuilder-staging\Assets
rd /S /Q %CD%\probuilder-staging

echo ================================== CREATE STAGING PROJECT ==================================

%unity_path_4% -quit -batchMode -createProject %CD%\probuilder-staging

md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor
md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes

:: Copy AutomatedExport script into project
xcopy %editor_debug%\AutomatedExport.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Copy SvnManager to temp project
xcopy %editor_debug%\SvnManager.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Copy Ionic.Zip DLL into project
xcopy %editor_debug%\Ionic.Zip.dll %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

:: Import ProBuilder resources
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -importPackage %CD%\bin\temp\ProBuilder2(Resources).unitypackage

:: ================================ BUILD 4.3 + LIBRARIES ================================ {

	echo ================================== Build U4 Lib ==================================

	:: Path to Unity 4 linked Core
	:: Path to Unity 4 linked Mesh Ops
	set u4core="%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4.sln"
	set u4mesh="%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4.sln"
	set u4editor="%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4.sln"

	:: Build Core - (post-build script places dll in staging project)
	%msbuild% /p:DefineConstants="RELEASE;" /t:Clean,Build %u4core%

	echo Copy core lib to staging
	xcopy "%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4\bin\Debug\ProBuilderCore-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

	:: Build mesh editing classes
	%msbuild% /p:DefineConstants="RELEASE;" /t:Clean,Build %u4mesh%

	echo Copy mesh ops lib to staging
	xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4\bin\Debug\ProBuilderMeshOps-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
	
	:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
	%msbuild% /p:DefineConstants="RELEASE;UNITY_EDITOR;" /v:q /t:Clean,Build %u4editor%

	echo Copy editor lib to staging
	xcopy "%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4\bin\Debug\ProBuilderEditor-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

	echo ================================== EXPORT UNITY 4 PACK ==================================

	:: Export release pack for Unity 4.3 +
	%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:..\..\bin\temp\ ignore:UserMaterials.asset;plist.txt;pb_Profiler folderRootName:ProBuilder suffix:-unity4 generateVersionInfo:TRUE -logFile %CD%/logs/probuilder4.3-compile-log.txt
:: ================================ END   4.3 + LIBRARIES ================================ }

	echo ================================== CLEAN STAGING ==================================

	del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderCore-Unity4.dll"
	del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderMeshOps-Unity4.dll"
	del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\ProBuilderEditor-Unity4.dll"

:: ================================ BUILD 5.0 + LIBRARIES ================================ {
	
	echo ================================== BUILD U5 LIB ================================== 

	:: Path to Unity 5 linked Core
	:: Path to Unity 5 linked Mesh Ops
	set u5core="%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5.sln"
	set u5mesh="%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5.sln"
	set u5editor="%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5.sln"

	:: Build Core against Unity 5 libs
	%msbuild% /p:DefineConstants="RELEASE;" /t:Clean,Build %u5core%

	echo Copy core 5 to staging
	xcopy "%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5\bin\Debug\ProBuilderCore-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

	:: Build Mesh ops against Unity 5
	%msbuild% /p:DefineConstants="RELEASE;" /t:Clean,Build %u5mesh%

	echo Copy mesh ops 5 to staging
	xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5\bin\Debug\ProBuilderMeshOps-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

	:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
	%msbuild% /p:DefineConstants="RELEASE;UNITY_EDITOR;" /v:q /t:Clean,Build %u4editor%

	echo Copy editor lib to staging
	xcopy "%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5\bin\Debug\ProBuilderEditor-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

	echo ================================== EXPORT UNITY 5 PACK ==================================

	:: Export release pack for Unity 5.0 +
	%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:..\..\bin\temp\ ignore:UserMaterials.asset;plist.txt;pb_Profiler folderRootName:ProBuilder suffix:-unity5 generateVersionInfo:TRUE
:: ================================ END   5.0 + LIBRARIES ================================ }

:: Clear out staging project again, but this time repopulate it with a built pack + install stuff
rd /S /Q %CD%\probuilder-staging

echo ================================== CREATE FINAL STAGING PROJECT ==================================

%unity_path_4% -quit -batchMode -createProject %CD%\probuilder-staging

md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Install\Editor\
md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Install\Packs\
md %CD%\probuilder-staging\Assets\ProCore\ProBuilder\About\

xcopy %editor_debug%\AutomatedExport.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\
xcopy %editor_debug%\SvnManager.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\
xcopy %editor_debug%\Ionic.Zip.dll %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\Debug\

xcopy %procore_changelog% %CD%\probuilder-staging\Assets\ProCore\ProBuilder\About\


:: Bring in upgrade packages and QuickStart script -- (this brings in all packages preesent in temp)
:: Export using old 6by7 path so that the QuickStart overwrites the old one.  Also allows us to move 
:: the root to ProCore since ProCore doesn't exist yet
xcopy %CD%\bin\temp\ProBuilder2-v*.unitypackage %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Install\Packs\
xcopy %CD%\probuilder2.0\Assets\ProCore\ProBuilder\Install\Editor\pb_InstallScript.cs %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Install\Editor\

echo ================================== EXPORT FINAL PACK ==================================

%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease exportFolderPath:"Assets/ProCore" installDir:..\..\bin\Debug\ generateZip:TRUE -logFile %CD%/logs/probuilder4.3-compile-log.txt

echo ================================== COMPLETE! ================================== 

pause