#!bin/bash

# UNITY_PATH=`/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity`
# MDTOOL_PATH=`/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool`

# svn up
svn update

echo "> > Cleaning out old temp folder"
rm -rf bin/temp
mkdir bin/temp

# Export release resources
echo "> > Exporting release resources"
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -projectPath $PWD/probuilder2.0 -executeMethod AutomatedExport.ExportReleaseResources 

rm -rf probuilder-staging

# create staging project
echo "> > Create staging project"
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -createProject probuilder-staging

# import resources to staging project
# echo $PWD/bin/temp/ProBuilder2/(Resources/).unitypackage
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -projectPath $PWD/probuilder-staging -importPackage $PWD/bin/temp/ProBuilder2\(Resources\).unitypackage

/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool build -c:Release visual\ studio/ProBuilderCore/ProBuilderCore.sln

/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool build -c:Release visual\ studio/ProBuilderEditor/ProBuilderEditor.sln

echo "> > Copy core dll to staging project"
cp bin/DLL/ProBuilderCore.dll probuilder-staging/Assets/6by7/ProBuilder/Classes/ProBuilderCore.dll

echo "> > Copy editor core dll to staging project"
cp bin/DLL/ProBuilderEditor.dll probuilder-staging/Assets/6by7/ProBuilder/Editor/ProBuilderEditor.dll

echo "> > Building package"

# if building w.out install scripts
# /Applications/Unity\ 3.5.6/Unity.app/Contents/MacOS/unity -quit -batchMode -projectPath $PWD/probuilder-staging -executeMethod AutomatedExport.ExportRelease -installDir:../../bin/Debug/

# if build with install scripts
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -projectPath $PWD/probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:../../bin/temp/

## EXIT HERE IF NOT BUILDING INSTALL SCRIPT

# Clear out staging project again, but this time repopulate it with a built pack + install stuff
rm -rf probuilder-staging

echo "> > Populate staging with install scripts"
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -createProject $PWD/probuilder-staging

mkdir $PWD/probuilder-staging/Assets/6by7
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Classes
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Editor
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Classes/Debug
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Editor/Debug
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install/Editor/
mkdir $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install/Packs/

# Copy AutomatedExport script into project
echo "> > Copy final build packs into staging project"
cp -r $PWD/probuilder2.0/Assets/6by7/ProBuilder/Editor/Debug/AutomatedExport.cs $PWD/probuilder-staging/Assets/6by7/ProBuilder/Editor/Debug/
cp -r $PWD/probuilder2.0/Assets/6by7/ProBuilder/Editor/Debug/SvnManager.cs $PWD/probuilder-staging/Assets/6by7/ProBuilder/Classes/Debug/

# Bring in upgrade packages and QuickStart script
cp -r $PWD/bin/temp/ProBuilder2-v*.unitypackage $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install/Packs/
# cp -r $PWD/bin/temp/ProBuilder2-upgrade.unitypackage $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install/Packs/

cp -r $PWD/probuilder2.0/Assets/6by7/ProBuilder/Install/Editor/QuickStart.cs $PWD/probuilder-staging/Assets/6by7/ProBuilder/Install/Editor/

echo "> > Building final package to bin/Debug"
/Applications/Unity\ 3.5.7/Unity.app/Contents/MacOS/Unity -quit -batchMode -projectPath $PWD/probuilder-staging -executeMethod AutomatedExport.ExportRelease installDir:../../bin/Debug/ generateAbout:FALSE
