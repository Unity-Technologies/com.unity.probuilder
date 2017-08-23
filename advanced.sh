#!/bin/bash

if [[ -z $(git status -s) ]] || [[ $@ == "-f" ]]
then
	mono pb-build.exe build/targets/ProBuilderAdvanced-Src.json build/targets/ProBuilderAdvanced-4.7.json build/targets/ProBuilderAdvanced-5.0.json build/targets/ProBuilderAdvanced-5.3.json build/targets/ProBuilderAdvanced-5.5.json build/targets/ProBuilderAdvanced-5.6.json build/targets/ProBuilderAdvanced-2017.2.json
else
	echo "Uncommitted changes in git, bailing out."
	echo "Please commit or stash your changes before building."
	exit 1
fi

echo finished $(date)

exit 0
