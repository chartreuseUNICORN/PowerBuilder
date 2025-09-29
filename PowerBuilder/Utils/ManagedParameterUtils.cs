using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    /// <summary>
    /// Collection of functions for managing parameters defined for the application
    /// </summary>
    public static class ManagedParameterUtils {
        
        /// <summary>
        /// Retrieve External Definition from PowerBuilder shared parameters file by Definition guid
        /// </summary>
        /// <param name="doc">current document</param>
        /// <param name="guid">target parameter GUID</param>
        /// <returns>ExternalDefinition</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ExternalDefinition LookupManagedParameter(Document doc, Guid guid) {
            //string managedSpPath = FileUtils.GetManagedParameterPath();
            //hardcoded path for development
            string managedSpPath = "C:\\Users\\mattycakes\\source\\repos\\PowerBuilder\\PB_SharedParameters.txt";
            List<ExternalDefinition> managedParameters = GetManagedDefinitions(managedSpPath, doc);
            int index = managedParameters.FindIndex(x => x.GUID.Equals(guid));
            if (index < 0) {
                throw new ArgumentOutOfRangeException("parameter not found");
            }
            return managedParameters[index];
        }
        
        /// <summary>
        /// Retrieve External Definition from PowerBuilder shared parameters file by parameter name
        /// </summary>
        /// <param name="doc">current document</param>
        /// <param name="parameterName">target parameter name</param>
        /// <returns>ExternalDefinition</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ExternalDefinition LookupManagedParameter(Document doc, string parameterName) {
            string managedSpPath = "\"C:\\Users\\mattycakes\\source\\repos\\PowerBuilder\\PB_SharedParameters.txt\"";
            List<ExternalDefinition> managedParameters = GetManagedDefinitions(managedSpPath, doc);
            int index = managedParameters.FindIndex(x => x.Name.Equals(parameterName));
            if (index < 0) {
                throw new ArgumentOutOfRangeException("parameter not found");
            }
            return managedParameters[index];
        }

        private static List<ExternalDefinition> GetManagedDefinitions (string path, Document doc) {
            List<ExternalDefinition> managedDefinitions = new List<ExternalDefinition> ();
            //using (Transaction T = new Transaction(doc, "temporary-sp-swap")){
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            Debug.WriteLine($"check existing shared parameter path: {app.SharedParametersFilename}");
            string referenceSpPath = app.SharedParametersFilename;

            app.SharedParametersFilename = path;
            Debug.WriteLine($"changed sp path to: {app.SharedParametersFilename}");
            DefinitionFile spFile = app.OpenSharedParameterFile();

            managedDefinitions = ExtractDefinitionsFromGroups(spFile.Groups).ToList();
            app.SharedParametersFilename = referenceSpPath;
                //T.RollBack();
            //}
            return managedDefinitions;
        }
        private static List<ExternalDefinition> ExtractDefinitionsFromGroups(DefinitionGroups dgs) {
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
