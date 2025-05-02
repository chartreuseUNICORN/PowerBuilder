#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdFamilyLibraryScanner : IPowerCommand {
        public string DisplayName { get; } = "Family Library Scanner";
        public string ShortDesc { get; } = "Select a directory to scan and produce a report with family name, size, authoring version, and category";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            PowerDialogResult res = GetInput(uiapp);
            ScanFamilyLibrary(res.SelectionResults[0] as string);

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            PowerDialogResult res = new PowerDialogResult();
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            res.SelectionResults.Add(folderBrowserDialog.SelectedPath);
            return res;
        }
        public void ScanFamilyLibrary (string path) {

            List<string> FilePaths = new List<string>();
            string[] files = Directory.GetFiles(path,"*.rfa", SearchOption.AllDirectories);
            string timestamp = DateTime.Now.ToShortTimeString();
            
            //where does this go?
            //it seems sensible to save it to the path that's selected
            //but Desktop and AppData also seem reasonable
            string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\FamilyLibraryScanner.txt";

            using (StreamWriter sw = new StreamWriter(DesktopPath)) {
                foreach (string file in files) {
                    FileInfo f = new FileInfo(file);
                    List<string> FileData = new List<string>() { f.Name, f.Length.ToString(), f.LastAccessTime.ToFileTime().ToString()};

                    /*
                     * TODO: implement partatom xml scan for Category, classification parameters
                     * TODO: generalize this to be a more dynamic partatom query builder to be dynamic, or accept customized user inputs
                     */

                    sw.WriteLine(String.Join(",",FileData));
                    
                }
            }

        }
    }
}
