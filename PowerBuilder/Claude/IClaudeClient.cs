using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public interface IClaudeClient : IDisposable {
        Task<ClaudeResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default);
        Task<ClaudeResponse> PostHttpAsync(ClaudeRequest request, CancellationToken cancellationToken = default);
        Task<string> GetTextResponseAsync(string message, CancellationToken cancellationToken = default);
    }
}
