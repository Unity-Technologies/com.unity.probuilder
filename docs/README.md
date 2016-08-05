# Working with Documentation

## Dependencies

- [MkDocs](http://www.mkdocs.org/)
- [wkhtmltopdf](http://wkhtmltopdf.org/)

`wkhtmltopdf` must be present in your system path.  

On Windows:

- Open Control Panel, type "Path" in the search bar
- Select "Edit the system environment variables"
- Click the "Environment Variables..." button
- Select "PATH", then "Edit..."
- Add the path to wkhtmltopdf bin folder.  Ex: `C:\Program Files\wkhtmltopdf\bin\`

## Previewing the docs

- Open `Command Prompt`
- Enter the following commands in order:
	- `D:`
	- `cd probuilder2/docs`
	- `mkdocs serve`
- Open the "ProBuilder Documentation.url" link, or open a web browser and enter the address `http://127.0.0.1:8000/`

Any changes made to the docs will now be instantly previewed in your web browser.

http://www.mkdocs.org/#getting-started

## Editing a Page

The ProBuilder docs are written in [markdown](https://daringfireball.net/projects/markdown/syntax) format.

## Adding a page

See http://www.mkdocs.org/#adding-pages
