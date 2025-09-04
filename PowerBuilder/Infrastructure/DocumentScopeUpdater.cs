using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace PowerBuilder.IUpdaters {
    public abstract class DocumentScopeUpdater {

        protected UpdaterId _uid;
        protected AddInId _appId;
        protected bool LoadOnStartup;

        /// <summary>
        /// Base class for IUpdaters targeting Documents
        /// </summary>
        public abstract void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args);
        
        public virtual void updater_OnDocumentClosing (object sender, DocumentClosingEventArgs args) {
            UpdaterRegistry.RemoveDocumentTriggers(_uid, args.Document);
            Log.Debug($"{this.GetType().Name} | document closing event @ {args.Document.Title}");
        }
    }
}
