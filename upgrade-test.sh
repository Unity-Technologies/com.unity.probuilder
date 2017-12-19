#!/bin/bash

rm -rf tests/upgrade/UnityPackageManager/com.unity.probuilder/
git clean -df tests/upgrade
git checkout tests
mono pb-build.exe upm.json -d
mkdir tests/upgrade/UnityPackageManager/
cp -r ../upm-package-probuilder-project/UnityPackageManager/com.unity.probuilder/ tests/upgrade/UnityPackageManager/com.unity.probuilder/
