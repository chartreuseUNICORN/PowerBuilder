using Autodesk.Revit.UI;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// Base class for IExternalCommands managed by PowerBuilder. Includes identification
    /// and ribbon status properties
    /// </summary>
    public abstract class CmdBase : IExternalCommand {
        //add ValidScope or equivalent enum designating where the command is valid
        public abstract string DisplayName { get; }
        public abstract string ShortDesc { get; }
        public virtual bool RibbonIncludeFlag { get; set; }

        public abstract Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements);

        public abstract PowerDialogResult GetInput(UIApplication uiapp);

        public CmdSignature GetCommandSignature() {
            return new CmdSignature(DisplayName, ShortDesc, RibbonIncludeFlag);
        }
    }
}
