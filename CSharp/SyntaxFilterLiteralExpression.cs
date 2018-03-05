using System;
using System.Collections.Generic;
using System.Text;

namespace SourcePath.CSharp {
    public class SyntaxFilterLiteralExpression : ISyntaxFilterExpression {
        public SyntaxFilterLiteralExpression(string value) {
            Value = value;
        }

        public string Value { get; }

        public void AppendToString(StringBuilder builder) {
            builder.Append("'").Append(Value).Append("'");
        }
    }
}
