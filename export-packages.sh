#!/bin/bash
# Exports Unity packages.
# Must be run from probuilder2/ root directory.
# If no arguments are passed all targets are built.
# Valid arguments:
# 	SRC = Build source code package
# 	4.7 = Build Unity 4.7 package (deprecated)
# 	5.0 = Build Unity 5.0 package (deprecated)
# 	5.3 = Build Unity 5.3 package
# 	5.5 = Build Unity 5.5 package
# 	5.6 = Build Unity 5.6 package
# 	2017.1 = Build Unity 2017.1 package
# 	2017.2 = Build Unity 2017.2 package
# 	2017.3 = Build Unity 2017.3 package
# 	trunk = Build Unity trunk package

ARGS=("$@")
ARGC=${#ARGS[@]}

# @todo accept launch arg to set this
UNITY_INSTALL_DIR="C:/Users/karlh/unity-installs"

if [ "$(uname)" == "Darwin" ]; then
WORKING_DIR=$(pwd)
UNITY_53=/Applications/Unity\ 5.3/Unity.app/Contents/MacOS/Unity
UNITY_55=/Applications/Unity\ 5.5/Unity.app/Contents/MacOS/Unity
UNITY_56=/Applications/Unity\ 5.6/Unity.app/Contents/MacOS/Unity
UNITY_2017_1=/Applications/Unity\ 2017.1/Unity.app/Contents/MacOS/Unity
UNITY_2017_2=/Applications/Unity\ 2017.2/Unity.app/Contents/MacOS/Unity
UNITY_2017_3=/Applications/Unity\ 2017.3/Unity.app/Contents/MacOS/Unity
TRUNK=~/unity/unity/build/MacEditor/Unity.app/Contents/MacOS/Unity
else
# cygwin paths don't cut it in -projectPath
WORKING_DIR=$(cygpath -aw $(pwd))
UNITY_53=$UNITY_INSTALL_DIR/Unity\ 5.3/Editor/Unity.exe
UNITY_55=$UNITY_INSTALL_DIR/Unity\ 5.5/Editor/Unity.exe
UNITY_56=$UNITY_INSTALL_DIR/Unity\ 5.6/Editor/Unity.exe
UNITY_2017_1=$UNITY_INSTALL_DIR/Unity\ 2017.1/Editor/Unity.exe
UNITY_2017_2=$UNITY_INSTALL_DIR/Unity\ 2017.2/Editor/Unity.exe
UNITY_2017_3=$UNITY_INSTALL_DIR/Unity\ 2017.3/Editor/Unity.exe
TRUNK=~/unity/unity/build/WindowsEditor/Unity.exe
fi

UNITY_TARGET_MACRO=(UNITY_53 UNITY_53 UNITY_55 UNITY_56 UNITY_2017_1 UNITY_2017_2 UNITY_2017_3 TRUNK)
UNITY_TARGET_SUFFIX=(SRC 5.3 5.5 5.6 2017.1 2017.2 2017.3 trunk)
UNITY_TARGETS_COUNT=${#UNITY_TARGET_MACRO[@]}

# VERSION_LINE=$(grep version: $WORKING_DIR/probuilder2.0/Assets/ProCore/ProBuilder/About/pc_AboutEntry_ProBuilder.txt)
# PROBUILDER_VERSION=${VERSION_LINE/"version: "/""}
PROBUILDER_VERSION=trunk
echo "Exporting package version: " $PROBUILDER_VERSION

rm -rf bin/packages
mkdir bin/packages

rm -rf bin/logs
mkdir bin/logs

# If arguments were passed only build the requested versions
if [ $ARGC -ne 0 ]; then
	for((i = 0; i < $ARGC; i++)); do
		# Suffix argument must match a folder in the bin/projects directory
		ARG=${ARGS[${i}]}
		for((n=0; n<$UNITY_TARGETS_COUNT; n++)); do
			SUFFIX=${UNITY_TARGET_SUFFIX[${n}]}
			if [ "$ARG" == "$SUFFIX" ]; then
				PROJECT_PATH=$WORKING_DIR/bin/projects/ProBuilder-$SUFFIX
				if [ -d $PROJECT_PATH ]; then
					echo "Building package: ProBuilder2-v"$PROBUILDER_VERSION"-"${SUFFIX}
					TARGET=${UNITY_TARGET_MACRO[${n}]}
					"${!TARGET}" -projectPath $PROJECT_PATH -batchmode -quit -nographics -exportPackage Assets/ProBuilder ../../packages/ProBuilder2-v$PROBUILDER_VERSION-$SUFFIX.unitypackage -logFile bin/logs/log_$SUFFIX.txt -disable-assembly-updater
				else
					echo "Cannot build package ProBuilder2-v"$PROBUILDER_VERSION"-"${SUFFIX}" because project does not exist. Run pb-build to generate this folder."
				fi
			fi
		done
	done
else
	# Otherwise build everything
	for((i=0; i<$UNITY_TARGETS_COUNT;i++)); do
		SUFFIX=${UNITY_TARGET_SUFFIX[${i}]}
		PROJECT_PATH=$WORKING_DIR/bin/projects/ProBuilder-$SUFFIX
		if [ -d $PROJECT_PATH ]; then
			echo "Building package: ProBuilder2-v"$PROBUILDER_VERSION"-"${SUFFIX}
			TARGET=${UNITY_TARGET_MACRO[${i}]}
			"${!TARGET}" -projectPath $PROJECT_PATH -batchmode -quit -nographics -exportPackage Assets/ProBuilder ../../packages/ProBuilder2-v$PROBUILDER_VERSION-$SUFFIX.unitypackage -logFile bin/logs/log_$SUFFIX.txt -disable-assembly-updater
		else
			echo "Cannot build package ProBuilder2-v"$PROBUILDER_VERSION"-"${SUFFIX}" because project does not exist. Run pb-build to generate this folder."
		fi
	done
fi

echo Finished

exit 0