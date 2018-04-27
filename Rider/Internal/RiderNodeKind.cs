using System.Collections.Generic;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace SourcePath.Rider.Internal {
    public class RiderNodeKind : ISourceNodeKind<ITreeNode> {
        private readonly ISet<NodeType> _types;

        public RiderNodeKind(string name, ISet<NodeType> types) {
            Name = name;
            _types = Argument.NotNullOrEmpty(nameof(types), types);
        }

        public string Name { get; }
        public bool Matches(ITreeNode node) => _types.Contains(node.NodeType);

        public string ToPathString() => Name;
        public override string ToString() => Name;
    }
}
