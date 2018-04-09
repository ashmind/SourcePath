using System.Collections.Generic;
using System.Text;

namespace SourcePath.CSharp {
    public class SyntaxPath : ISyntaxFilterExpression {
        public SyntaxPath(IReadOnlyList<SyntaxPathSegment> segments) {
            Argument.NotNullOrEmpty(nameof(segments), segments);
            Segments = segments;
        }

        public IReadOnlyList<SyntaxPathSegment> Segments { get; }

        public override string ToString() {
            var builder = new StringBuilder();
            AppendToString(builder);
            return builder.ToString();
        }

        public void AppendToString(StringBuilder builder) {
            var first = true;
            foreach (var segment in Segments) {
                if (!first)
                    builder.Append("/");
                builder.Append(segment);
                first = false;
            }
        }
    }
}