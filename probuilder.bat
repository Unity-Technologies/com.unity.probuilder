@echo off

set unity_path_4="D:\Applications\Unity 4.6.8f1\Editor\Unity.exe"
set unity_path_5="D:\Applications\Unity 5.0.0f4\Editor\Unity.exe"
set msbuild="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set build_directory="%CD%\bin\Debug"

:: DLL VS project paths for Unity 4 & 5
:: ====================
set u4core="%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4.sln"
set u4mesh="%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4.sln"
set u4editor="%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4.sln"
set u5core="%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5.sln"
set u5mesh="%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5.sln"
set u5editor="%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5.sln"

echo UNITY 4 PATH IS %unity_path_4%
echo UNITY 5 PATH IS %unity_path_5%

:: Update SVN
:: ====================
svn update

:: clean out temp directory.
:: ====================
echo Clean temp and library

rd /s /q bin\temp\
rd /s /q probuilder2.0\Library
rd /s /q probuilder-staging\

echo Make bin folders

mkdir bin\
mkdir bin\temp
mkdir bin\Debug
mkdir bin\logs

echo Create empty project

:: Create an empty project to stage package exports from
:: ====================
%unity_path_4% -quit -batchMode -createProject %CD%\probuilder-staging

echo Copy resources

:: Copy ProCore folder into staging project
:: ====================
xcopy /E /Y /I /Q %CD%\probuilder2.0\Assets\ProCore\ProBuilder %CD%\probuilder-staging\Assets\ProCore\ProBuilder

:: Copy pb_ExportPackage into staging project
:: ====================
mkdir %CD%\probuilder-staging\Assets\Editor\
xcopy %CD%\probuilder2.0\Assets\Debug\Editor\pb_ExportPackage.cs %CD%\probuilder-staging\Assets\Editor\

:: Export Source
 :: ====================
echo Export source
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder4.6-source-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilder2 outSuffix:-source

:: Build Unity DLLs
:: ====================
echo Build Unity 4 Core
%msbuild% /p:DefineConstants="RELEASE;UNITY_4_6;" /t:Clean,Build %u4core%

echo Build Unity 5 Core
%msbuild% /p:DefineConstants="RELEASE;UNITY_5;UNITY_5_0;" /t:Clean,Build %u5core%

echo Build Unity 4 Mesh Ops
%msbuild% /p:DefineConstants="RELEASE;UNITY_4_6;" /t:Clean,Build %u4mesh%

echo Build Unity 5 Mesh Ops
%msbuild% /p:DefineConstants="RELEASE;UNITY_5;UNITY_5_0;" /t:Clean,Build %u5mesh%

echo Build Unity 4 Editor Core
%msbuild% /p:DefineConstants="RELEASE;UNITY_EDITOR;UNITY_4_6;" /v:q /t:Clean,Build %u4editor%

echo Build Unity 5 Editor Core
%msbuild% /p:DefineConstants="RELEASE;UNITY_EDITOR;UNITY_5;UNITY_5_0;" /v:q /t:Clean,Build %u5editor%

:: Remove scripts from staging project
:: ====================
echo Remove Core, Mesh, and Editor scripts
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesCore
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesEditing
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\EditorCore

:: Copy Unity 4 build artifacts
:: ====================
echo Copy Unity 4 build artifacts
xcopy "%CD%\visual studio\ProBuilderCore-Unity4\ProBuilderCore-Unity4\bin\Debug\ProBuilderCore-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity4\ProBuilderMeshOps-Unity4\bin\Debug\ProBuilderMeshOps-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
xcopy "%CD%\visual studio\ProBuilderEditor-Unity4\ProBuilderEditor-Unity4\bin\Debug\ProBuilderEditor-Unity4.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

echo Override DLL GUIDs
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder4-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

:: Export Unity 4
:: ====================
echo Export Unity 4 DLL project
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder4.6-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilder2 outSuffix:-unity4


:: Remove Unity 4 DLL from staging, and rebuild with 5 libs
:: ====================
echo Remove Core, Mesh, and Editor DLL
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderCore-Unity4.dll"
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderMeshOps-Unity4.dll"
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\ProBuilderEditor-Unity4.dll"

echo Copy Unity 5 build artifacts
xcopy "%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5\bin\Debug\ProBuilderCore-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5\bin\Debug\ProBuilderMeshOps-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
xcopy "%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5\bin\Debug\ProBuilderEditor-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

echo Override DLL GUIDs
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

:: Export Unity 5
:: ====================
echo Export Unity 5 DLL project
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5.0-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilder2 outSuffix:-unity5

:: Export UpgradeKit
xcopy /E /Y /I /Q %CD%\probuilder2.0\Assets\ProBuilderUpgradeKit %CD%\probuilder-staging\Assets\ProBuilderUpgradeKit
%unity_path_4% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\ProBuilderUpgradeKit-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProBuilderUpgradeKit outDir:%build_directory% outName:ProBuilderUpgradeKit

pause
