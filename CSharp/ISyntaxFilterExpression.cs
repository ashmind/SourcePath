using System.Text;

namespace Lastql.CSharp {
    public interface ISyntaxFilterExpression {
        void AppendToString(StringBuilder builder);
    }
}
