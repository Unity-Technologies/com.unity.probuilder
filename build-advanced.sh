#!/bin/bash

ASSETS_DIR="probuilder2.0/Assets"

# Assets/ProCore/ProBuilder
PB_DIR="$ASSETS_DIR/ProCore/ProBuilder"

# ProBuilder core lib
CORE_SRC="$PB_DIR/Classes/ClassesCore"

# ProBuilder mesh operations lib
MESH_SRC="$PB_DIR/Classes/ClassesEditing"

# Unity 5.5 directory
UNITY_5_5=D:/Applications/Unity\ 5.5.0f3

echo Compile ProBuilder Core
mcs -recurse:"$CORE_SRC/*.cs" -t:library -sdk:2 -out:ProBuilderCore.dll -lib:"$UNITY_5_5"/Editor/Data/Managed -r:UnityEngine.dll -debug

echo Compile ProBuilder Mesh Operations
mcs -recurse:"$MESH_SRC/*.cs" -t:library -sdk:2 -out:ProBuilderMeshOps.dll -lib:"$UNITY_5_5"/Editor/Data/Managed -lib:"$PB_DIR"/Classes/ClassesLib -r:UnityEngine.dll -r:ProBuilderCore.dll -r:KDTree.dll -r:pb_Stl.dll -r:Triangle.dll -debug

BUILD_DIR=~/Desktop/probuilder

if [ -d $BUILD_DIR ]; then rm -rf $BUILD_DIR; fi

# mkdir $BUILD_DIR
# mkdir $BUILD_DIR/Assets
# 
# cp -r $ASSETS_DIR/ProCore $BUILD_DIR/Assets/ProCore
# rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesCore
# rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesEditing

cp ProBuilderCore.dll $ASSETS_DIR/ProCore/ProBuilder/Classes/ProBuilderCore.dll
cp ProBuilderCore.dll.mdb $ASSETS_DIR/ProCore/ProBuilder/Classes/ProBuilderCore.dll.mdb

cp ProBuilderMeshOps.dll $ASSETS_DIR/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll
cp ProBuilderMeshOps.dll.mdb $ASSETS_DIR/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll.mdb
