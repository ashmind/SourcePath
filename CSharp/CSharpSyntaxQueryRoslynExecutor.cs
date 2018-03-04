using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lastql.CSharp {
    public class CSharpSyntaxQueryRoslynExecutor {
        public IEnumerable<SyntaxNodeOrToken> QueryAll(CSharpSyntaxNode current, CSharpSyntaxQuery query) {
            if (current is CompilationUnitSyntax)
                return current.ChildNodes().SelectMany(c => QueryAll((CSharpSyntaxNode)c, query));

            switch (query.Axis) {
                case SyntaxQueryAxis.Self:
                    if (MatchesIgnoringAxis(current, query))
                        return Enumerable.Repeat((SyntaxNodeOrToken)current, 1);
                    return Enumerable.Empty<SyntaxNodeOrToken>();
                case SyntaxQueryAxis.Child:
                    return QueryAllChildrenOrDescendants(current, query, descendants: false);
                case SyntaxQueryAxis.Descendant:
                    return QueryAllChildrenOrDescendants(current, query, descendants: true);
                default:
                    throw new ArgumentException($"Unsupported query axis: {query.Axis}.", nameof(query));
            }
        }

        private IEnumerable<SyntaxNodeOrToken> QueryAllChildrenOrDescendants(SyntaxNode node, CSharpSyntaxQuery query, bool descendants) {
            foreach (var child in node.ChildNodesAndTokens()) {
                if (MatchesIgnoringAxis(child, query)) {
                    yield return child;
                    continue;
                }

                if (descendants && child.IsNode) {
                    foreach (var descendant in QueryAllChildrenOrDescendants(child.AsNode(), query, descendants: true)) {
                        yield return descendant;
                    }
                }
            }
        }

        private static bool MatchesIgnoringAxis(SyntaxNodeOrToken child, CSharpSyntaxQuery query) {
            var node = child.AsNode();
            if (node is ExpressionStatementSyntax statement && node.Kind() == SyntaxKind.ExpressionStatement)
                return MatchesIgnoringAxis(statement.Expression, query);
            
            if (node is SwitchSectionSyntax switchSection) {
                foreach (var label in switchSection.Labels) {
                    if (query.MatchesSyntaxKind(label.Kind()))
                        return true;
                }
            }

            return query.MatchesSyntaxKind(child.Kind());
        }
    }
}
