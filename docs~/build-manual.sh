#!/bin/bash

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md

mkdocs build -c -f mkdocs-manual.yml

rm -f manual.pdf

python manual.py

cp manual.pdf ../probuilder2.0/Assets/ProCore/ProBuilder/ProBuilderManual.pdf
