#!/bin/bash

mono pb-build.exe ProBuilderAdvanced-5.5.json -debug

ASSETS_DIR="probuilder2.0/Assets"
BUILD_DIR=~/Desktop/probuilder

# if [ -d $BUILD_DIR ]; then rm -rf $BUILD_DIR; fi

# mkdir $BUILD_DIR
# mkdir $BUILD_DIR/Assets
 
# cp -r $ASSETS_DIR/ProCore $BUILD_DIR/Assets/ProCore
rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesCore
rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ClassesEditing
# rm -rf $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/EditorCore

cp ProBuilderCore.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderCore.dll
cp ProBuilderCore.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderCore.dll.mdb

cp ProBuilderMeshOps.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll
cp ProBuilderMeshOps.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Classes/ProBuilderMeshOps.dll.mdb

# cp ProBuilderEditor.dll $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/ProBuilderEditor.dll
# cp ProBuilderEditor.dll.mdb $BUILD_DIR/Assets/ProCore/ProBuilder/Editor/ProBuilderEditor.dll.mdb
