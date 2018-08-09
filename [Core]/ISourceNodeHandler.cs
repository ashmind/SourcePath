using System.Collections.Generic;

namespace SourcePath {
    public interface ISourceNodeHandler<TNode> {
        IEnumerable<TNode> Navigate(TNode node, SourcePathAxis axis);
        bool Matches(TNode node, SourcePathConstant<TNode> constant);
        string GetStringValueOrDefault(TNode node);
    }
}
