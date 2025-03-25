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
    /// </summary>
    public interface IPowerCommand : IExternalCommand
    {
        abstract string DisplayName { get; }
        abstract string ShortDesc { get; }
        abstract bool RibbonIncludeFlag { get; }

        ///<summary>
        /// User interaction should always be separated from command function.  Place the call for the Form here, or trigger Revit Selection UI
        ///</summary>
        PowerDialogResult GetInput(UIApplication uiapp);
    }
}

