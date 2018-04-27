using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SourcePath.Roslyn {
    public class RoslynAxisNavigator : ISourcePathAxisNavigator<SyntaxNodeOrToken> {
        public IEnumerable<SyntaxNodeOrToken> Navigate(SyntaxNodeOrToken node, SourcePathAxis axis) {
            switch (axis) {
                case SourcePathAxis.Self:
                    yield return node;
                    yield break;

                case SourcePathAxis.Child:
                    foreach (var child in node.ChildNodesAndTokens()) {
                        yield return child;
                    }
                    yield break;

                case SourcePathAxis.Parent:
                    if (node.Parent != null)
                        yield return node.Parent;
                    yield break;

                case SourcePathAxis.Descendant:
                    if (node.IsToken)
                        yield break;
                    foreach (var descendant in node.AsNode().DescendantNodesAndTokens()) {
                        yield return descendant;
                    }
                    yield break;

                case SourcePathAxis.DescendantOrSelf:
                    yield return node;
                    if (node.IsToken)
                        yield break;
                    foreach (var descendant in node.AsNode().DescendantNodesAndTokens()) {
                        yield return descendant;
                    }
                    yield break;
            }
        }
    }
}
