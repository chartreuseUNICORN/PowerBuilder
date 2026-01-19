using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using RvtView = Autodesk.Revit.DB.View;

namespace PowerBuilder.Extensions {
    public static class ViewExtensions {
        public static BoundingBoxXYZ GetCropBoundingBox (this ViewPlan view) {

            Document doc = view.Document;
            
            PlanViewRange vr = view.GetViewRange();
            ElementId bottomLevelId = vr.GetLevelId(PlanViewPlane.BottomClipPlane);
            ElementId topLevelId = vr.GetLevelId(PlanViewPlane.TopClipPlane);

            Level bottomLevel = doc.GetElement(bottomLevelId) as Level;
            Level topLevel = doc.GetElement(topLevelId) as Level;

            double bottomOffset = vr.GetOffset(PlanViewPlane.BottomClipPlane);
            double topOffset = vr.GetOffset(PlanViewPlane.TopClipPlane);
            double zMin = bottomLevel.Elevation + bottomOffset;
            double zMax = topLevel.Elevation + topOffset;

            BoundingBoxXYZ bbox = view.CropBox;

            XYZ bboxMax = bbox.Max;
            XYZ bboxMin = bbox.Min;

            XYZ cbboxMax = new XYZ(bboxMax.X, bboxMax.Y, zMax);
            XYZ cbboxMin = new XYZ(bboxMin.X, bboxMin.Y, zMin);

            BoundingBoxXYZ cbbox = new BoundingBoxXYZ();
            cbbox.Max = cbboxMax;
            cbbox.Min = cbboxMin;

            return cbbox;
        }

        public static void RefreshView (this Autodesk.Revit.DB.View view) {
            if (view.CropBoxActive != null) {
                bool originalState = view.CropBoxActive;
                view.CropBoxActive = !originalState;
                view.CropBoxActive = originalState;
            }
        }
    }
}