using SourcePath.CSharp;

namespace SourcePath.Configuration {
    public class SyntaxRule {
        public SyntaxRule(string id, SyntaxQuery query, SyntaxRuleSeverity severity, string message) {
            Id = id;
            Query = query;
            Severity = severity;
            Message = message;
        }

        public string Id { get; }
        public SyntaxQuery Query { get; }
        public SyntaxRuleSeverity Severity { get; }
        public string Message { get; }
    }
}
