using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PowerBuilder.Claude;
using System;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    /// <summary>
    /// Singleton connector that provides persistent access to ClaudeClient across Revit commands
    /// </summary>
    public sealed class ClaudeConnector : IDisposable {
        private static readonly Lazy<ClaudeConnector> _instance = new Lazy<ClaudeConnector>(() => new ClaudeConnector());
        private readonly ClaudeClient _claudeClient;
        private static string _apiKey;
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of ClaudeConnector
        /// </summary>
        public static ClaudeConnector Instance => _instance.Value;

        /// <summary>
        /// Gets the ClaudeClient instance for sending/receiving messages
        /// </summary>
        public ClaudeClient Client => _claudeClient;

        /// <summary>
        /// Initialize the ClaudeConnector with API key during application startup
        /// </summary>
        /// <param name="apiKey">The Claude API key</param>
        public static void Initialize(string apiKey) {
            lock (_lock) {
                if (_isInitialized)
                    return; // Already initialized, ignore subsequent calls

                if (string.IsNullOrEmpty(apiKey))
                    throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

                _apiKey = apiKey;
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Private constructor - creates the ClaudeClient instance
        /// </summary>
        private ClaudeConnector() {
            if (!_isInitialized)
                throw new InvalidOperationException("ClaudeConnector must be initialized before use. Call Initialize() first.");

            try {
                var options = new ClaudeClientOptions {
                    ApiKey = _apiKey,
                    BaseUrl = "https://api.anthropic.com",
                    DefaultModel = "claude-sonnet-4-20250514",
                    DefaultMaxTokens = 4096,
                    AnthropicVersion = "2023-06-01",
                    Timeout = TimeSpan.FromMinutes(2)
                };

                _claudeClient = new ClaudeClient(options);
            }
            catch (Exception ex) {
                throw new InvalidOperationException("Failed to initialize Claude client", ex);
            }
        }

        /// <summary>
        /// Check if the connector is properly initialized and ready to use
        /// </summary>
        public bool IsInitialized => _isInitialized && _claudeClient != null;

        /// <summary>
        /// Dispose of the ClaudeClient and clean up resources
        /// </summary>
        public void Dispose() {
            _claudeClient?.Dispose();
        }
    }
}