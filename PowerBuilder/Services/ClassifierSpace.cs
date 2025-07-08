using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Mechanical;
using PowerBuilder.Claude;
using PowerBuilder.Commands;
using PowerBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PowerBuilder.Objects;

namespace PowerBuilder.Services {
    public class ClassifierSpace {
        public static ElementClassification ClassifySpaceTypeByRoom (Element e, Document doc) {

            ClaudeClient cc = ClaudeConnector.Instance.Client;
            //this is like the SpecCulture 
            Dictionary<long, string> spaceTypeDefs = new FilteredElementCollector(doc).OfClass(typeof(HVACLoadSpaceType)).ToElements().ToDictionary(x => x.Id.Value, x => x.Name);

            ClaudeRequest cQuery = new ClaudeRequest(cc.GetClaudeClientOptions());
            cQuery.MaxTokens = 1024;
            cQuery.System =
@"You are a building commissioning and thermal analysis expert.  Determine a Space Type classification based on the provided data

Return a JSON format:
{
    ""classificationNumber"":""XXXX"",
    ""classificationName"":""Corridor"",
    ""elementName"":""RoomName"",
    ""elementNumber"":""11223344"",
    ""confidence"":0.95
}
Evaluate all the data provided about the element including inheritance hierarchy, category, and parameter values as a basis for your answer    
Do not include any markdown formatting or code blocks. Return only the JSON object."";
";
            ClaudeMessage cMessage = BuildPrompt(e.ToJson(), spaceTypeDefs);
            cQuery.Messages.Add(cMessage);

            string response = cc.GetTextResponseAsync(cQuery).Result.Trim();
            ElementClassification eClass = JsonSerializer.Deserialize<ElementClassification>(response);

            return eClass;
        }

        private static ClaudeMessage BuildPrompt (string elementJson, Dictionary<long,string> spaceTypeNames) {
            ClaudeMessage cMessage = new ClaudeMessage();
            cMessage.Role = "user";
            string HvacLoadSpaceTypeJson = JsonSerializer.Serialize(spaceTypeNames);
            string basePrompt = "Please return a classification for an element defined by this data";

            cMessage.Content = $"{basePrompt}\n\n" +
                $"Room Data:\n{elementJson}\n\n" +
                "Select from the following HVACLoadSpaceType definitions found in the document:\n"+
                $"{HvacLoadSpaceTypeJson}\n"+
                "Provide your classification response as JSON only";

            return cMessage;
        }
    }
}
