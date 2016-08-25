#!/usr/bin/env python

# pip install pdfkit
# pip install pyyaml

import os
import pdfkit
import yaml

directory = yaml.load( open("mkdocs-manual.yml") )
pages = directory["pages"]
pdf_dir = "pdfs"
site_dir = "site"
pdfs = []

# gather all the docs pages in a list of tuples (category, title, path)
print("finding doc pages in mkdocs-manual.yml")

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

if not os.path.exists(pdf_dir):
    os.makedirs(pdf_dir)

gen_pdfs = []

for page in pdfs:
	if page[1] == "Home" and page[2] == "index.md":
		path = page[2].replace(".md", ".html")
	else:
		split = page[2].split("/")
		path = page[2].replace(".md", "") + "/index.html"

	if len(page[0]) > 0:
		title = page[0] + " - " + page[1] + ".pdf"
	else:
		title = page[1] + ".pdf";

	# print( path + " => " + pdf_dir + "/" + title)

	gen_pdfs.append( site_dir + "/" + path )

pdfkit.from_file(gen_pdfs, pdf_dir + "/manual.pdf", options={
	'load-error-handling': 'ignore',
	'disable-plugins':'' });

