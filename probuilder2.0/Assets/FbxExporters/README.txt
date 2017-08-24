# FbxExporters

Copyright (c) 2017 Unity Technologies. All rights reserved.

See LICENSE.txt file for full license information.

**Version**: 0.0.10a

Requirements
------------

* [FBX SDK C# Bindings](https://github.com/Unity-Technologies/FbxSharp)

Installing Maya Integration
--------------------------------------------

The easy way to install the Maya integration is with the
        FbxExporters -> Install Maya Integration
menu option in Unity.

It uses the most recent version of Maya it can find.  Set your MAYA_LOCATION
environment before running Unity if you want to specify a particular version of Maya.


Alternately, you can install the package and integrations from the command-line
using the following script:


MacOS:

# Configure where Unity is installed
if [ ! -d "${UNITY3D_PATH}" ]; then
    UNITY3D_PATH="/Applications/Unity/Unity.app"
fi

# Configure which Unity project to install package
if [ ! -d "${PROJECT_PATH}" ]; then
    PROJECT_PATH=~/Development/FbxExporters
fi

# Configure where unitypackage is located
if [ ! -f "${PACKAGE_PATH}" ]; then
    PACKAGE_PATH=`ls -t ${PROJECT_PATH}/FbxExporters_*.unitypackage | head -1`
fi

# Configuring Maya2017 to auto-load integration
if [ ! -d "${MAYA_LOCATION}" ] ; then
    MAYA_LOCATION=/Applications/Autodesk/maya2017/Maya.app
fi
export MAYA_LOCATION

if [ ! -d "${UNITY3D_PATH}" ]; then
    echo "Unity is not installed"
elif [ ! -d "${MAYA_LOCATION}" ] ; then
    echo "Maya is not installed"
else
    # Install FbxExporters package
    "${UNITY3D_PATH}/Contents/MacOS/Unity" -projectPath "${PROJECT_PATH}" -importPackage ${PACKAGE_PATH} -quit

    # Install Maya Integration. Requires MAYA_LOCATION.
    "${UNITY3D_PATH}/Contents/MacOS/Unity" -batchMode -projectPath "${PROJECT_PATH}" -executeMethod FbxExporters.Integrations.InstallMaya -quit

    # To configure without user interface change the last argument to 1 instead of 0
    "${MAYA_LOCATION}/Contents/MacOS/Maya" -command "configureUnityOneClick \"${PROJECT_PATH}\" \"${UNITY3D_PATH}\" 0; scriptJob -idleEvent quit;"
fi

