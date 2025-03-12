using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    internal class FileUtils {
        
        public static void WriteToFile(string message) {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            using (StreamWriter outFile = new StreamWriter(Path.Combine(docPath, "ValidationLog.txt"))) {
                outFile.WriteLine(message);
            }
        }
    }
}
