#!/bin/bash
# Exports Unity packages.

if [ "$(uname)" == "Darwin" ]; then
WORKING_DIR=$(pwd)
UNITY_47=/Applications/Unity\ 4.6.1f1/Unity.app/Contents/MacOS/Unity
UNITY_50=/Applications/Unity\ 5.0.0f4/Unity.app/Contents/MacOS/Unity
UNITY_53=/Applications/Unity\ 5.3.0f4/Unity.app/Contents/MacOS/Unity
UNITY_55=/Applications/Unity\ 5.5.0f3/Unity.app/Contents/MacOS/Unity
UNITY_56=/Applications/Unity\ 5.6.0f3/Unity.app/Contents/MacOS/Unity
else
# cygwin paths don't cut it in -projectPath
WORKING_DIR=$(cygpath -aw $(pwd))
UNITY_47=/d/Applications/Unity\ 4.7.0f1/Editor/Unity.exe
UNITY_50=/d/Applications/Unity\ 5.0.0f4/Editor/Unity.exe
UNITY_53=/d/Applications/Unity\ 5.3.0f4/Editor/Unity.exe
UNITY_55=/d/Applications/Unity\ 5.5.0f3/Editor/Unity.exe
UNITY_56=/d/Applications/Unity\ 5.6.0f3/Editor/Unity.exe
fi

PROBUILDER_VERSION=2.9.0b0

rm -rf bin/packages
mkdir bin/packages

rm -rf bin/logs
mkdir bin/logs

# /d/Applications/Unity\ 5.6.0f3/Editor/Unity.exe -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

UNITY_VERSION=47
echo Export Unity $UNITY_VERSION

"$UNITY_47" -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

UNITY_VERSION=50
echo Export Unity $UNITY_VERSION

"$UNITY_50" -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

UNITY_VERSION=53
echo Export Unity $UNITY_VERSION

"$UNITY_53" -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

UNITY_VERSION=55
echo Export Unity $UNITY_VERSION

"$UNITY_55" -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

UNITY_VERSION=56
echo Export Unity $UNITY_VERSION

"$UNITY_56" -projectPath $WORKING_DIR/bin/temp/ProBuilder-Unity$UNITY_VERSION -batchmode -quit -nographics -exportPackage Assets/ProCore ../../packages/ProBuilder2-v$PROBUILDER_VERSION-unity$UNITY_VERSION.unitypackage -logFile bin/logs/log_$UNITY_VERSION.txt -disable-assembly-updater

echo Finished
