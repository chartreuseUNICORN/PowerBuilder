using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    // reference: https://rippleengineeringsoftware.com/practical-air-terminal-selection-and-layout-procedure/
    // reference: https://aircondlounge.com/hvac-diffuser-sizing-guide-cfm-chart-selection/

    public class AirDiffuserSelectionService {
        private Document _doc;
        public AirDiffuserSelectionService(Document doc) {
            // does this actually need to check dependencies?
            // it can just skip the calculation if parameters are missing
            DependencyChecker depCheck = new DependencyChecker(doc);
        }

        public void UpdateAirDiffuser (FamilyInstance airDiff) {
            // it makes the  most sense to store the selection data in the family.
            // in like a github repo, or as a lookuptable.
        }
    }
}
