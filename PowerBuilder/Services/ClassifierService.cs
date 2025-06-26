using PowerBuilder.Claude;
using PowerBuilder.Enums;
using PowerBuilder.Extensions;
using PowerBuilder.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace PowerBuilder.Services {
    public class ClassifierService {
        
        public static ElementClassification ClassifyElement(Element element, SpecCulture culture = SpecCulture.OmniClass) {

            ClaudeClient cc = ClaudeConnector.Instance.Client;

            ClaudeRequest cQuery = new ClaudeRequest(cc.GetClaudeClientOptions());
            cQuery.MaxTokens = 1024;
            cQuery.System = 
@"You are a building commissioning and BIM expert. Classify this Revit element using MasterFormat, OmniClass, or UniFormat II.

Return JSON format:
{
    ""classificationSystem"": ""MasterFormat"",
    ""classificationNumber"": ""XX.XX.XX"",
    ""classificationName"": ""Description"",
    ""confidence"": 0.95,
    ""comments"": ""Brief explanation""
}
Classification Guidelines:
- MasterFormat: Use 6-digit codes (classificationNumber: ""03 30 00"", classificationTitle: ""Cast-in-Place Concrete"")
- OmniClass: Use Table 23 Element codes (classificationNumber: ""23-15 11 11"", classificationTitle: ""Concrete Structural Walls"")
- Uniformat: Use Level 3-4 codes (classificationNumber: ""B2010.10"", classificationTitle: ""Exterior Walls"")

Evaluate all the data provided about the element including inheritance hierarchy, category, and parameter values as a basis for your answer    
Do not include any markdown formatting or code blocks. Return only the JSON object.";

            ClaudeMessage cMessage = BuildPrompt(element.ToJson(), culture);
            cQuery.Messages.Add(cMessage);

            string response = cc.GetTextResponseAsync(cQuery).Result.Trim();

            ElementClassification elementClassification = JsonSerializer.Deserialize<ElementClassification>(response);
            //bool check = ValidateClassification(elementClassification, culture);

            return elementClassification;
        }

        private static ClaudeMessage BuildPrompt(string elementJson, SpecCulture culture) {
            ClaudeMessage cMessage = new ClaudeMessage();
            cMessage.Role = "user";

            string basePrompt = "Please return a classification for an element identified with this data";

            var cultureInstruction = culture switch {
                SpecCulture.MasterFormat => "Classify using MasterFormat 2020 specification sections.",
                SpecCulture.OmniClass => "Classify using OmniClass Table 23 (Elements) classification.",
                SpecCulture.Uniformat => "Classify using Uniformat II elemental classification system.",
                _ => "Classify using MasterFormat 2020 specification sections."
            };

            cMessage.Content = $"{basePrompt}\n\n{cultureInstruction}" +
                $"Element Data to CLassify:\n{elementJson}\n\n" +
                "Provide your classification response as JSON only.";

            return cMessage;
        }
        private static bool ValidateClassification (ElementClassification elemClass, SpecCulture culture) {
            Dictionary<string, string> SpecCultureMap = GetSpecCultureMap(culture);
            if (SpecCultureMap.ContainsKey(elemClass.ClassificationNumber))
                if (SpecCultureMap[elemClass.ClassificationNumber] == elemClass.ClassificationName)
                    return true;
            return false;
        }
        private static Dictionary<string,string> GetSpecCultureMap (SpecCulture culture) {
            
            string path = culture switch {
                SpecCulture.MasterFormat => "C:\\Users\\mclough\\source\\repos\\PowerBuilder\\PowerBuilder\\ReferenceFiles\\MasterFormat.csv",
                SpecCulture.Uniformat => "C:\\Users\\mclough\\source\\repos\\PowerBuilder\\PowerBuilder\\ReferenceFiles\\UniFormat.csv",
                _ => "C:\\Users\\mclough\\source\\repos\\PowerBuilder\\PowerBuilder\\ReferenceFiles\\OmniClass.csv"
            };

            Dictionary<string, string> SpecCultureMap =
                File.ReadLines(path)
                    .Select(line => line.Split(','))
                    .ToDictionary(gr => gr[0],
                                  gr => gr[1]);

            return SpecCultureMap;
        }
    }
}
