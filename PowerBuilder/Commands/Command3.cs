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
using System.Windows.Forms;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Command3 : IExternalCommand
    {
        public static string DisplayName { get; } = "Sample Command 3";
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            PBDialogResult res = new PBDialogResult();

            Categories cats = doc.Settings.Categories;

            List<string> categoryNames = cats.Cast<Category>().Select(c => c.Name).ToList();
            object[] names = categoryNames.Cast<object>().ToArray();

            test_frmCommand3 Form = new test_frmCommand3();
            Form.AddItemsToCBox(names);
            res = Form.ShowDialogWithResult();


            if (res.IsAccepted) {
                MessageBox.Show($"you selected item {categoryNames[(int)res.SelectionResults[0]]}");
            }
            else {
                MessageBox.Show("you cancelled the operation");
            }

            return Result.Succeeded;
        }
    }
}
