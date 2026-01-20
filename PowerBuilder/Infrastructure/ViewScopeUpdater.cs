using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using PowerBuilder.Infrastructure;

namespace PowerBuilder.IUpdaters {
    /// <summary>
    /// Base class for IUpdaters targeting Views
    /// </summary>
    /// 

    //is in valuable to have this set up this way? changing the updater target for the views. I think the upside is 
    //for things like the callout blocker, maybe this helps limit the scope of what elements its trying to watch
    //at the same time, maybe it's just sort of overkill because you're really only ever in one transaction in one view so the effects
    //of the updater should really only 
    public abstract class ViewtScopeUpdater : DmuBase {
        
        public abstract void updater_OnViewActivated(object sender, DocumentOpenedEventArgs args);
        
        public virtual void updater_OnViewActivating (object sender, DocumentClosingEventArgs args) {
            UpdaterRegistry.RemoveDocumentTriggers(_uid, args.Document);
            Log.Debug($"{this.GetType().Name} | document closing event @ {args.Document.Title}");
        }
    }
}