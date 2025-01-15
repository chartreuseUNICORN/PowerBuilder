#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilderUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PowerBuilderUI.Forms;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSelectiveTransfer : IExternalCommand
    {
        public static string DisplayName { get; } = "Selective Transfer";
        public static string ShortDescr { get; } = "Select Element Types to copy from the source document to the target document";
        public static bool LoadCommandFlag = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            // In the other one, i called the active doc_tar and the others doc_src
            Document docTarget = uidoc.Document;

            // Retrieve elements from database
            // display elements in src not in tar
            // this has to occur inside the form? how did i do this in the python version..

            HashSet<Document> openTargets = new HashSet<Document>();
            //Probably a tidier way to do this.
            foreach (Document openDoc in app.Documents)
            {
                if (!openDoc.Equals(docTarget)) {
                    openTargets.Add(openDoc);
                }
            }

            frmSelectiveTransfer SelectiveTransferForm = new frmSelectiveTransfer();
            //need to fix the types on for this interaction.
            SelectiveTransferForm.AddItemsToCBox(openTargets.ToList<Document>());
            PBDialogResult res =  SelectiveTransferForm.ShowDialogWithResult();

            //TODO: add handling for emtpy selection
            if (res.IsAccepted) {
                Debug.WriteLine("form submitted");
                Document docSource = (Document)res.SelectionResults[0];
                List<ElementId> selectedIds = res.SelectionResults[1] as List<ElementId>;
                SelectiveTransfer(selectedIds, docSource, docTarget);
            }

            return Result.Succeeded;
        }
        public bool SelectiveTransfer(ICollection<ElementId> lSelectedTypes, Document src, Document tar) {

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

            return true;
        }
    }

}
