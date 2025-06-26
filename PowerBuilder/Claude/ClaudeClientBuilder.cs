using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public class ClaudeClientBuilder {
        private readonly ClaudeClientOptions _options = new();
        private HttpClient _httpClient;

        public ClaudeClientBuilder WithApiKey(string apiKey) {
            _options.ApiKey = apiKey;
            return this;
        }

        public ClaudeClientBuilder WithModel(string model) {
            _options.DefaultModel = model;
            return this;
        }

        public ClaudeClientBuilder WithMaxTokens(int maxTokens) {
            _options.DefaultMaxTokens = maxTokens;
            return this;
        }

        public ClaudeClientBuilder WithTimeout(TimeSpan timeout) {
            _options.Timeout = timeout;
            return this;
        }

        public ClaudeClientBuilder WithHttpClient(HttpClient httpClient) {
            _httpClient = httpClient;
            return this;
        }

        public ClaudeClient Build() {
            if (_httpClient != null) {
                return new ClaudeClient(_httpClient, _options);
            }
            return new ClaudeClient(_options);
        }
    }
}
