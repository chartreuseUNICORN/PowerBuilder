using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// Base class for IUpdaters in PowerBuilder. Includes components to ensure interaction with DmuRegistry
    /// </summary>
    public abstract class DmuBase : IUpdater {
        protected abstract string _name { get; }
        protected abstract string _description { get; }
        public abstract bool LoadOnStartup { get; }
        protected UpdaterId _uid;
        protected ChangePriority _changePriority;
        protected AddInId _addInId;
        
        public abstract void Execute(UpdaterData data);
        public UpdaterId GetUpdaterId() => _uid;
        public string GetAdditionalInformation() => _description;
        public string GetUpdaterName() => _name;
        public virtual ChangePriority GetChangePriority() => _changePriority;
    }
}