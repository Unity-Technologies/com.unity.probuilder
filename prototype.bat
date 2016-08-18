@echo off

:: Builds for Unity 4.6 & 5.0.1

set unity_path_5="D:\Applications\Unity 5.0.0f4\Editor\Unity.exe"
set unity_path_53="D:\Applications\Unity 5.3.0f4\Editor\Unity.exe"

:: Path to Unity 5 VS projects
set u5_core="%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5.sln"
set u5_mesh="%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5.sln"
set u5_editor="%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5.sln"

:: Path to Unity 5.3 VS projects
:: set u53_core="%CD%\visual studio\ProBuilderCore-Unity53\ProBuilderCore-Unity53.sln"
:: set u53_mesh="%CD%\visual studio\ProBuilderMeshOps-Unity53\ProBuilderMeshOps-Unity53.sln"
set u53_editor="%CD%\visual studio\ProBuilderEditor-Unity5_3\ProBuilderEditor-Unity5_3.sln"

set msbuild="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set build_directory="%CD%\bin\Debug"

set editor_debug="%CD%\probuilder2.0\Assets\ProCore\ProBuilder\Editor\Debug"

echo ===: UNITY 5 PATH IS %unity_path_5%
echo ===: UNITY 5_3 PATH IS %unity_path_53%

:: clean out temp directory.
echo ===: clean out temp directory
rd /s /q bin\temp\
rd /s /q probuilder2.0\Library\
md bin\temp
rd /s /q probuilder-staging\

echo ===: Create staging project
%unity_path_5% -quit -batchMode -createProject %CD%\probuilder-staging

echo ===: Copy export scripts
xcopy /E /Y /I %CD%\probuilder2.0\Assets\Debug\Editor\pb_AddDefine.cs %CD%\probuilder-staging\Assets\Debug\Editor\
xcopy /E /Y /I %CD%\probuilder2.0\Assets\Debug\Editor\pb_ExportPackage.cs %CD%\probuilder-staging\Assets\Debug\Editor\

echo ===: Copy Resources
xcopy /E /Y /I /q %CD%\probuilder2.0\Assets\ProCore %CD%\probuilder-staging\Assets\ProCore

:: do this before removing shit because otherwise unity can't run
echo ===: Prefix files with #define PROTOTYPE
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod pb_AddDefine.PrependDefine define:PROTOTYPE ignore:Debug

pause

echo ===: Remove core, mesh ops, and editor core

rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesCore
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ClassesEditing
rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\EditorCore

rd /s /q %CD%\probuilder-staging\Assets\ProCore\ProGrids
del /Q %CD%\probuilder-staging\Assets\ProCore\ProGrids.meta
del /Q %CD%\probuilder-staging\Assets\ProCore\ProGrids_Documentation*


:: for prototype, remove all kinds of other stuff 
:: @echo on
:: rd /S /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\API Examples"
:: rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Debug
rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Tools
rd /S /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Window
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_ExportObj.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_MakeMeshAsset.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_ProBuilderize.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Actions\pb_StripProBuilderScripts.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_BridgeEdges.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_ConformNormals.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_ConnectEdges.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_FreezeTransform.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_MergeFaces.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_Triangulate.cs
del /Q %CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\MenuItems\Geometry\pb_VertexMergeWeld.cs

:: @echo off

:: ================================ BUILD 4.3 + LIBRARIES ================================ {

echo ===: Build Unity 5.0 DLL


:: Build Core - (post-build script places dll in staging project)
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_0;" /p:Configuration=Release /t:Clean,Build %u5_core%

echo ===: Copy core lib to staging
xcopy "%CD%\visual studio\ProBuilderCore-Unity5\ProBuilderCore-Unity5\bin\Release\ProBuilderCore-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: Build mesh editing classes
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_0;" /p:Configuration=Release /t:Clean,Build %u5_mesh%

echo ===: Copy mesh ops lib to staging
xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity5\ProBuilderMeshOps-Unity5\bin\Release\ProBuilderMeshOps-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;UNITY_5;UNITY_5_0" /p:Configuration=Release /v:q /t:Clean,Build %u5_editor%

echo ===: Copy editor lib to staging
xcopy "%CD%\visual studio\ProBuilderEditor-Unity5\ProBuilderEditor-Unity5\bin\Release\ProBuilderEditor-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

:: echo ===: renaming ProBuilder to ProBuilder Basic
:: move /Y "%CD%\probuilder-staging\Assets\ProCore\ProBuilder" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder Basic"

echo ===: Override DLL GUIDs
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

echo ===: Export release pack for Unity 5.0
%unity_path_5% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\prototype5.0-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilderBasic outSuffix:-unity50

echo ===: Clean Staging

:: del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderCore-Unity5.dll"
:: del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\ProBuilderMeshOps-Unity5.dll"
del /Q "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\ProBuilderEditor-Unity5.dll"

echo ===: Build Unity 5.3 DLL

:: :: Build Core against Unity 5 libs
:: %msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_3;" /t:Clean,Build %u53_core%
:: 
:: echo ===: Copy Core 5.3 to staging
:: xcopy "%CD%\visual studio\ProBuilderCore-Unity53\ProBuilderCore-Unity53\bin\Release\ProBuilderCore-Unity53.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"
:: 
:: :: Build Mesh ops against Unity 5
:: %msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_5;UNITY_5_3;" /t:Clean,Build %u53_mesh%
:: 
:: echo ===: Copy mesh ops 5 to staging
:: xcopy "%CD%\visual studio\ProBuilderMeshOps-Unity53\ProBuilderMeshOps-Unity53\bin\Release\ProBuilderMeshOps-Unity53.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Classes\"

:: /clp:ErrorsOnly  <--- This flag for ErrorsOnly
%msbuild% /p:DefineConstants="RELEASE;PROTOTYPE;UNITY_EDITOR;UNITY_5;UNITY_5_3;" /p:Configuration=Release /v:q /t:Clean,Build %u53_editor%

echo ===: Copy editor lib to staging
xcopy "%CD%\visual studio\ProBuilderEditor-Unity5_3\ProBuilderEditor-Unity5_3\bin\Release\ProBuilderEditor-Unity5.dll" "%CD%\probuilder-staging\Assets\ProCore\ProBuilder\Editor\"

echo ===: Override DLL GUIDs
%unity_path_53% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\probuilder5.3-guid_dll-log.txt -executeMethod pb_ExportPackage.OverrideDLLGUIDs

:: Export release pack for Unity 5.3 +
echo ===: EXPORT UNITY 5.3 PACK
%unity_path_53% -quit -batchMode -projectPath %CD%\probuilder-staging -logFile %CD%\bin\logs\prototype5.3-dll-log.txt -executeMethod pb_ExportPackage.ExportCommandLine sourceDir:ProCore outDir:%build_directory% outName:ProBuilderBasic outSuffix:-unity53

echo ===: Done building Unity 5, Unity 5.3 project packages.

pause
