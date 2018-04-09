using System.Text;

namespace SourcePath.CSharp {
    public class SyntaxPathSegment {
        public SyntaxPathSegment(SyntaxPathAxis axis, SyntaxPathKeyword keyword, ISyntaxFilterExpression filter) {
            Axis = axis;
            Keyword = keyword;
            Filter = filter;
        }

        public SyntaxPathAxis Axis { get; }
        public SyntaxPathKeyword Keyword { get; }
        public ISyntaxFilterExpression Filter { get; }

        public override string ToString() {
            var builder = new StringBuilder();
            AppendToString(builder);
            return builder.ToString();
        }

        public void AppendToString(StringBuilder builder) {
            switch (Axis) {
                case SyntaxPathAxis.Self: builder.Append("self::"); break;
                case SyntaxPathAxis.Descendant: builder.Append("//"); break;
                case SyntaxPathAxis.Parent: builder.Append("parent::"); break;
            }
            builder.Append(Keyword != SyntaxPathKeyword.Star ? Keyword.ToString("G").ToLowerInvariant() : "*");
            if (Filter != null) {
                builder.Append("[");
                Filter.AppendToString(builder);
                builder.Append("]");
            }
        }
    }
}
