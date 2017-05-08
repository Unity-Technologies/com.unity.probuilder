#!/bin/bash

if [[ -z $(git status -s) ]]
then
	mono pb-build.exe build/targets/ProBuilderAdvanced-5.5.json -debug
else
	echo "uncommitted changes in git, bailing out"
	exit 1
fi
