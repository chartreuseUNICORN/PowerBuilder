using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Interfaces {
    internal interface IDocumentUpdater: IUpdater {
        
        public void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args);
        public void updater_OnDocumentClosing(object sender, DocumentClosingEventArgs args);
    }
}
