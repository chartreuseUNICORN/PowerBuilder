using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NUnit.Framework;
using System;
using System.Reflection;
using PowerBuilder.Commands;

[assembly: AssemblyMetadata("NUnit.Version", "2024")]

namespace SampleNUnitProject {
    public class Tests {
        UIApplication uiapp;
        [OneTimeSetUp]
        public void OneTimeSetup (UIApplication uiapp, Application app, UIControlledApplication uicapp, ControlledApplication capp){
            Console.WriteLine(uiapp);
            Console.WriteLine(app);
            Console.WriteLine(uicapp);
            Console.WriteLine(capp);
        }
        [SetUp]
        public void Setup(UIApplication uiapp) {
            this.uiapp = uiapp;
        }
        
        [Test]
        public void Test1() {
            Document document = uiapp.ActiveUIDocument?.Document;
            if (document == null)
                Assert.Ignore("No document open");
            Console.WriteLine(uiapp.Application.VersionBuild);
            
        }
    }
}