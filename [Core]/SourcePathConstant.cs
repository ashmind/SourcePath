using System.Text;

namespace SourcePath {
    public class SourcePathConstant<TNode> : ISourcePath<TNode> {
        private readonly ISourceNodeHandler<TNode> _nodeHandler;

        public SourcePathConstant(string value, ISourceNodeHandler<TNode> nodeHandler) {
            Argument.NotNull(nameof(nodeHandler), nodeHandler);

            Value = value;
            _nodeHandler = nodeHandler;
        }

        public string Value { get; }

        public void AppendToPathString(StringBuilder builder) {
            if (Value.Contains("'")) {
                builder.Append('"').Append(Value).Append('"');
                return;
            }
            builder.Append('\'').Append(Value).Append('\'');
        }

        public bool Matches(TNode node, SourcePathAxis defaultAxis) => _nodeHandler.Matches(node, this);
    }
}
