#!/bin/bash

if [[ -z $(git status -s) ]]
then
echo Build 5.0
	mono pb-build.exe build/targets/ProBuilderBasic-5.0.json
echo Build 5.3
	mono pb-build.exe build/targets/ProBuilderBasic-5.3.json
echo Build 5.5
	mono pb-build.exe build/targets/ProBuilderBasic-5.5.json
echo Build 5.6
	mono pb-build.exe build/targets/ProBuilderBasic-5.6.json
else
	echo "Uncommitted changes in git, bailing out."
	echo "Please commit or stash your changes before building."
	exit 1
fi

echo finished $(date)

exit 0
