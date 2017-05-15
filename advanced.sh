#!/bin/bash

if [[ -z $(git status -s) ]]
then
echo Build 4.7
	mono pb-build.exe build/targets/ProBuilderAdvanced-4.7.json -debug
echo Build 5.0
	mono pb-build.exe build/targets/ProBuilderAdvanced-5.0.json -debug
echo Build 5.3
	mono pb-build.exe build/targets/ProBuilderAdvanced-5.3.json -debug
echo Build 5.6
	mono pb-build.exe build/targets/ProBuilderAdvanced-5.6.json -debug
else
	echo "Uncommitted changes in git, bailing out."
	echo "Please commit or stash your changes before building."
	exit 1
fi

echo Finished building ProBuilder

exit 0
