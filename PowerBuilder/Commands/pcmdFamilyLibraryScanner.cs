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
using System.Xml;
using System.Linq;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Windows.Forms;

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
            string TargetLibraryPath;
            /* uncomment when testing complete
            PowerDialogResult res = GetInput(uiapp);
            TargetLibraryPath = res.SelectionResults[0] as string
            */
            TargetLibraryPath = "C:\\Users\\mclough\\OneDrive - Symetri\\_Coding\\PowerBuilder_Test\\TEST_FamilyLibrary";

            ScanFamilyLibrary(TargetLibraryPath);

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
            string[] FileHeaders = ["Name", "Size", "AccessTime", "Category", "OmniClass Number", "Type Qty", "Authoring Version"];

            using (StreamWriter sw = new StreamWriter(DesktopPath)) {
                sw.WriteLine(String.Join(",", FileHeaders));
                foreach (string file in files) {
                    FileInfo f = new FileInfo(file);
                    List<string> FileData = new List<string>() { f.Name, f.Length.ToString(), f.LastAccessTime.ToShortDateString()};
                    Debug.WriteLine(file);

                    XmlDocument FamilyPartatom = ParsePartatomFromFile(file);
                    FileData.Add(GetValueByXpath (FamilyPartatom, "//atom:category[atom:scheme='adsk:revit:grouping']/atom:term"));
                    FileData.Add(GetValueByXpath(FamilyPartatom, "//A:group[A:title='Identity Data']/atom:OmniClass_Number"));
                    FileData.Add(GetValueByXpath(FamilyPartatom, "//A:family/A:variationCount"));
                    FileData.Add(GetValueByXpath(FamilyPartatom, "//A:design-file/A:product-version"));
                    
                    sw.WriteLine(String.Join(",",FileData));
                }
            }
        }
        public XmlDocument ParsePartatomFromFile(string path) { 
            string PartatomString = null;
            string xmlBase = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            XmlDocument PartAtom = new XmlDocument();

            using (StreamReader reader = new StreamReader(path)) {
                while (PartatomString == null) { 
                    string check = reader.ReadLine();
                    if (check.StartsWith("<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:A=\"urn:schemas-autodesk-com:partatom\">")) {
                        PartatomString = xmlBase+check;
                    }
                }
            }
            
            PartAtom.LoadXml(PartatomString);
            return PartAtom;
        }
        public string GetValueByXpath (XmlDocument partatom, string Xpath) {
            //is this better as "GetXmlDataByDOMquery"?

            List<string> FamilyData = new List<string>();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(partatom.NameTable);
            nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("A", "urn:schemas-autodesk-com:partatom");
            
            XmlNode target = partatom.SelectSingleNode(Xpath, nsmgr);
            Debug.WriteLine(target?.InnerText ?? string.Empty);
            
            return target?.InnerText ?? string.Empty;
        }
    }
}
