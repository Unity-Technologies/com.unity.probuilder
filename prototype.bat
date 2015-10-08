@echo off

:: Builds for Unity 4.6 & 5.0.1

set unity_path_4="D:\Applications\Unity 4.6.8f1\Editor\Unity.exe"
set unity_path_5="D:\Applications\Unity 5.0.0f4\Editor\Unity.exe"
set msbuild="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set build_directory="%CD%\bin\Debug"

set editor_debug="%CD%\probuilder2.0\Assets\ProCore\ProBuilder\Editor\Debug"
 
svn update

echo ===: UNITY 4 PATH IS %unity_path_4%
echo ===: UNITY 5 PATH IS %unity_path_5%

:: clean out temp directory.
echo ===: clean out temp directory
rd /s /q bin\temp\
rd /s /q probuilder2.0\Library\
md bin\temp
rd /s /q probuilder-staging\

echo ===: Create staging project
%unity_path_4% -quit -batchMode -createProject %CD%\probuilder-staging

echo ===: Copy export scripts
xcopy /E /Y /I %CD%\probuilder2.0\Assets\Debug\Editor\pb_AddDefine.cs %CD%\probuilder-staging\Assets\Debug\Editor\
xcopy /E /Y /I %CD%\probuilder2.0\Assets\Debug\Editor\pb_ExportPackage.cs %CD%\probuilder-staging\Assets\Debug\Editor\

echo ===: Copy Resources
xcopy /E /Y /I /q %CD%\probuilder2.0\Assets\ProCore %CD%\probuilder-staging\Assets\ProCore

echo ===: Prefix files with #define PROTOTYPE

%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod pb_AddDefine.PrependDefine define:PROTOTYPE ignore:Debug

echo ===: Remove core, mesh ops, and editor core

rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesCore
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesEditing
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\EditorCore

:: for prototype, remove all kinds of other stuff 

:: rd /S /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\API Examples"
rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Debug
rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Tools
rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Window
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_ExportObj.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_MakeMeshAsset.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_ProBuilderize.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_StripProBuilderScripts.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_BridgeEdges.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_ConformNormals.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_ConnectEdges.cs
:: del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_DetachDeleteFace.cs
:: del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_SetPivot.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_FreezeTransform.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_MergeFaces.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_Triangulate.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_VertexMergeWeld.cs

:: ================================ BUILD 4.3 + LIBRARIES ================================ {

echo ===: Build U4 DLL

:: Path to Unity 4 linked Core
:: Path to Unity 4 linked Mesh Ops
set u4core="%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4.sln"
set u4mesh="%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4.sln"
set u4editor="%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4.sln"

:: Build Core - (post-build script places dll in staging project)
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_4_6;" /t:Clean,Build %u4core%

echo ===: Copy core lib to staging
xcopy "%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4\bin\Debug\ProBuilderCore-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build mesh editing classes
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_4_6;" /t:Clean,Build %u4mesh%

echo ===: Copy mesh ops lib to staging
xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4\bin\Debug\ProBuilderMeshOps-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;UNITY_4_6;" /v:q /t:Clean,Build %u4editor%

echo ===: Copy editor lib to staging
xcopy "%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4\bin\Debug\ProBuilderEditor-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

echo ===: renaming ProBuilder to ProBuilder Basic
move /Y "%CD%\probuilder-staging\Assets\ProCore\ProBuilder" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic"

echo ===: Export Unity 4 Package

echo ===: Override DLL GUIDs
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

:: Export release pack for Unity 4.3 +
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\prototype4.6-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilderBasic outSuffix:-unity4

echo ===: Clean Staging

del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Classes\ProBuilderCore-Unity4.dll"
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Classes\ProBuilderMeshOps-Unity4.dll"
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Editor\ProBuilderEditor-Unity4.dll"

echo ===: Build U5 DLL

:: Path to Unity 5 linked Core
:: Path to Unity 5 linked Mesh Ops
set u5core="%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5.sln"
set u5mesh="%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5.sln"
set u5editor="%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5.sln"

:: Build Core against Unity 5 libs
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_0;" /t:Clean,Build %u5core%

echo ===: Copy core 5 to staging
xcopy "%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5\bin\Debug\ProBuilderCore-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Classes\"

:: Build Mesh ops against Unity 5
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_0;" /t:Clean,Build %u5mesh%

echo ===: Copy mesh ops 5 to staging
xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5\bin\Debug\ProBuilderMeshOps-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Classes\"

:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;UNITY_5;UNITY_5_0;" /v:q /t:Clean,Build %u5editor%

echo ===: Copy editor lib to staging
xcopy "%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5\bin\Debug\ProBuilderEditor-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic\Editor\"

echo ===: EXPORT UNITY 5 PACK

echo ===: Override DLL GUIDs
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

:: Export release pack for Unity 5.0 +
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\prototype5.0-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilderBasic outSuffix:-unity5

echo ===: Done building Unity 4, Unity 5 project packages.

pause
