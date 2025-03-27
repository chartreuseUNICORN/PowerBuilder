#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PowerBuilderUI.Forms;
using PowerBuilderUI;
using PowerBuilder.Interfaces;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdSelectiveTransfer : IPowerCommand {
        public string DisplayName { get; } = "Selective Transfer";
        public string ShortDesc { get; } = "Select Element Types to copy from the source document to the target document";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            PowerDialogResult res = GetInput(uiapp);
            //TODO: add handling for emtpy selection
            if (res.IsAccepted) {
                Debug.WriteLine("form submitted");
                Document docSource = (Document)res.SelectionResults[0];
                List<ElementId> selectedIds = res.SelectionResults[1] as List<ElementId>;
                SelectiveTransfer(selectedIds, docSource, res.SelectionResults[2] as Document);
            }

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            // In the other one, i called the active doc_tar and the others doc_src
            Document docTarget = uidoc.Document;

            // Retrieve elements from database
            // display elements in src not in tar
            // this has to occur inside the form? how did i do this in the python version..

            HashSet<Document> openTargets = new HashSet<Document>();
            //Probably a tidier way to do this.
            foreach (Document openDoc in app.Documents) {
                if (!openDoc.Equals(docTarget)) {
                    openTargets.Add(openDoc);
                }
            }

            frmSelectiveTransfer SelectiveTransferForm = new frmSelectiveTransfer();
            //need to fix the types on for this interaction.
            SelectiveTransferForm.AddItemsToCBox(openTargets.ToList<Document>());
            PowerDialogResult res = SelectiveTransferForm.ShowDialogWithResult();
            res.SelectionResults.Add(docTarget);

            return res;
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
