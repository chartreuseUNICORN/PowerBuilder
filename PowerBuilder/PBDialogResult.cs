using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder
{
    public class PBDialogResult
    {
        public bool IsAccepted { get; set; } // True if "Accept" button was pressed, otherwise false
        public int? SelectedIndex { get; set; } // The selected index from the ComboBox, null if "Cancel" was pressed
    }

}
