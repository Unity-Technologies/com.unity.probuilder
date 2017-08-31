#!/bin/bash

if [[ -z $(git status -s) ]] || [[ $@ == "-f" ]]
then
	mono pb-build.exe build/targets/ProBuilderBasic-5.3.json build/targets/ProBuilderBasic-5.5.json build/targets/ProBuilderBasic-5.6.json build/targets/ProBuilderBasic-2017.1.json build/targets/ProBuilderBasic-2017.2.json
else
	echo "Uncommitted changes in git, bailing out."
	echo "Please commit or stash your changes before building."
	exit 1
fi

echo finished $(date)

exit 0
