using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerBuilder.Extensions {
    public static class ElementExtension {

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static bool IsSameOrSubclass(this Element e, Type Candidate) {
            return e.GetType().IsSubclassOf(Candidate) || Candidate == e.GetType();
        }

        public static string ToJson(this Element e) {
            try {
                var data = new {
                    ElementId = e.Id.Value,
                    Name = e.Name ?? "Unnamed",
                    Document = e.Document?.Title ?? "Unknown Document",
                    Category = e.Category?.Name ?? "Unknown",
                    ClassHierarchy = GetClassHierarchy(e.GetType()),
                    Parameters = ExtractParameters(e)
                };

                return JsonSerializer.Serialize(data, JsonOptions);
            }
            catch (Exception ex) {
                // Fallback serialization if extraction fails
                var fallbackData = new {
                    ElementId = e.Id.Value,
                    Name = e.Name ?? "Unnamed",
                    Document = e.Document?.Title ?? "Unknown Document",
                    Category = e.Category?.Name ?? "Unknown",
                    ClassHierarchy = new[] { e.GetType().Name },
                    Parameters = new Dictionary<string, string>(),
                    Error = $"Failed to extract data: {ex.Message}"
                };
                return JsonSerializer.Serialize(fallbackData, JsonOptions);
            }
        }

        private static Dictionary<string, string> ExtractParameters(Element e) {
            var parameters = new Dictionary<string, string>();

            try {
                foreach (Parameter param in e.Parameters) {
                    if (param == null || !param.HasValue) continue;

                    var paramName = param.Definition?.Name;
                    if (string.IsNullOrEmpty(paramName)) continue;

                    // Get parameter value as string
                    var value = GetParameterValueAsString(param);
                    if (!string.IsNullOrEmpty(value)) {
                        parameters[paramName] = value;
                    }
                }
            }
            catch (Exception ex) {
                parameters["ParameterExtractionError"] = ex.Message;
            }

            return parameters;
        }

        private static string GetParameterValueAsString(Parameter param) {
            try {
                return param.AsValueString() ?? string.Empty;
            }
            catch {
                return string.Empty;
            }
        }

        private static List<string> GetClassHierarchy(Type type) {
            var hierarchy = new List<string>();
            while (type != null && type != typeof(object)) {
                hierarchy.Add(type.Name);
                type = type.BaseType;
            }
            return hierarchy;
        }
    }
}