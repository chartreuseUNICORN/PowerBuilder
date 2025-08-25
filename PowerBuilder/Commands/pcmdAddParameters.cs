#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Extensions;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdAddParameters : CmdBase {
        public override string DisplayName { get; } = "Add Parameters";
        public override string ShortDesc { get; } = "Select a partial parameter file and add Parameters to the active Family Document";
        public override bool RibbonIncludeFlag { get; set; } = true; 
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements){

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            
            Log.Debug($"{this.GetType()}");
            Log.Debug("TEST-ADD-PARAMETERS");

            if (doc.IsFamilyDocument) {

                //all of this can probably be implemented as a base class "FileBased" or "RequiresFile"
                //can you do this as an attribute? [UserSelectedFile]
                FamilyManager famMan = doc.FamilyManager;
                List<ExternalDefinition> parameterDefs = ParseParameterDefs(uiapp);
                using (Transaction T = new Transaction(doc, "batch-add-parameters")) {
                    T.Start();
                    foreach (ExternalDefinition pDef in parameterDefs) {
                        //the thought here is to enable parameter definitions to be marked with an invalid GUID in the file, instead of trying to find some 
                        //
                        Log.Debug($"Try-Add\t{pDef.Name}");
                        bool isInstance = true;
                        ForgeTypeId groupTypeId = pDef.GetGroupTypeId();
                        if (pDef.GUID.ToString() != "-1") {
                            ForgeTypeId specTypeId = pDef.GetDataType();
                            //famMan.AddParameter(pDef.Name, groupTypeId, specTypeId, isInstance);
                            Log.Debug("valid GUID ok");
                        }
                        else {
                            //famMan.AddParameter(pDef, groupTypeId, isInstance);
                            Log.Debug("not a valid GUID");
                        }
                    }
                    T.Commit();
                }
            }
            else {
                TaskDialog msg = new TaskDialog("Add Parameters");
                msg.MainContent = "Run this command in a Family Document";
                msg.Show();
            }



            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }

        private List<ExternalDefinition> ParseParameterDefs(UIApplication uiapp) {

            //need to do this thing where we float and swap the current shared parameter path
            //Id really like to be able to accommodate Family and Shared parameters here.  my thought 
            Log.Debug("ENTER ParseParameterDefs");
            
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            string referenceSpPath = app.SharedParametersFilename;

            
            app.SharedParametersFilename = GetFilePath();
            DefinitionFile spFile = app.OpenSharedParameterFile();

            Log.Debug($"access spfile {spFile.Filename}");
            List<ExternalDefinition> externalDefinitions = ExtractDefinitionsFromGroups(spFile.Groups).ToList();
            Log.Debug($"found {externalDefinitions.Count} itemss");

            app.SharedParametersFilename = referenceSpPath;
            return externalDefinitions;
        }
        private string GetFilePath() {
            Log.Debug("ENTER GetFilePath");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            string path = null;

            if (openFileDialog.ShowDialog() == DialogResult.OK) {

                Debug.WriteLine("first point");
                path = openFileDialog.FileName;
            }
            return path;
        }
        private List<ExternalDefinition> ExtractDefinitionsFromGroups(DefinitionGroups dgs) {
            Log.Debug("ENTER ExtractDefinitionsFromGroups");
            Log.Debug($"{dgs.Size} groups");
            List<ExternalDefinition> extDefs = new List<ExternalDefinition>();

            foreach (DefinitionGroup dg in dgs) {
                Debug.WriteLine($"found DefinitionGroup {dg.Name}");
                foreach (ExternalDefinition def in dg.Definitions) {
                    Debug.WriteLine($"found definition {def.Name}");
                    extDefs.Add(def);
                }
            }

            return extDefs;
        }
    }
}
