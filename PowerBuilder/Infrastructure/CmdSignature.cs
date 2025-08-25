using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// Wrapper for commaand ui data
    /// </summary>
    public class CmdSignature {

        private string _displayName;
        private string _shortDesc;
        public string DisplayName => _displayName;
        public string ShortDesc => _shortDesc;
        public bool RibbonIncludeFlag { get; set; }
        //TODO: include icon path

        public CmdSignature() { }
        public CmdSignature(string displayName, string shortDesc, bool ribbonIncludeFlag = true) {
            _displayName = displayName;
            _shortDesc = shortDesc;
            RibbonIncludeFlag = ribbonIncludeFlag;
        }
        
    }
}
