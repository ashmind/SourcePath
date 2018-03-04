using System;
using System.Text;

namespace Lastql.CSharp {
    public class SyntaxQuery : ISyntaxFilterExpression {
        public SyntaxQuery(SyntaxQueryAxis axis, SyntaxQueryKeyword keyword, ISyntaxFilterExpression filter) {
            Axis = axis;
            Keyword = keyword;
            Filter = filter;
        }

        public SyntaxQueryAxis Axis { get; }
        public SyntaxQueryKeyword Keyword { get; }
        public ISyntaxFilterExpression Filter { get; }

        public override string ToString() {
            var builder = new StringBuilder();
            AppendToString(builder);
            return builder.ToString();
        }

        public void AppendToString(StringBuilder builder) {
            switch (Axis) {
                case SyntaxQueryAxis.Self: builder.Append("self::"); break;
                case SyntaxQueryAxis.Descendant: builder.Append("//"); break;
            }
            builder.Append(Keyword.ToString("G").ToLowerInvariant());
            if (Filter != null) {
                builder.Append("[");
                Filter.AppendToString(builder);
                builder.Append("]");
            }
        }
    }
}