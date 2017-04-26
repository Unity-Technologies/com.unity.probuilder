#!/bin/bash

ASSETS_DIR="probuilder2.0/Assets"

# Assets/ProCore/ProBuilder
PB_DIR="$ASSETS_DIR/ProCore/ProBuilder"

# ProBuilder core lib
CORE_SRC="$PB_DIR/Classes/ClassesCore"

# ProBuilder mesh operations lib
MESH_SRC="$PB_DIR/Classes/ClassesEditing"

# ProBuilder editor lib
EDITOR_SRC="$PB_DIR/Editor/EditorCore"

# Unity 5.5 directory
UNITY_5_5=D:/Applications/Unity\ 5.5.0f3

# Unity 5.5 assemblies directory
UNITY_5_5_LIB="$UNITY_5_5"/Editor/Data/Managed

echo Compile ProBuilder Core
mcs -recurse:"$CORE_SRC/*.cs" -t:library -sdk:2 -out:ProBuilderCore.dll -lib:"$UNITY_5_5_LIB" -r:UnityEngine.dll -debug

echo Compile ProBuilder Mesh Operations
mcs -recurse:"$MESH_SRC/*.cs" -t:library -sdk:2 -out:ProBuilderMeshOps.dll -lib:"$UNITY_5_5_LIB" -lib:"$PB_DIR"/Classes/ClassesLib -r:UnityEngine.dll -r:ProBuilderCore.dll -r:KDTree.dll -r:pb_Stl.dll -r:Triangle.dll -debug

echo Compile ProBuilder Editor
mcs -recurse:"$EDITOR_SRC/*.cs" -t:library -sdk:2 -out:ProBuilderEditor.dll -lib:"$UNITY_5_5_LIB"  -lib:"$PB_DIR"/Classes/ClassesLib -r:UnityEngine.dll -r:UnityEditor.dll -r:ProBuilderCore.dll -r:ProBuilderMeshOps.dll -r:pb_Stl.dll -debug

BUILD_DIR=~/Desktop/probuilder

if [ -d $BUILD_DIR ]; then rm -rf $BUILD_DIR; fi

mkdir $BUILD_DIR
mkdir $BUILD_DIR/Assets
 
cp -r $ASSETS_DIR/ProCore $BUILD_DIR/Assets/ProCore
rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesCore
rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesEditing
rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/EditorCore

echo $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderCore.dll

cp ProBuilderCore.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderCore.dll
cp ProBuilderCore.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderCore.dll.mdb

cp ProBuilderMeshOps.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll
cp ProBuilderMeshOps.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll.mdb

cp ProBuilderEditor.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/ProBuilderEditor.dll
cp ProBuilderEditor.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/ProBuilderEditor.dll.mdb
