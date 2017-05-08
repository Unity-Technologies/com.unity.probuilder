#!/bin/bash

if [[ -z $(git status -s) ]]
then
	mono pb-build.exe build/targets/ProBuilderAdvanced-5.5.json -debug
else
	echo "Uncommitted changes in git, bailing out."
	echo "Please commit or stash your changes before building."
	exit 1
fi
