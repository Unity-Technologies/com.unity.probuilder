#!/bin/bash

cp ../probuilder2.0/Assets/ProCore/ProBuilder/About/changelog.txt docs/changelog.md
rm -rf docs/css/specialized
mkdir docs/css/specialized
cp -r css/manual/* docs/css/specialized

mkdocs build -c -f mkdocs-manual.yml

if [ -d pdfs ]; then
	rm -rf pdfs
fi

mkdir pdfs

python manual.py
