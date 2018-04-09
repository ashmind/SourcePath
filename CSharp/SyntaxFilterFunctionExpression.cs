using System.Collections.Generic;
using System.Text;

namespace SourcePath.CSharp {
    public class SyntaxFilterFunctionExpression : ISyntaxFilterExpression {
        private static readonly IReadOnlyDictionary<SyntaxPathFunction, string> FunctionNames = new Dictionary<SyntaxPathFunction, string> {
            { SyntaxPathFunction.Kind, "kind" }
        };

        public SyntaxFilterFunctionExpression(SyntaxPathFunction function) {
            Function = function;
        }

        public SyntaxPathFunction Function { get; }

        public void AppendToString(StringBuilder builder) {
            builder.Append(FunctionNames[Function]).Append("()");
        }
    }
}
