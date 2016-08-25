#!/bin/bash

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

mkdocs build -c -f mkdocs-manual.yml

if [ -d pdfs ]; then
	rm -rf pdfs
fi

mkdir pdfs

python manual.py

cp pdfs/manual.pdf ../probuilder2.0/Assets/ProCore/ProBuilder/ProBuilderManual.pdf
