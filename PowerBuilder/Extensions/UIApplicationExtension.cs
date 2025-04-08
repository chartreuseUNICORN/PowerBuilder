using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Extensions {
    public static class UIApplicationExtension {
    
        public static UIApplication GetUIApplication (this UIControlledApplication application) {

            var type = typeof(UIControlledApplication);

            var propertie = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(e => e.FieldType == typeof(UIApplication));

            return propertie?.GetValue(application) as UIApplication;
        }
    }
}
