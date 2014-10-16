set unity_path="C:\Program Files\Unity 3.5.7\Editor\Unity 3.5.7.exe"

echo This assumes you have .NET 3.5.7 installed (Unity doesn't support 4 yet)

:: Create resources pack (ExportReleaseResources dumps the pack in bin/temp)
%unity_path% -quit -batchMode -projectPath %CD%\probuilder2.0 -executeMethod AutomatedExport.ExportReleaseResources 

:: No need for upgrade stuff in FREE builds
:: %unity_path% -quit -batchMode -projectPath %CD%/probuilder2.0 -executeMethod AutomatedExport.ExportUpgradePackage

:: md %CD%\probuilder-staging\Assets
rd /S /Q %CD%\probuilder-staging

%unity_path% -quit -batchMode -createProject %CD%\probuilder-staging

md %CD%\probuilder-staging\Assets\6by7\ProBuilder\Editor
md %CD%\probuilder-staging\Assets\6by7\ProBuilder\Classes

:: Import ProBuilder resources
%unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -importPackage %CD%\bin\temp\ProBuilder2(Resources).unitypackage

:: Build Core - (post-build script places dll in staging project)
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;FREE" /t:Clean,Build "%CD%\visual studio\ProBuilderCore\ProBuilderCore.sln"

:: Build Editor Core - (post-build script places dll in staging project)
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:DefineConstants="RELEASE;UNITY_EDITOR;FREE" /t:Clean,Build "%CD%\visual studio\ProBuilderEditor\ProBuilderEditor.sln"

:: Export pack
%unity_path% -quit -batchMode -projectPath %CD%\probuilder-staging -executeMethod AutomatedExport.ExportRelease packName:ProBuilder2(free) ignore:Actions installDir:..\..\bin\Debug\

pause