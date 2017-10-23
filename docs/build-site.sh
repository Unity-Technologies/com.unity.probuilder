#!/bin/bash

set -e

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

mkdocs build -c

find site/ -name '*.psd' -exec rm {} \;

# generate the current.txt file
mono gen_cur_txt.exe -f docs/changelog.md > site/current.txt

SITE=../../site/probuilder2
MKBUILD=$(pwd)

cd $SITE

git reset
git checkout .
git clean -df
git pull origin gh-pages
git rm -r ./*
echo $MKBUILD/site/
cp -r $MKBUILD/site/* .
git add -A

# git commit -m "Update documentation"
# git push origin gh-pages

echo Site built: $MKBUILD
echo "To push changes to website, run git commit & git push origin gh-pages"
