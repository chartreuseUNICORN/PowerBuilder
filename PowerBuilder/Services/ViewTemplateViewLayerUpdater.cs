using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using PowerBuilder.Extensions;

namespace PowerBuilder.Services {

    
    public class ViewTemplateViewLayerUpdater {
        private Document _doc;
        private Autodesk.Revit.DB.View _ViewTemplate;
        
        private Dictionary<ElementId, List<OverrideGraphicSettings>> _OverrideRegistry;
        private List<Autodesk.Revit.DB.View> _ViewSequence; //_ViewSequence is stored top down

        private static HashSet<BuiltInParameter> _OverrideParameters = new HashSet<BuiltInParameter>() {
                BuiltInParameter.VIS_GRAPHICS_MODEL,
                BuiltInParameter.VIS_GRAPHICS_ANNOTATION,
                BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL,
            };
        
        public ViewTemplateViewLayerUpdater(Autodesk.Revit.DB.View ViewTemplate, IEnumerable<Autodesk.Revit.DB.View> ViewLayers) {
            
            this._ViewTemplate = ViewTemplate;
            this._ViewSequence = ViewLayers.ToList();
            this._doc = ViewTemplate.Document;
            this._OverrideRegistry = new Dictionary<ElementId, List<OverrideGraphicSettings>>();
            ViewTemplate.SetNonControlledTemplateParameterIds(ViewTemplate.GetTemplateParameterIds());

            foreach(BuiltInParameter obip in _OverrideParameters) {
                /*_OverrideRegistry.Join(RegisterOverrides(obip),
                    d1 => d1.Key,
                    d2 => d2.Key,
                    (d1, d2) => new { d1.Key, Value1 = d1.Value, Value2 = d2.Value });*/

                foreach (KeyValuePair<ElementId, List<OverrideGraphicSettings>> kvp in RegisterOverrides(obip)) {
                    _OverrideRegistry.Add(kvp.Key, kvp.Value);
                }
            }
        }
        public void Update() {
            Debug.WriteLine($"Update View Template: {_ViewTemplate.Name}");
            HashSet<ElementId> TemplateNonControlledParameters;
            _ViewTemplate.SetNonControlledTemplateParameterIds(_ViewTemplate.GetTemplateParameterIds());

            TemplateNonControlledParameters = UpdateNonOverrideSettings();
            UpdateOverrideSettings();

            _ViewTemplate.SetNonControlledTemplateParameterIds(TemplateNonControlledParameters);
        }

        /// <summary>
        /// Apply View Template parameters from View Layer to the View Template in reverse order.
        /// </summary>
        private HashSet<ElementId> UpdateNonOverrideSettings() {
            HashSet<ElementId> TemplateNonControlledParameters = new HashSet<ElementId>(_ViewTemplate.GetTemplateParameterIds());
            //Debug.WriteLine($"Set Parameters by Override:");
            for (int i = _ViewSequence.Count-1; i > 0; i--) {
                Debug.WriteLine($"\t{_ViewSequence[i].Name}");
                _ViewTemplate.ApplyViewTemplateParameters(_ViewSequence[i]);
                TemplateNonControlledParameters.IntersectWith((_ViewSequence[i].GetNonControlledTemplateParameterIds()));
            }
            return TemplateNonControlledParameters;
        }

        private void UpdateOverrideSettings() {
            foreach (KeyValuePair <ElementId, List<OverrideGraphicSettings>> overrides in _OverrideRegistry) {
                
                //
                Element OverrideTarget = _doc.GetElement(overrides.Key);

                OverrideGraphicSettings MergedOverride = new OverrideGraphicSettings();
                MergedOverride = new OverrideGraphicSettingsMerger(overrides.Value).MergeOverrides();

                if (OverrideTarget == null || OverrideTarget.IsSameOrSubclass(typeof(Category))) {

                    _ViewTemplate.SetCategoryHidden(overrides.Key, false);
                    _ViewTemplate.SetCategoryOverrides(overrides.Key, MergedOverride);
                }
                else if (OverrideTarget.IsSameOrSubclass(typeof(ParameterFilterElement))) {
                    _ViewTemplate.SetFilterOverrides(overrides.Key, MergedOverride);
                }
                else {
                    throw new ArgumentException($"unexpected OverrideTarget Type:{OverrideTarget.GetType()}");
                }
            
            }

        }
        private Dictionary<ElementId, List<OverrideGraphicSettings>> RegisterOverrides(BuiltInParameter obip) {
            //Debug.WriteLine($"check override BIP: {obip}");
            switch (obip) {
                case BuiltInParameter.VIS_GRAPHICS_MODEL or BuiltInParameter.VIS_GRAPHICS_ANNOTATION or BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL:
                    return RegisterCategoryOverrides(obip);
                case BuiltInParameter.VIS_GRAPHICS_FILTERS:
                    return RegisterFilterOverrides ();
                default:
                    throw new ArgumentException($"unexpected Override BuiltInParameter:{obip}");
            }

        }
        private Dictionary<ElementId ,List<OverrideGraphicSettings>> RegisterCategoryOverrides(BuiltInParameter obip) {
            Dictionary<ElementId, List<OverrideGraphicSettings>> ObipOverrides = new Dictionary<ElementId, List<OverrideGraphicSettings>>();
            List<Category> Cats = GetCategoriesByOverrideParameter(obip);
            
            foreach (Category cat in Cats) {
                //Debug.WriteLine($"\tRegisterOverrides: {cat.Name}");
                List<OverrideGraphicSettings> CategoryOverrides = new List<OverrideGraphicSettings>();
                foreach (Autodesk.Revit.DB.View view in _ViewSequence) {
                    List<ElementId> LayerNotControlledParameterIds = view.GetNonControlledTemplateParameterIds().Select(x => x).ToList();
                    if (!LayerNotControlledParameterIds.Contains(new ElementId(obip)) && !view.GetCategoryHidden(cat.Id) && cat.get_AllowsVisibilityControl(view)) {
                        //Debug.WriteLine($"\t\t{obip} in ViewTemplate: {view.Name} => get {cat.Name} overrides");

                        CategoryOverrides.Add(view.GetCategoryOverrides(cat.Id));
                    }
                }
                if (CategoryOverrides.Count > 0) {
                    ObipOverrides[cat.Id] = CategoryOverrides;
                } 
            }    
            return ObipOverrides;
        }

        private Dictionary<ElementId, List<OverrideGraphicSettings>> RegisterFilterOverrides() {
            Dictionary<ElementId, List<OverrideGraphicSettings>> Overrides = new Dictionary<ElementId, List<OverrideGraphicSettings>>();
            foreach (Autodesk.Revit.DB.View view in _ViewSequence) {
                List<ElementId> ViewFilters = view.GetFilters().ToList();
                foreach (ElementId filterId in ViewFilters) {
                    if (!Overrides.ContainsKey(filterId)) {
                        Overrides[filterId] = new List<OverrideGraphicSettings>() { view.GetFilterOverrides(filterId) };
                    }
                    else {
                        Overrides[filterId].Add(view.GetFilterOverrides(filterId));
                    }
                }
            }
            return Overrides;
        }

        private List<Category> GetCategoriesByOverrideParameter (BuiltInParameter obip) {
            Dictionary<BuiltInParameter, CategoryType> _BipToCategoryType = new Dictionary<BuiltInParameter, CategoryType> {
                { BuiltInParameter.VIS_GRAPHICS_MODEL , CategoryType.Model},
                { BuiltInParameter.VIS_GRAPHICS_ANNOTATION , CategoryType.Annotation },
                { BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL , CategoryType.AnalyticalModel },
            };

            List<Category> OverrideCats = new List<Category>();
            foreach (Category cat in _doc.Settings.Categories) {
                if (cat.CategoryType == _BipToCategoryType[obip] && cat.IsVisibleInUI) {
                    //Debug.WriteLine($"\tMerge:\t{cat.Name}");
                    OverrideCats.Add(cat);
                }
            }
            return OverrideCats;
        }
    }
}
