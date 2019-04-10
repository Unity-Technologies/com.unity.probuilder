var doc = activeDocument;
var thePath = activeDocument.path;
var layerName = app.activeDocument.activeLayer.name;

//alert(thePath + "/" + layerName + ".png");

var theFile = new File (thePath + "/" + layerName + ".jpg");

//
docExportOptions = new ExportOptionsSaveForWeb; 
docExportOptions.format = SaveDocumentType.JPEG;
docExportOptions.quality = 60; 

//

activeDocument.exportDocument (theFile,ExportType.SAVEFORWEB,docExportOptions);
