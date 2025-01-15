using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder
{
    /// <summary>
    /// Container for User Input results.  First element 'IsAccepted' indicates the run status.  Successful input collection results in True,
    /// canceled or failed collection results in false.  The second element 'SelectionResults' is a container for collected objects, most 
    /// commonly this will contain lists of selections or run modes.
    /// </summary>
    public class PBDialogResult
    {
        public bool IsAccepted { get; set; }
        public List<object> SelectionResults { get; set; }

        public PBDialogResult()
        {
            SelectionResults = new List<object>();
        }
        // i don't know if any of these are necessary.  maybe if we make the selection results private, then there needs to be some way to access them
        /// <summary>
        /// Add an object to the selection results
        /// </summary>
        /// <param name="result">object to add to the selection results</param>
        public void AddSelectionResult(object result)
        {
            SelectionResults.Add(result);
        }

        /// <summary>
        /// Return the selection result an an indicated index, cast to an indicated type <T>
        /// </summary>
        /// <typeparam name="T">casting type</typeparam>
        /// <param name="index">result item index</param>
        /// <returns></returns>
        public T GetSelectionResult<T>(int index)
        {
            return (T)SelectionResults[index];
        }

        /// <summary>
        /// Empty the SelectionResults list
        /// </summary>
        public void ClearSelectionResults()
        {
            SelectionResults.Clear();
        }
    }

}
