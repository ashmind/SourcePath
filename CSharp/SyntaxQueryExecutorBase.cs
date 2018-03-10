using System;
using System.Collections.Generic;
using System.Linq;

namespace SourcePath.CSharp {
    public abstract class SyntaxQueryExecutorBase<TNodeOrToken, TNodeOrTokenEnumerable, TNode>
        where TNodeOrTokenEnumerable : IEnumerable<TNodeOrToken>
    {
        public IEnumerable<TNodeOrToken> QueryAll(TNodeOrToken current, SyntaxQuery query) {
            switch (query.Axis) {
                case SyntaxQueryAxis.Self:
                    return QuerySelf(current, query);
                case SyntaxQueryAxis.Child:
                    return QueryAllChildrenOrDescendants(current, query, descendants: false);
                case SyntaxQueryAxis.Descendant:
                    return QueryAllChildrenOrDescendants(current, query, descendants: true);
                case SyntaxQueryAxis.Parent:
                    var parent = ToNodeOrToken(GetParent(current));
                    if (MatchesIgnoringAxis(parent, query))
                        return Enumerable.Repeat(parent, 1);
                    return Enumerable.Empty<TNodeOrToken>();
                default:
                    throw new ArgumentException($"Unsupported query axis: {query.Axis}.", nameof(query));
            }
        }

        private IEnumerable<TNodeOrToken> QuerySelf(TNodeOrToken node, SyntaxQuery query) {
            foreach (var self in GetClosestNoJumpOver(node)) {
                if (MatchesIgnoringAxis(self, query))
                    yield return self;
            }
        }

        private IEnumerable<TNodeOrToken> QueryAllChildrenOrDescendants(TNodeOrToken node, SyntaxQuery query, bool descendants) {
            foreach (var child in GetChildren(node)) {
                if (MatchesIgnoringAxis(child, query)) {
                    yield return child;
                    continue;
                }

                if (descendants) {
                    foreach (var descendant in QueryAllChildrenOrDescendants(child, query, descendants: true)) {
                        yield return descendant;
                    }
                }
            }
        }

        private bool MatchesIgnoringAxis(TNodeOrToken node, SyntaxQuery query) {
            if (IsExpressionStatement(node, out var expression))
                return MatchesIgnoringAxis(ToNodeOrToken(expression), query);

            if (IsSwitchSection(node, out var labels)) {
                foreach (var label in labels) {
                    if (MatchesNodeTypeAndFilter(node, ToNodeOrToken(label), query))
                        return true;
                }
            }

            if (IsPredefinedType(node, out var keyword))
                return MatchesNodeTypeAndFilter(node, keyword, query);

            if (HasOtherNodeTypeDefiningChildNode(node, out var nodeTypeChild))
                return MatchesNodeTypeAndFilter(node, nodeTypeChild, query);

            return MatchesNodeTypeAndFilter(node, node, query);
        }

        private bool MatchesNodeTypeAndFilter(TNodeOrToken node, TNodeOrToken typeFromNode, SyntaxQuery query) {
            return MatchesNodeType(typeFromNode, query)
                && MatchesFilter(node, query.Filter);
        }

        private bool MatchesFilter(TNodeOrToken node, ISyntaxFilterExpression filter) {
            if (filter == null)
                return true;
            switch (filter) {
                case SyntaxQuery query:
                    return QueryAll(node, query).Any();

                case SyntaxFilterBinaryExpression binary:
                    return MatchesFilterBinary(node, binary);

                default:
                    throw new NotSupportedException($"Unknown filter type: {filter.GetType()}.");
            }
        }

        private bool MatchesFilterBinary(TNodeOrToken node, SyntaxFilterBinaryExpression binary) {
            switch (binary.Operator) {
                case SyntaxFilterBinaryOperator.And:
                    return MatchesFilter(node, binary.Left)
                        && MatchesFilter(node, binary.Right);

                case SyntaxFilterBinaryOperator.Equals:
                    return EvaluateToString(node, binary.Left)
                        == EvaluateToString(node, binary.Right);

                default:
                    throw new NotSupportedException($"Unknown binary operator: {binary.Operator}.");
            }
        }

        private string EvaluateToString(TNodeOrToken node, ISyntaxFilterExpression expression) {
            switch (expression) {
                case SyntaxQuery query: {
                    var first = QueryAll(node, query).FirstOrDefault();
                    return EvaluateToString(node);
                }

                case SyntaxFilterLiteralExpression literal:
                    return literal.Value;

                default:
                    throw new NotSupportedException($"Expression canno be evaluated to a single value: {expression}.");
            }
        }

        private string EvaluateToString(TNodeOrToken node) {
            return AsIdentifierToString(node);
        }

        private IEnumerable<TNodeOrToken> GetClosestNoJumpOver(TNodeOrToken node) {
            var nodeNotToken = AsNode(node);
            if (nodeNotToken != null && ShouldJumpOver(nodeNotToken))
                return GetChildrenAfterJumpOver(node);
            return Enumerable.Repeat(node, 1);
        }

        private TNode GetParent(TNodeOrToken node) {
            var parent = GetDirectParent(node);
            while (parent != null && ShouldJumpOver(parent)) {
                parent = GetDirectParent(ToNodeOrToken(parent));
            }
            return parent;
        }

        private IEnumerable<TNodeOrToken> GetChildren(TNodeOrToken node) {
            foreach (var noJumpOver in GetClosestNoJumpOver(node)) {
                foreach (var child in GetChildrenAfterJumpOver(noJumpOver)) {
                    yield return child;
                }
            }
        }

        private IEnumerable<TNodeOrToken> GetChildrenAfterJumpOver(TNodeOrToken node) {
            foreach (var child in GetDirectChildren(node)) {
                foreach (var noJumpOver in GetClosestNoJumpOver(child)) {
                    yield return noJumpOver;
                }
            }
        }

        protected abstract bool ShouldJumpOver(TNode node);
        protected abstract TNode GetDirectParent(TNodeOrToken node);
        protected abstract TNodeOrTokenEnumerable GetDirectChildren(TNodeOrToken node);
        protected abstract TNode AsNode(TNodeOrToken node);
        protected abstract TNodeOrToken ToNodeOrToken(TNode node);
        protected abstract bool IsExpressionStatement(TNodeOrToken node, out TNode expression);
        protected abstract bool IsSwitchSection(TNodeOrToken node, out IEnumerable<TNode> labels);
        protected abstract bool IsPredefinedType(TNodeOrToken node, out TNodeOrToken keyword);
        protected virtual bool HasOtherNodeTypeDefiningChildNode(TNodeOrToken node, out TNodeOrToken child) {
            child = default;
            return false;
        }
        protected abstract bool MatchesNodeType(TNodeOrToken node, SyntaxQuery query);
        protected abstract string AsIdentifierToString(TNodeOrToken node);
    }
}
