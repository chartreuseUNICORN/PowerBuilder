using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    /*
     so what is this.
     is this where you would use ExternalService vs. an updater?
    ExternalService -with- an updater?

    somewhere we need to cache the 'current' view state
    on view changing update new active view with the 

    pyrevit calls it ZoomPanState

    so this gives you that A-B behavior.  i can't remember if that other viewsync addin worked this way
    I think maybe the thing to try to make this mor ereal time is to make a custom EventHandler that monitors changes in the UIView
    This probably ends up too expensive/slow to really implement.
     */

    internal sealed class ViewSynchronizationService {
        private bool _status;
        
        internal ViewSynchronizationService () {
            _status = false;
        }
        public bool Status {
            get => _status;
            set {
                if (_status != value) {
                    _status = value;
                }
            }
        }
        internal void onViewActivated(object sender, ViewActivatedEventArgs e) {
            UIApplication sapp = sender as UIApplication;
            UIDocument a_uidoc = sapp.ActiveUIDocument;
            Document doc = a_uidoc.Document;

            IList<UIView> UIViews = a_uidoc.GetOpenUIViews();
            UIView prevUIView = UIViews.Where(x => x.ViewId == e.PreviousActiveView.Id).FirstOrDefault();
            UIViews.Remove(prevUIView);

            IList<XYZ> PanAndZoom = prevUIView.GetZoomCorners();

            foreach (UIView uiview in UIViews) {
                if(doc.GetElement(uiview.ViewId) is ViewPlan) {
                    uiview.ZoomAndCenterRectangle(PanAndZoom.First(), PanAndZoom.Last());
                }
            }
        }
        internal void ActivateService(UIApplication uiapp) {
            _status = true;
            uiapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(onViewActivated);
        }
        internal void DeactivateService(UIApplication uiapp) {
            _status = false;
            uiapp.ViewActivated -= new EventHandler<ViewActivatedEventArgs>(onViewActivated);
        }
    }
}
