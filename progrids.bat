set unity_path="C:\Program Files (x86)\Unity 3.5.7\Editor\Unity.exe"

svn update

echo This assumes you have .NET 3.5 installed (Unity doesn't support 4 yet)

%unity_path% -quit -batchMode -projectPath %CD%\progrids2 -executeMethod AutomatedExport.ExportRelease installDir:../../bin/Debug/

echo Finish!
pause
