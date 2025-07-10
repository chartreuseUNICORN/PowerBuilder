using Autodesk.Revit.DB;
using PowerBuilder.Claude;
using PowerBuilder.Extensions;
using PowerBuilder.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PowerBuilder.Services {
    public class ElementClassifier{
        private ClaudeClient _cc;
        private ClaudeRequest _cRequest;
        private SpecCulture _specCulture;
        public ElementClassifier (SpecCulture culture) {

            _cc = ClaudeConnector.Instance.Client;

            ClaudeRequest cQuery = new ClaudeRequest(_cc.GetClaudeClientOptions());
            cQuery.MaxTokens = 1024;
            cQuery.System = "You are an expert on the built environment in all aspects of Architecture, Structural Engineering, Mechanical Engineering, " +
                "Electrical Engineering, Plumbing Engineering, Telecommunications Design, and Construction Engineering with a deep understanding of " +
                "object classification, especially as it relates to digital representation of components.\n"+
                $"classify elements based on the Specification Culture: {culture.Name}\n"+
                culture.ToJson()+"\n\n"+
@"Return JSON format:
{
    ""classificationNumber"": ""XX.XX.XX"",
    ""classificationName"": ""Description"",
    ""elementName"":""My Element Name"",
    ""elementNumber"":""11223344"",
    ""confidence"": 0.95,
    ""comments"": ""Brief explanation""
}

Evaluate all the data provided about the element including inheritance hierarchy, category, and parameter values as a basis for your answer    
Do not include any markdown formatting or code blocks. Return only the JSON object.";

            _specCulture = culture;
            _cRequest = cQuery;
        }
        public ElementClassification Classify(Element e) {
            ClaudeMessage cMessage = BuildPrompt(e.ToJson());
            _cRequest.Messages.Add(cMessage);

            string response = _cc.GetTextResponseAsync(_cRequest).Result.Trim();
            ElementClassification elementClassification = JsonSerializer.Deserialize<ElementClassification>(response);

            return elementClassification;
        }
        private static ClaudeMessage BuildPrompt(string elementJson) {
            ClaudeMessage cMessage = new ClaudeMessage();
            cMessage.Role = "user";

            string basePrompt = "Please return a classification for an element identified with this data";

            cMessage.Content = $"{basePrompt}\n\n" +
                $"Element Data to Classify:\n{elementJson}\n\n" +
                "Provide your classification response as JSON only.";

            return cMessage;
        }
    }
}
