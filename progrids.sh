#!/bin/sh

/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -projectPath $PWD/progrids2 -executeMethod AutomatedExport.ExportRelease installDir:../../bin/Debug/
echo Dumped ProGrds to bin/Debug