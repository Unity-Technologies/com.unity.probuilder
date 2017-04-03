#!/bin/bash

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

mkdocs build -c

find site/ -name '*.psd' -exec rm {} \;

SITE=../../site/probuilder2
MKBUILD=$(pwd)

cd $SITE
git pull origin gh-pages
git rm -r ./*
echo $MKBUILD/site/
cp -r $MKBUILD/site/* .
git add -A

# git commit -m "Update documentation"
# git push origin gh-pages

echo did the thing
