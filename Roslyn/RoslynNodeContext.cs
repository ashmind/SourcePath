using System;
using Microsoft.CodeAnalysis;

namespace SourcePath.Roslyn {
    public readonly struct RoslynNodeContext {
        public RoslynNodeContext(SyntaxNodeOrToken nodeOrToken, SemanticModel semanticModel) {
            //Argument.NotNull(nameof(semanticModel), semanticModel);

            NodeOrToken = nodeOrToken;
            SemanticModel = semanticModel;
        }

        public SyntaxNodeOrToken NodeOrToken { get; }
        public SemanticModel SemanticModel { get; }

        public bool IsToken => NodeOrToken.IsToken;
        public SyntaxToken AsToken() => NodeOrToken.AsToken();
        public SyntaxNode AsNode() => NodeOrToken.AsNode();

        public RoslynNodeContext? GetParent() {
            if (NodeOrToken.Parent == null)
                return null;
            return new RoslynNodeContext(NodeOrToken.Parent, SemanticModel);
        }
    }
}
