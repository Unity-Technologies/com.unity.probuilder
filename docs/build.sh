#!/bin/bash  

# pdf build
# pandoc --toc -f markdown+grid_tables+table_captions -o manual.pdf manual.pd -V geometry:margin=1in

mkdocs build -c

SITE=../../site/probuilder
MKBUILD=$(pwd)

cd $SITE
git rm -r ./*
echo $MKBUILD/site/
cp -r $MKBUILD/site/* .
git add -A

git commit -m "Update documentation"
git push origin gh-pages

echo did the thing
