var doc = activeDocument;
var thePath = activeDocument.path;
var layerName = app.activeDocument.activeLayer.name;

//alert(thePath + "/" + layerName + ".png");

var theFile = new File (thePath + "/" + layerName + ".png");

//
docExportOptions = new ExportOptionsSaveForWeb; 
docExportOptions.format = SaveDocumentType.PNG;
docExportOptions.transparency = true; 
docExportOptions.blur = 0.0; 
docExportOptions.interlaced = false;
docExportOptions.quality = 100; 
docExportOptions.PNG8 = false; 
//

activeDocument.exportDocument (theFile,ExportType.SAVEFORWEB,docExportOptions);
