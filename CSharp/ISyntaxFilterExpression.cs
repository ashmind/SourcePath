using System.Text;

namespace SourcePath.CSharp {
    public interface ISyntaxFilterExpression {
        void AppendToString(StringBuilder builder);
    }
}
