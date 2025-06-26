using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public class ClaudeRequest {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("messages")]
        public List<ClaudeMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; }
        public ClaudeRequest() { }
        public ClaudeRequest(ClaudeClientOptions op) {
            Model = op.DefaultModel;
            MaxTokens = op.DefaultMaxTokens;
        }
    }

    
}
