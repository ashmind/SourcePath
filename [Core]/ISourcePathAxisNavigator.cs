using System;
using System.Collections.Generic;
using System.Text;

namespace SourcePath {
    public interface ISourcePathAxisNavigator<TNode> {
        IEnumerable<TNode> Navigate(TNode node, SourcePathAxis axis);
    }
}
