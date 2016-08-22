#!/usr/bin/env python

# pip install pdfkit
# pip install pyyaml

import pdfkit
import yaml

directory = yaml.load( open("mkdocs.yml") )
pages = directory["pages"]

pdfs = []

# gather all the docs pages in a list of tuples (category, title, path)

for header in pages:
	for key in header:
		if type(header[key]) is str:
			pdfs.append( ("", key, header[key]) )
		elif type(header[key]) is list:
			for item in header[key]:
				if type(item) == str:
					pdfs.append( ("", key, item) )
				else:
					for kvp in item:
						pdfs.append( (key, kvp, item[kvp]) )

print( str(pdfs) )

# pdfkit.from_file("site/index.html", "index.pdf")
