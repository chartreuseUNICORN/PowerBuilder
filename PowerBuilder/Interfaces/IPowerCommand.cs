using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace PowerBuilder.Interfaces
{
    /// <summary>
    /// Interface for implementing PowerBuilder commands
    /// 
    /// DEPRECATED. Create new commands from CmdBase
    /// 
    /// </summary>
    
    public interface IPowerCommand : IExternalCommand
    {
        string _displayName { get; }
        string ShortDesc { get; }
        bool RibbonIncludeFlag { get; }

        ///<summary>
        /// User interaction should always be separated from command function.  Place the call for the Form here, or trigger Revit Selection UI
        ///</summary>
        PowerDialogResult GetInput(UIApplication uiapp);
    }
}

