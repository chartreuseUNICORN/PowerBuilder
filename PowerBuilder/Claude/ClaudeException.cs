using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Claude {
    public class ClaudeException : Exception {
        public int? StatusCode { get; }
        public string ResponseContent { get; }

        public ClaudeException(string message) : base(message) { }

        public ClaudeException(string message, int statusCode, string responseContent)
            : base(message) {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public ClaudeException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
