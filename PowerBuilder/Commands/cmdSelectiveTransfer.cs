#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSelectiveTransfer : IExternalCommand
    {
        public static string DisplayName { get; } = "Selective Transfer";
        public static string ShortDescr { get; } = "Select Element Types to copy from the source document to the target document";
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            // In the other one, i called the active doc_tar and the others doc_src
            Document doc_src = uidoc.Document;

            // Retrieve elements from database

            DocumentSet openDocs = app.Documents;
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            

            return Result.Succeeded;
        }
        public void SelectiveTransfer(ICollection<ElementId> lSelectedTypes, Document src, Document tar) {

            CopyPasteOptions cpOptions = new CopyPasteOptions();
            
            // Modify document within a transaction
            using (Transaction tx = new Transaction(tar))
            {
                tx.Start("selective-transfer");
                try
                {
                    ElementTransformUtils.CopyElements(src, lSelectedTypes, tar, null, cpOptions);
                    tx.Commit();
                }
                catch {
                    tx.Dispose();
                }
                
            }

            throw new NotImplementedException("implement this method");
        }
    }

}
