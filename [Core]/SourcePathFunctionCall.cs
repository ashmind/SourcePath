using System;
using System.Collections.Generic;
using System.Text;

namespace SourcePath {
    public class SourcePathFunctionCall<TNode> : ISourcePath<TNode> {
        private readonly ISourceNodeHandler<TNode> _nodeHandler;

        public SourcePathFunctionCall(
            string functionName,
            IReadOnlyList<SourcePathConstant<TNode>> arguments,
            ISourceNodeHandler<TNode> nodeHandler
        ) {
            Argument.NotNullOrEmpty(nameof(functionName), functionName);
            Argument.NotNull(nameof(arguments), arguments);
            Argument.NotNull(nameof(nodeHandler), nodeHandler);

            FunctionName = functionName;
            Arguments = arguments;
            _nodeHandler = nodeHandler;
        }

        public string FunctionName { get; }
        public IReadOnlyList<SourcePathConstant<TNode>> Arguments { get; }

        public void AppendToPathString(StringBuilder builder) {
            builder.Append(".").Append(FunctionName).Append("(");
            foreach (var argument in Arguments) {
                argument.AppendToPathString(builder);
            }
            builder.Append(")");
        }

        public bool Matches(TNode node, SourcePathAxis defaultAxis) {
            switch (FunctionName) {
                case nameof(string.StartsWith):
                    var value = _nodeHandler.GetStringValueOrDefault(node);
                    return value?.StartsWith(Arguments[0].Value) ?? false;
                default:
                    throw new NotSupportedException($"Unknown function: ${FunctionName}");
            }
        }
    }
}
