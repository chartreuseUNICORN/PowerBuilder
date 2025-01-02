using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder
{
    public class PBDialogResult
    {
        public bool IsAccepted { get; set; }
        public List<object> SelectionResults { get; set; }

        public PBDialogResult()
        {
            SelectionResults = new List<object>();
        }

        // Method to add a single selection result
        public void AddSelectionResult(object result)
        {
            SelectionResults.Add(result);
        }

        // Method to get a selection result by index
        public T GetSelectionResult<T>(int index)
        {
            return (T)SelectionResults[index];
        }

        // Method to clear all selection results
        public void ClearSelectionResults()
        {
            SelectionResults.Clear();
        }
        // how/where does the unwrapping of inputs need to occur. i guess this is something where this whole type
        // thing makes you have to think just a litle bit more..
        // things we may run into:
        //      boolean     mode controller
        //      int[]       ListBox, CheckedListBox selection
        //      int         combobox selection, integer clicker
        //      double/float    number input
        //      string      string inputs
        // maybe List<object> is fine? is it possible to store it as (<T>,value) tuples?
    }

}
