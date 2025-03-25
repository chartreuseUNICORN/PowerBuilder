using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Nice3point.Revit.Extensions;
using PowerBuilder.Extensions;
using PowerBuilder.Interfaces;

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdUpdateViewTemplatesByVTsequence : IPowerCommand {
        public string DisplayName { get; } = "Update View Templates by Layer";
        public string ShortDesc { get; } = "Sets view accessible view properties according to Templates listed in the parameter \"TemplateLayers\" to each view template." +
            "Graphic Overrides from all Layered Templates are combined." +
            "\nUnmodifiable options:\n" +
            "\tShadows\n\tLighting\n\tPhotographic Exposure\n\n" +
            "Control these independently in the View Template's settings. They are unchanged by this procedure.";
        public bool RibbonIncludeFlag { get; } = true;
        //TODO: manage control parameter and RibbonIncludeFlag as part of startup
        /* There are special cases that need to be considered
             *      Overrides: manipulate these using the Get and Set methods
             *      accessible Graphic Display Options
             *          need to verify if manipulating the object produced by the Get methods changes the view's settings when the
             *          returned type is not an Element. e.g. is ViewModelDislpay linked to the specific view 
             * There are Graphic Display Options that cannot be controlled
             * to account for these, it may be required to create the successively apply templates in order to create the final state
             * what is probably most reasonable to do is clarify the limitations in the tooltip.
             */
        private static HashSet<BuiltInParameter> _overrideParameters = new HashSet<BuiltInParameter>() {
                BuiltInParameter.VIS_GRAPHICS_MODEL,
                BuiltInParameter.VIS_GRAPHICS_ANNOTATION,
                BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL,
            };
        private static HashSet<BuiltInParameter> _omittedParameters = new HashSet<BuiltInParameter>() {
                // Not able to implement
                BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_PHOTO_EXPOSURE,
                BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_SHADOWS,
                BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_LIGHTING,
                // TODO: implement specific handling
                BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_SKETCHY_LINES,
                BuiltInParameter.COLOR_SCHEME_LOCATION,
                BuiltInParameter.VIEW_SCHEMA_SETTING_FOR_BUILDING,
                BuiltInParameter.VIEW_SCHEMA_SETTING_FOR_SYSTEM,
                BuiltInParameter.VIEW_SCHEMA_SETTING_FOR_SYSTEM_TEMPLATE,
                // this only works in 2024 and current
                BuiltInParameter.VIS_GRAPHICS_RVT_LINKS,
                BuiltInParameter.VIS_GRAPHICS_IMPORT,
                //this will need a check to determine if worksharing is enabled
                BuiltInParameter.VIS_GRAPHICS_WORKSETS,
                //this will need a check to determine if DO are used
                //i guess this is partly why the parameter Ids are produced in a method
                BuiltInParameter.VIS_GRAPHICS_DESIGNOPTIONS,
                BuiltInParameter.VIS_GRAPHICS_FILTERS,
            };
        private static Definition _controlParam;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            //TODO
            //  get control parameter "ViewTemplateLayers" from project parameters if it targets Category:Views
            //  may be useful to package this into Utils.ViewsUtils or something like this
            //  do you ever stash this in the Command attributes
            _controlParam = doc.ActiveView.LookupParameter("ViewTemplateLayers").Definition; //update this with a better procedure

            if (_controlParam != null) {
                List<Autodesk.Revit.DB.View> ViewTemplates = new FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.View))
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(vp => vp.IsTemplate).
                    ToList<Autodesk.Revit.DB.View>();
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("update-tagged-view-templates") == TransactionStatus.Started) {

                        foreach (Autodesk.Revit.DB.View vt in ViewTemplates) {
                            //TODO: figure out how to order the vts by dependency
                            //  e.g. if i have a -Composite layer that combines -Piping and -Ductwork, and a plan that uses -Composite,
                            //  -Composite needs to be updated first.
                            if (vt.LookupParameter("ViewTemplateLayers").HasValue) UpdateViewTemplatesByVTsequence(vt, ViewTemplates, 0);
                        }
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }
            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("method not used");
        }
        /// <summary>
        /// Applies View settings to a target View from other Views identified by name in the parameter "ViewTemplateLayers" from bottom to top.  Run with a mode selected to control the merge behavior
        /// </summary>
        /// <param name="ViewTemplate">Target view to modify</param>
        /// <param name="ViewTemplates">Collection of all view templates in the project</param>
        /// <param name="mode">Mode selector: 1 = opaque merge, 2 = transparent merge, -1 for blind merge</param>
        public void UpdateViewTemplatesByVTsequence(Autodesk.Revit.DB.View ViewTemplate, List<Autodesk.Revit.DB.View> ViewTemplates, int mode=-1) {
            //TODO: implement mode selector
            // get sequence names from multi-line text
            Document doc = ViewTemplate.Document;
            //TODO: validate the parameter has a value.  probably on line 36 in the VT collector
            List<string> TemplateSequenceNames = ViewTemplate.LookupParameter("ViewTemplateLayers").AsString().Split('\n').Select(x => x.Trim()).ToList();
            Debug.WriteLine($"<CHECK VIEW TEMPLATE>{ViewTemplate.Name}");
            foreach (string vlayer in TemplateSequenceNames) Debug.WriteLine($"\t{vlayer}");
            // TODO: add some sort of input validation.  at least need to clean position control whitespace \t \n \r

            List<Autodesk.Revit.DB.View> ViewTemplateSequence = ViewTemplates
                .Where(x => TemplateSequenceNames.Contains(x.Name))
                .OrderBy(x => TemplateSequenceNames.IndexOf(x.Name))
                .ToList();

            ViewTemplateSequence.Reverse();

            HashSet<ElementId> TemplateParameters = new HashSet<ElementId>(ViewTemplate.GetTemplateParameterIds());

            ViewTemplate.SetNonControlledTemplateParameterIds(TemplateParameters);
            foreach (Autodesk.Revit.DB.View ViewLayer in ViewTemplateSequence) ViewTemplate.ApplyViewTemplateParameters(ViewLayer);
            
            ViewTemplateSequence.Reverse();
            foreach (Autodesk.Revit.DB.View ViewLayer in ViewTemplateSequence) {
                //LayerAffectedParameters is the {ParameterIds} - {NonControlledTemplateParameters}
                HashSet<ElementId> LayerAffectedParameters = new HashSet<ElementId>(ViewLayer.GetTemplateParameterIds());
                LayerAffectedParameters.ExceptWith(ViewLayer.GetNonControlledTemplateParameterIds());

                TemplateParameters.ExceptWith(LayerAffectedParameters);
                
                //TODO refine the loop
                //this is currently O(n2) and can be reduced.  doesn't need to if it's a blind merge, the categories can be controlled from the top down
                //redundant category checks should be eliminated
                //if we're doing a 'transparent' overrides merge, then redundant category checking is probably ok.
                foreach (ElementId lap in LayerAffectedParameters) {
                    if (_overrideParameters.Contains((BuiltInParameter)(lap.Value))) {
                        MergeGraphicOverrideStrategy((BuiltInParameter)(lap.Value), ViewTemplate, ViewLayer);
                    }
                }
                ViewTemplate.SetNonControlledTemplateParameterIds(TemplateParameters);
            }
        }
        //TODO this should really return the function/function pointer and be processed at the top level. or in a controller function "MergeGraphicOverrides"
        private void MergeGraphicOverrideStrategy(BuiltInParameter bip, Autodesk.Revit.DB.View source, Autodesk.Revit.DB.View layer) {
            switch (bip) {
                case BuiltInParameter.VIS_GRAPHICS_MODEL or BuiltInParameter.VIS_GRAPHICS_ANNOTATION or BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL:
                    GraphicOverridesMergeCategory(bip, source, layer);
                    break;
                case BuiltInParameter.VIS_GRAPHICS_FILTERS:
                    GraphicOverridesMergeFilters(bip, source, layer);
                    break;
                default:
                    throw new ArgumentException($"unexpected Override BuiltInParameter:{bip}");
                }
            }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bip"></param>
        /// <param name="source"></param>
        /// <param name="layer"></param>
        private void GraphicOverridesMergeCategory(BuiltInParameter bip, Autodesk.Revit.DB.View source, Autodesk.Revit.DB.View layer) {
            /*TODO:
             * 
             * ISSUES:
             *  .   categories affected by previous layers not tracked. the real issue is that categories want to be set bottom-up and 
             *      filters want to be set top down..the tracking is needed if 'transparent' mode is ever implemented anyways.
             *          . maybe you transpose the logic and build the override for each item category/filter across the templates
             */
            Debug.WriteLine($"[GraphicOverridesMergeCategory] {layer.Name}\t->\t{source.Name}");
            Dictionary<BuiltInParameter, CategoryType> BipToCategoryType = new Dictionary<BuiltInParameter, CategoryType> {
                { BuiltInParameter.VIS_GRAPHICS_MODEL , CategoryType.Model},
                { BuiltInParameter.VIS_GRAPHICS_ANNOTATION , CategoryType.Annotation },
                { BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL , CategoryType.AnalyticalModel },
            };
            List<Category> layerCats = new List<Category>();
            foreach (Category cat in source.Document.Settings.Categories) {
                if (cat.CategoryType == BipToCategoryType[bip]) {
                    Debug.WriteLine($"\tMerge:\t{cat.Name}");
                    layerCats.Add(cat);
                }
            }
            
            //TODO replace above with one-liner (below)
            //List<ElementId> cats = new List<ElementId>(source.Document.Settings.Categories).Where(x => x.CategoryType == BipToCategoryType[bip]).Select(x => x.Id);
            foreach (Category laycat in layerCats) {
                bool CurrentCatState = layer.GetCategoryHidden(laycat.Id);
                if (!CurrentCatState && source.CanCategoryBeHidden(laycat.Id)) {
                    source.SetCategoryHidden(laycat.Id, CurrentCatState);
                    source.SetCategoryOverrides(laycat.Id, layer.GetCategoryOverrides(laycat.Id));
                    Debug.WriteLine($"\tSet CategoryHidden:\t{laycat.Name}{CurrentCatState}");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bip"></param>
        /// <param name="source"></param>
        /// <param name="layer"></param>
        private void GraphicOverridesMergeFilters(BuiltInParameter bip, Autodesk.Revit.DB.View source, Autodesk.Revit.DB.View layer) {
            /*TODO
             *  . implement filter order inforcement
             *    . Assume layer filters should always be at the bottom. retained source templates should be unchanged order within their set
             *    . i think this requires filters to be implemented from top down
             *    [x] blind reinforcement is $$ but simpler to implement from an expected order
             *    
             *ISSUES
             *  . Filter Elements affected by previous layers not tracked
             */
            Debug.WriteLine($"[GraphicOverridesMergeFilter] {layer.Name}\t->\t{source.Name}");
            foreach (ElementId filterId in layer.GetOrderedFilters().Reverse()) {
                OverrideGraphicSettings filterOGS = layer.GetFilterOverrides(filterId);
                if (source.GetFilters().Contains(filterId)) source.RemoveFilter(filterId);

                source.AddFilter(filterId);
                source.SetFilterVisibility(filterId, layer.GetFilterVisibility(filterId));
                source.SetIsFilterEnabled(filterId, layer.GetIsFilterEnabled(filterId));
                source.SetFilterOverrides(filterId, layer.GetFilterOverrides(filterId));
            }
        }
    }
}
