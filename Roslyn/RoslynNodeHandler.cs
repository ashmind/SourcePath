using System;
using System.Collections.Generic;

namespace SourcePath.Roslyn {
    public class RoslynNodeHandler : ISourceNodeHandler<RoslynNodeContext> {
        public IEnumerable<RoslynNodeContext> Navigate(RoslynNodeContext node, SourcePathAxis axis) {
            switch (axis) {
                case SourcePathAxis.Self:
                    yield return node;
                    yield break;

                case SourcePathAxis.Child:
                    foreach (var child in node.NodeOrToken.ChildNodesAndTokens()) {
                        yield return new RoslynNodeContext(child, node.SemanticModel);
                    }
                    yield break;

                case SourcePathAxis.Parent:
                    var parent = node.GetParent();
                    if (parent != null)
                        yield return parent.Value;
                    yield break;

                case SourcePathAxis.Descendant:
                    if (node.IsToken)
                        yield break;
                    foreach (var descendant in node.AsNode().DescendantNodesAndTokens()) {
                        yield return new RoslynNodeContext(descendant, node.SemanticModel);
                    }
                    yield break;

                case SourcePathAxis.DescendantOrSelf:
                    yield return node;
                    if (node.IsToken)
                        yield break;
                    foreach (var descendant in node.AsNode().DescendantNodesAndTokens()) {
                        yield return new RoslynNodeContext(descendant, node.SemanticModel);
                    }
                    yield break;
            }
        }

        public bool Matches(RoslynNodeContext node, SourcePathConstant<RoslynNodeContext> constant) {
            throw new NotImplementedException();
        }

        public string GetStringValueOrDefault(RoslynNodeContext node) {
            if (!node.IsToken)
                return null;

            return node.AsToken().Value as string;
        }
    }
}
