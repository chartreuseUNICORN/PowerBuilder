using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PowerBuilder.Services {

    
    public class ViewTemplateViewLayerUpdater {
        private Document doc;
        private Autodesk.Revit.DB.View _ViewTemplate;

        public ViewTemplateViewLayerUpdater(Autodesk.Revit.DB.View ViewTemplate) {
            doc = ViewTemplate.Document;

        }
    }
}
