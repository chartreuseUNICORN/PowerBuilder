using PowerBuilder.Utils;
using Serilog;
using System.Collections.Generic;
using RvtView = Autodesk.Revit.DB.View;
using PowerBuilder.Extensions;
using System.Diagnostics;

namespace PowerBuilder.Services {

    // what are the issues we're trying to address here
    // * when the first callout is created, create a selection set and filters for the parent view
    // * when an element is created within a callout, add it to the view's calloutBlocking selection set
    // * when an element is moved into a callout, add it to the view's calloutBlocking selection set
    // * when a callout boundary is changed, add all elements within the bounding box to the calloutBlocking selection set
    // * when a callout is added, add all elements within the bounding box to the calloutBlocking selection set

    // TODO: how does this efficiently handle detection for non-rectangular callout boundaries
    public class CalloutBlockingManager {
        private Document _doc = null;
        private RvtView _managedView = null;
        private SelectionFilterElement _managedViewSef = null;
        private List<ElementId> _viewerIds = null;
        public bool isEnabled { get; } = false;

        public CalloutBlockingManager(Document doc, RvtView activeView, bool includeInTransaction) { // is this really a manager class or more just a utils library

            _doc = doc;
            _managedView = activeView;
            
            string calloutBlockingSetName = $"calloutBlocking_{activeView.Id}";
            Log.Debug($"expect to find callout blocking sef [{calloutBlockingSetName}]");

            List<ElementId> fec_SelectionSet = new FilteredElementCollector(_doc)
                .OfClass(typeof(SelectionFilterElement))
                .WhereElementIsNotElementType()
                .Where(x => x.Name == calloutBlockingSetName)
                .Select(x => x.Id)
                .ToList();

            ElementCategoryFilter getViewerFilter = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);
            _viewerIds = activeView.GetDependentElements(getViewerFilter).ToList();

            if (_viewerIds.Count > 0 ) {
                if (fec_SelectionSet.Count == 0) {
                    if (includeInTransaction) {
                        using (Transaction T = new Transaction(doc)) {
                            T.Start($"configure-callout-blocking-{_managedView.Id}");
                            try {
                                _managedViewSef = ConfigureViewCalloutBlocking(_viewerIds);
                                T.Commit();
                            }
                            catch {
                                T.RollBack();
                            }
                        }
                    }
                    else {
                        _managedViewSef = ConfigureViewCalloutBlocking(_viewerIds);
                    }
                }
                else {
                    _managedViewSef = _doc.GetElement(fec_SelectionSet.FirstOrDefault()) as SelectionFilterElement;
                }
                isEnabled = true;
            }
            else { isEnabled = false; }
        }

        internal SelectionFilterElement ConfigureViewCalloutBlocking(List<ElementId> viewerIds) {
            // create a named selection set associated with the active view
            ElementFilter calloutBlockingFilter = GetInsideViewerFilter();

            List<BuiltInCategory> mepModelCats = CategoryUtils.GetMepModeElements();
            ElementMulticategoryFilter modelElementFilter = new ElementMulticategoryFilter(mepModelCats);

            List<ElementId> fec_blockedElements = new FilteredElementCollector(_doc, _managedView.Id)
                .WhereElementIsNotElementType()
                .WherePasses(calloutBlockingFilter)
                .WherePasses(modelElementFilter)
                .ToElementIds()
                .ToList();

            string cbSetName = $"calloutBlocking_{_managedView.Id}";

            SelectionFilterElement calloutBlockingSet = SelectionFilterElement.Create(_doc, cbSetName);
            calloutBlockingSet.SetElementIds(fec_blockedElements);

            // create a ViewFileter targeting the calloutBlocking set for the active view
            _managedView.AddFilter(calloutBlockingSet.Id);
            _managedView.SetFilterVisibility(calloutBlockingSet.Id, false);

            OverrideGraphicSettings ogsCalloutBlockingFilter = _managedView.GetFilterOverrides(calloutBlockingSet.Id);

            ogsCalloutBlockingFilter.SetProjectionLineColor(new Autodesk.Revit.DB.Color(255, 128, 255));
            _managedView.SetFilterOverrides(calloutBlockingSet.Id, ogsCalloutBlockingFilter);

            return calloutBlockingSet;
        }

        public ElementFilter GetInsideViewerFilter() {
            List<ElementFilter> isInViewerFilters = new List<ElementFilter>();
            
            foreach (ElementId viewerId in _viewerIds) {

                
                Element viewer = _doc.GetElement(viewerId);
                ElementId viewId = viewer.GetDependentElements(new ElementClassFilter(typeof(ViewPlan))).FirstOrDefault();
                ViewPlan view = _doc.GetElement(viewId) as ViewPlan;

                BoundingBoxXYZ viewerBbox = view.GetCropBoundingBox();
                Outline outline = new Outline(viewerBbox.Min, viewerBbox.Max);

                isInViewerFilters.Add(new BoundingBoxIsInsideFilter(outline));
            }
            LogicalOrFilter isInsideViewerBboxFilter = new LogicalOrFilter(isInViewerFilters);

            return isInsideViewerBboxFilter;
        }
        
        public void AssignElementVisibility(ElementId eid) { 
            // for things like this, how should transactions be managed.  i think the atomic functions can be unmanaged
            ElementFilter isInsideViewersFilter = GetInsideViewerFilter();
            if (isInsideViewersFilter.PassesFilter(_doc, eid)) {
                _managedViewSef.AddSingle(eid);
            }
            else if (_managedViewSef.Contains(eid)){
                _managedViewSef.RemoveSingle(eid);
            }
        }

        public void AssignElementsVisibility(ICollection<ElementId> elementIds) {

            // ok, let's think through this. this is an arbitrary collection of elements
            // some will be inside the viewers. some will be outside viewers
            // 
            HashSet<ElementId> managedElementIds = new HashSet<ElementId>(elementIds);

            Debug.WriteLine($"<> {_managedViewSef.Name} ct: {_managedViewSef.GetElementIds().Count}");
            ElementFilter isInsideViewersFilter = GetInsideViewerFilter();


            HashSet<ElementId> hiddenElementIds = managedElementIds
                .Where(x => isInsideViewersFilter.PassesFilter(_doc, x))
                .ToHashSet();

            HashSet<ElementId> visibleElementIds = managedElementIds
                .Where(x => !isInsideViewersFilter.PassesFilter(_doc, x))
                .ToHashSet();

            int countChanged;

            if (hiddenElementIds.Count > 0) {
                _managedViewSef.AddSet(hiddenElementIds);
            }
            if (visibleElementIds.Count > 0) {
                //Debug
                foreach (ElementId eid in visibleElementIds) {
                    Debug.WriteLine($"managedSef contains {eid}: {_managedViewSef.Contains(eid)}");
                    Debug.WriteLine($"element ({eid}) contained in a viewer: {isInsideViewersFilter.PassesFilter(_doc, eid)}");
                }

                if (visibleElementIds.Count > 0) {
                    countChanged = _managedViewSef.RemoveSet(visibleElementIds);
                    Debug.WriteLine($"successfully removed {countChanged} elements");
                }
                Debug.WriteLine($"<> {_managedViewSef.Name} ct: {_managedViewSef.GetElementIds().Count}");
            }
            _managedView.RefreshView();
        }
        public ICollection<ElementId> GetManagedElements() {
            return _managedViewSef.GetElementIds();
        }
    }
}
