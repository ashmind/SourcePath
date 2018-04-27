using System.Text;

namespace SourcePath {
    public interface ISourceNodeKind<TNode> {
        bool Matches(TNode node);
        string ToPathString();
    }
}