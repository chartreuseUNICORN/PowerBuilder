using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public class ClaudeClientOptions {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.anthropic.com";
        public string DefaultModel { get; set; } = "claude-sonnet-4-20250514";
        public int DefaultMaxTokens { get; set; } = 4096;
        public string AnthropicVersion { get; set; } = "2023-06-01";
        public double? Temperature { get; set; } = null;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
