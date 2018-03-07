using System;
using System.Collections.Generic;
using System.Text;
using SourcePath.CSharp;

namespace SourcePath.Analyzers {
    public class Rule {
        public Rule(string id, SyntaxQuery query, RuleSeverity severity, string message) {
            Id = id;
            Query = query;
            Severity = severity;
            Message = message;
        }

        public string Id { get; }
        public SyntaxQuery Query { get; }
        public RuleSeverity Severity { get; }
        public string Message { get; }
    }
}
