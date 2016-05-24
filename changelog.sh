#!/usr/bin/bash

# https://git-scm.com/docs/pretty-formats

LAST_TAG=`git describe --abbrev=0 --tags`

git log --date=short --pretty=format:"%ad %h %s" $LAST_TAG..
