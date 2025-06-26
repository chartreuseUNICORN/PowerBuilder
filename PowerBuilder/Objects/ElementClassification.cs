using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PowerBuilder.Objects {
    public class ElementClassification {
        [JsonPropertyName("classificationSystem")]
        public string ClassificationSystem { get; set; }

        [JsonPropertyName("classificationNumber")]
        public string ClassificationNumber { get; set; }

        [JsonPropertyName("classificationName")]
        public string ClassificationName { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; }
    }
}
