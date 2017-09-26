
//VARIABLES

var documentPath = activeDocument.path;
var documentShortName = activeDocument.name;
var newShortName = (documentShortName.replace(".png", "") + "_Horizontal.png");
var newFullName = (documentPath + "/" + newShortName);
var theNewFile = new File (newFullName);

//EXPORT SETTINGS
docExportOptions = new ExportOptionsSaveForWeb; 
docExportOptions.format = SaveDocumentType.PNG;
docExportOptions.transparency = true; 
docExportOptions.blur = 0.0; 
docExportOptions.interlaced = false;
docExportOptions.quality = 100; 
docExportOptions.PNG8 = false; 
//

//DO STUFF
activeDocument.rotateCanvas(90);
activeDocument.exportDocument (theNewFile,ExportType.SAVEFORWEB,docExportOptions);
