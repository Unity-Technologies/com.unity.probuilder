# Working with Documentation

## Dependencies

- [MkDocs](http://www.mkdocs.org/)
- [wkhtmltopdf](http://wkhtmltopdf.org/)
- [pdfkit](https://pypi.python.org/pypi/pdfkit)
- [Cinder](https://github.com/procore3d/cinder)

## Previewing the docs

- Run `mkdocs-serve.bat`

Or

- Open `Command Prompt`
- `cd probuilder2/docs`
- `mkdocs serve`

Any changes made to the docs will now be instantly previewed in your web browser.

## Editing a Page

The ProBuilder docs are written in [markdown](https://daringfireball.net/projects/markdown/syntax) format.

## Adding a page

See http://www.mkdocs.org/#adding-pages

# Building the Manual

- Check out ProCore3D fork of Cinder theme
- Create a symlink to `cinder/cinder` folder in the `docs/` directory (or copy the folder if you don't plan on making changes)
	- (`cd probuilder2/docs; ln -s ../../cinder/cinder ./cinder`)
- `sh build-manual.sh`
