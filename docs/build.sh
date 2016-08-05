#!/bin/bash  

# pdf build
# pandoc --toc -f markdown+grid_tables+table_captions -o manual.pdf manual.pd -V geometry:margin=1in

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

mkdocs build -c

find site/ -name '*.psd' -exec rm {} \;

# find site/ -name '*.html' -exec wkhtmltopdf {} {}.html \;

SITE=../../site/probuilder
MKBUILD=$(pwd)

cd $SITE
git pull origin gh-pages
git rm -r ./*
echo $MKBUILD/site/
cp -r $MKBUILD/site/* .
git add -A

git commit -m "Update documentation"
git push origin gh-pages

echo did the thing
