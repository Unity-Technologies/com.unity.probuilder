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
- Open web browser, enter address: `localhost:8000`

Any changes made to the docs will now be instantly previewed in your web browser.

## Editing a Page

The ProBuilder docs are written in [markdown](https://daringfireball.net/projects/markdown/syntax) format.

## Adding a page

See http://www.mkdocs.org/#adding-pages

# Building the Manual

- Check out ProCore3D fork of Cinder theme
- Create a symlink to `cinder/cinder` folder in the `docs/` directory (or copy the folder if you don't plan on making changes)
	- (`cd probuilder2/docs; ln -s ../../cinder/cinder ./cinder`)
- Install [wk<html>topdf](https://wkhtmltopdf.org/downloads.html)
- Make sure `wkhtmltopdf` is on your `$PATH` (symlink won't cut it with cygwin, add wkhtmltopdf/bin to env path).
- Install `pdfkit` python module. (`pip install pdfkit`)
- `sh build-manual.sh`

---

# Artist-Friendly Instructions
> just for GW...

## Just the Basics (Previewing and Editing with MKDocs)

1. Install [Python](https://www.python.org/)
	> make SURE "Add Python 3.6 to PATH" is enabled when installing!
2. Open Command Prompt, run "pip install --upgrade pip"
3. Still in Command Prompt, run "pip install mkdocs"

## Publishing Changes
> danger zone!

1. stuff


