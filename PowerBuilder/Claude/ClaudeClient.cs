using Autodesk.Revit.DB;
using Autodesk.Windows;
using PowerBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public class ClaudeClient : IClaudeClient {
        private readonly HttpClient _httpClient;
        private readonly ClaudeClientOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly bool _disposeHttpClient;

        public ClaudeClient(ClaudeClientOptions options) : this(new HttpClient(), options, true) {
        }

        public ClaudeClient(HttpClient httpClient, ClaudeClientOptions options, bool disposeHttpClient = false) {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _disposeHttpClient = disposeHttpClient;

            if (string.IsNullOrEmpty(_options.ApiKey))
                throw new ArgumentException("API key is required", nameof(options));

            ConfigureHttpClient();

            _jsonOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        private void ConfigureHttpClient() {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.Timeout = _options.Timeout;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.AnthropicVersion);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ClaudeClient/1.0");
        }

        public ClaudeClientOptions GetClaudeClientOptions() {
            return _options;
        }

        #region Async Methods - Use ConfigureAwait(false) to avoid context capture

        public async Task<ClaudeResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default) {
            var request = new ClaudeRequest {
                Model = _options.DefaultModel,
                MaxTokens = _options.DefaultMaxTokens,
                Messages = new List<ClaudeMessage> {
                    new ClaudeMessage { Role = "user", Content = message }
                }
            };

            return await PostHttpAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetTextResponseAsync(string message, CancellationToken cancellationToken = default) {
            var response = await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            return response.Content?.FirstOrDefault()?.Text ?? string.Empty;
        }

        public async Task<string> GetTextResponseAsync(ClaudeRequest request, CancellationToken cancellationToken = default) {
            var response = await PostHttpAsync(request, cancellationToken).ConfigureAwait(false);
            return response.Content?.FirstOrDefault()?.Text ?? string.Empty;
        }

        public async Task<ClaudeResponse> PostHttpAsync(ClaudeRequest request, CancellationToken cancellationToken = default) {
            try {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use ConfigureAwait(false) to avoid capturing the synchronization context
                var response = await _httpClient.PostAsync("/v1/messages", content, cancellationToken)
                    .ConfigureAwait(false);

                var responseContent = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    throw new ClaudeException(
                        $"Claude API request failed: {response.StatusCode}",
                        (int)response.StatusCode,
                        responseContent);
                }

                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, _jsonOptions);
                return claudeResponse ?? throw new ClaudeException("Failed to deserialize Claude response");
            }
            catch (HttpRequestException ex) {
                throw new ClaudeException("Network error occurred while calling Claude API", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException) {
                throw new ClaudeException("Request to Claude API timed out", ex);
            }
            catch (JsonException ex) {
                throw new ClaudeException("Failed to process Claude API response", ex);
            }
        }

        #endregion

        #region Synchronous Methods - Handle threading internally to prevent UI deadlocks

        /// <summary>
        /// Sends a message synchronously. Safe to call from UI thread
        /// </summary>
        public ClaudeResponse SendMessage(string message, CancellationToken cancellationToken = default) {
            // Run on background thread to prevent UI thread deadlock
            return Task.Run(async () => await SendMessageAsync(message, cancellationToken))
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Sends a request synchronously. Safe to call from UI thread
        /// </summary>
        public ClaudeResponse SendMessage(ClaudeRequest request, CancellationToken cancellationToken = default) {
            // Run on background thread to prevent UI thread deadlock
            return Task.Run(async () => await PostHttpAsync(request, cancellationToken))
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Gets text response synchronously. Safe to call from UI thread
        /// </summary>
        public string GetTextResponse(string message, CancellationToken cancellationToken = default) {
            // Run on background thread to prevent UI thread deadlock
            return Task.Run(async () => await GetTextResponseAsync(message, cancellationToken))
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Gets text response synchronously. Safe to call from UI thread
        /// </summary>
        public string GetTextResponse(ClaudeRequest request, CancellationToken cancellationToken = default) {
            // Run on background thread to prevent UI thread deadlock
            return Task.Run(async () => await GetTextResponseAsync(request, cancellationToken))
                .GetAwaiter()
                .GetResult();
        }

        #endregion

        public void Dispose() {
            if (_disposeHttpClient) {
                _httpClient?.Dispose();
            }
        }
    }
}