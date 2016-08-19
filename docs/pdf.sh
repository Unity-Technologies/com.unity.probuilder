#!/bin/bash

# cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

# mkdocs build -c -t cinder

# find site/ -name '*.psd' -exec rm {} \;

# find site/ -name 'index.html' -exec echo PROCESSING: {} \; -exec wkhtmltopdf {} {}.pdf \;

rm -rf pdfs
mkdir pdfs
find site/ -name '*.pdf' -exec bash -c 'cp "$0" "pdfs/${0//\//_}"' {} \;
# cp "$1" $2"${1//\//_}"
