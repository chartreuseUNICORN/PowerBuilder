using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    public class OverrideGraphicSettingsMerger {
    private List<OverrideGraphicSettings> _Overrides;
    public OverrideGraphicSettingsMerger(IEnumerable<OverrideGraphicSettings> Overrides) {
            _Overrides = Overrides.ToList();
        }
        /// <summary>
        /// Merge OverrideGraphicSettings according to the specified mode.
        /// </summary>
        /// <param name="mode">0-Opaque, 1-Translucent</param>
        /// <returns></returns>
        public OverrideGraphicSettings MergeOverrides(int mode=0) {
            OverrideGraphicSettings MergedOverrides = new OverrideGraphicSettings();
            switch (mode) {
                case 1:
                    return TranslucentMerge();
                    break;
                default:
                    return OpaqueMerge();
            }
        }
        private OverrideGraphicSettings OpaqueMerge () {
            return _Overrides[0];
        }
        private OverrideGraphicSettings TranslucentMerge() {
            throw new NotImplementedException("Translucent Merge not implemented yet. Please use Opaque Merge.");
        }
    }
}
