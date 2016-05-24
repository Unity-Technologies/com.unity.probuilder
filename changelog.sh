#!/usr/bin/bash

LAST_TAG=`git describe --abbrev=0 --tags`

git log --pretty=format:"%h %s" $LAST_TAG..
