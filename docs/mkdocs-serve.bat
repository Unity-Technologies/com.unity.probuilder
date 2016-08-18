xcopy /Y /Q ..\probuilder2.0\Assets\ProCore\ProBuilder\About\changelog.txt %CD%\docs\changelog.md

start http://127.0.0.1:8000

mkdocs serve
