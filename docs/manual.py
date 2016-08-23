#!/usr/bin/env python

# pip install pdfkit
# pip install pyyaml

import os
import pdfkit
import yaml

directory = yaml.load( open("mkdocs.yml") )
pages = directory["pages"]
pdf_dir = "pdfs"
site_dir = "site"
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

	# ignore exceptions because pdfkit (or more accurately, wkhtmltopdf) exits 1 with non-fatal
	# errors
	try:
		gen_pdfs.append( pdfkit.from_file(site_dir + "/" + path, pdf_dir + "/" + title, options={ 'load-error-handling': 'ignore', 'disable-plugins':'' }) )
	except:
		pass

print( str(gen_pdfs) )

# [
# 	('', 'Home', 'index.md'),
# 	('', 'Fundamentals', 'general/fundamentals.md'),
# 	('Toolbar', 'Overview', 'toolbar/overview-toolbar.md'),
# 	('Toolbar', 'Tool Panels', 'toolbar/tool-panels.md'),
# 	('Toolbar', 'Selection Actions', 'toolbar/selection-tools.md'),
# 	('Toolbar', 'Object Actions', 'toolbar/object-actions.md'),
# 	('Toolbar', 'Vertex Actions', 'toolbar/vertex.md'),
# 	('Toolbar', 'Edge Actions', 'toolbar/edge.md'),
# 	('Toolbar', 'Face Actions', 'toolbar/face.md'),
# 	('Toolbar', 'Element Actions', 'toolbar/all.md'),
# 	('Texture Mapping', 'Overview', 'texturing/overview-texture-mapping.md'),
# 	('Texture Mapping', 'UV Editor Toolbar', 'texturing/uv-editor-toolbar.md'),
# 	('Texture Mapping', 'Auto UVs Actions', 'texturing/auto-uvs-actions.md'),
# 	('Texture Mapping', 'Manual UVs Actions', 'texturing/manual-uvs-actions.md'),
# 	('', 'Preferences', 'preferences/preferences.md'),
# 	('', 'Troubleshooting', 'troubleshooting/faq.md'),
# 	('Upgrading', 'What Upgrade Procedure Should I Follow?', 'upgrading/overview.md'),
# 	('Upgrading', 'Standard', 'upgrading/standard.md'),
# 	('Upgrading', 'DLL Rename', 'upgrading/dllrename.md'),
# 	('Upgrading', 'Upgrade Kit', 'upgrading/upgrade-kit.md'),
# 	('Upgrading', 'Prototype', 'upgrading/prototype.md'),
# 	('', 'Changelog', 'changelog.md')
# ]
