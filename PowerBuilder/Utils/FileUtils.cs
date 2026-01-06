using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    internal class FileUtils {

        private static string _default = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


        public static void WriteToFile(string message, string filename , string path = "") {
            
            if (path == "") path = _default;
            
            using (StreamWriter outFile = new StreamWriter(Path.Combine(path, filename),true)) {
                outFile.WriteLine(message);
            }
        }

        public static string GetManagedParameterPath() {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string managedParameterPath = root + "PB_SharedParameters.txt";
            return managedParameterPath;
        }
    }
}
