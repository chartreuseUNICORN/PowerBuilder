using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.IUpdaters {
    public abstract class DocumentScopeUpdater {

        protected UpdaterId _uid;
        protected AddInId _appId;

        /// <summary>
        /// Base class for Parameter based updaters
        /// </summary>
        /// <param name="id">Active AddInId</param>
        /// <param name="ParameterGuid"></param>
        /// <param name="UpdaterGuid"></param>

        public abstract void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args);
        
        public virtual void updater_OnDocumentClosing (object sender, DocumentClosingEventArgs args) {
            UpdaterRegistry.RemoveDocumentTriggers(_uid, args.Document);
        }
    }
}
