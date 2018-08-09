using System;
using System.Text;

namespace SourcePath {
    public interface ISourcePath<TNode> {
        bool Matches(TNode node, SourcePathAxis defaultAxis);
        void AppendToPathString(StringBuilder builder);
    }
}
