using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using System.Diagnostics;
using System.CodeDom;

namespace PowerBuilder.Extensions {
    public static class UnitsExtension {
        public static void ExportToXml(this Units units, string OutPath) {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            

            IList<ForgeTypeId> ModifiableSpecs = Units.GetModifiableSpecs();


            using (XmlWriter writer = XmlWriter.Create(OutPath, settings)) {
                
                writer.WriteStartElement("UnitConfigs");
                foreach (ForgeTypeId spec in ModifiableSpecs) {

                    FormatOptions formatOptions = units.GetFormatOptions(spec);
                    string symbolTypeId = formatOptions.GetSymbolTypeId().TypeId;

                    Type formatOptionsType = formatOptions.GetType();
                    PropertyInfo[] foProps = formatOptionsType.GetProperties();
                    Log.Debug($"CHECK-FORMAT-OPTIONS[{spec.TypeId}]");

                    writer.WriteStartElement("UnitConfiguration");
                    writer.WriteStartElement("SpecTypeId");
                    writer.WriteString(spec.TypeId.ToString());
                    writer.WriteEndElement();
                    
                    writer.WriteStartElement("FormatOptions");
                    writer.WriteStartElement("SymbolTypeId");
                    writer.WriteString(symbolTypeId);
                    writer.WriteEndElement();

                    writer.WriteStartElement("UnitTypeId");
                    writer.WriteString(formatOptions.GetUnitTypeId().TypeId);
                    writer.WriteEndElement();

                    foreach (PropertyInfo prop in foProps) {
                        
                        Log.Debug($"\t{prop.Name}:\t{prop.GetValue(formatOptions)}\tas {prop.PropertyType}\tCanread: {prop.CanRead}\tCanWrite{prop.CanWrite}");
                        if (prop.CanWrite && prop.CanRead) {
                            writer.WriteStartElement($"{prop.Name}");
                            writer.WriteAttributeString("type", prop.PropertyType.ToString());
                            if (prop.Name == "RoundingMethod") //this is ugly, but works
                                writer.WriteValue((int)(prop.GetValue(formatOptions)));
                            else
                                writer.WriteString(prop.GetValue(formatOptions).ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
            }//todo: break this into component functions to serialize xmldocument fragments, then join the fragments at the top level
        }
        public static void ImportFromXml(this Units units, XmlDocument XmlUnitConfig) {
            Log.Debug("READ UNIT CONFIGURATIONS");
            
            XmlReaderSettings settings = new XmlReaderSettings();
            IList<ForgeTypeId> ModifiableSpecs = Units.GetModifiableSpecs();

            // need a better way to cache the XmlUnitConfig.  this should never be a huge amount of data so i 
            // think it's ok to pass this in memory for now, but i don't like it;
            XmlNodeList UnitConfigurations = XmlUnitConfig.GetElementsByTagName("UnitConfiguration");

            foreach (XmlNode UnitConfNode in UnitConfigurations) {
                //ok, lazy way is by expected order of (SpecTypeId,FormatOptions)
                XmlNode FormatOptionNode = UnitConfNode.LastChild;
                XmlNode SpecTypeNode = UnitConfNode.FirstChild;

                //how can we do this without this clumsy initialization.
                string xSymbolTypeId = "", xUnitTypeId = "";
                bool xUseDigitGrouping = false, xUsePlusPrefix = false, xSuppressSpaces = false, xSuppressLeadingZeros = false
                    , xSuppressTrailingZeros = false, xUseDefault = false;
                double xAccuracy = 0.0;

                ForgeTypeId SpecTypeId = new ForgeTypeId(SpecTypeNode.InnerText);
                FormatOptions fo = new FormatOptions();
                
                RoundingMethod xRoundingMethod = RoundingMethod.Nearest;

                Log.Debug($"SpecTypeId:\t{SpecTypeNode.InnerText}");
                Log.Debug("FormatOption properties");

                foreach (XmlNode FoProperty in FormatOptionNode.ChildNodes) {
                    
                    Debug.WriteLine($"{FoProperty.Name}:\t{FoProperty.InnerText}");
                    Log.Debug($"{FoProperty.Name}:\t{FoProperty.InnerText}");
                    XmlAttributeCollection NodeAttributes = FoProperty.Attributes;
                    //ASSUME the first attribute is "type"

                    switch (FoProperty.Name) {
                        case "UnitTypeId":
                            xUnitTypeId = FoProperty.InnerText;
                            break;
                        case "SymbolTypeId":
                            xSymbolTypeId = FoProperty.InnerText;
                            break;
                        case "UseDigitGrouping":
                            xUseDigitGrouping = FoProperty.InnerText == "True";
                            break;
                        case "UsePlusPrefix":
                            xUsePlusPrefix = FoProperty.InnerText == "True";
                            break;
                        case "SuppressSpaces":
                            xSuppressSpaces = FoProperty.InnerText == "True";
                            break;
                        case "SuppressLeadingZeros":
                            xSuppressLeadingZeros = FoProperty.InnerText == "True";
                            break;
                        case "SuppressTrailingZeros":
                            xSuppressTrailingZeros = FoProperty.InnerText == "True";
                            break;
                        case "Accuracy":
                            xAccuracy = float.Parse(FoProperty.InnerText);
                            break;
                        case "RoundingMethod":
                            xRoundingMethod = (RoundingMethod)Convert.ToInt16(FoProperty.InnerText);
                            break;
                        case "UseDefault":
                            xUseDefault = Convert.ToBoolean(FoProperty.InnerText);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"invalid property name: {FoProperty.Name}");
                    }
                }
                //i think this is fine as an "always" action. no need to try and check for FormatOptions equality before changing?
                if (!xUseDefault) {
                    fo.UseDefault = xUseDefault;
                    fo.RoundingMethod = xRoundingMethod;
                    fo.SetUnitTypeId(new ForgeTypeId(xUnitTypeId));
                    fo.SuppressLeadingZeros = xSuppressLeadingZeros;
                    fo.SuppressTrailingZeros = xSuppressTrailingZeros;
                    fo.SuppressSpaces = xSuppressSpaces;
                    fo.UsePlusPrefix = xUsePlusPrefix;
                    fo.UseDigitGrouping = xUseDigitGrouping;
                    if (fo.IsValidAccuracy(xAccuracy)) fo.Accuracy = xAccuracy;
                    if (fo.IsValidSymbol(new ForgeTypeId(xSymbolTypeId))) fo.SetSymbolTypeId(new ForgeTypeId(xSymbolTypeId));
                }
                else {
                    fo.UseDefault = xUseDefault;
                }
                bool check = fo.Equals(units.GetFormatOptions(SpecTypeId));
                Debug.WriteLine($"modifiable spec equal?? {check}");
                if (fo.IsValidForSpec(SpecTypeId)) units.SetFormatOptions(SpecTypeId, fo);
                else Debug.WriteLine($"could not modify spec??");
            }
        }
    }
}
