using System;
using System.Collections.Generic;
using System.Linq;

namespace SourcePath.CSharp {
    public abstract class SyntaxQueryExecutorBase<TNodeOrToken, TNodeOrTokenEnumerable, TNode>
        where TNodeOrTokenEnumerable : IEnumerable<TNodeOrToken>
    {
        public bool Matches(TNodeOrToken current, SyntaxPathSegment path) {
            if (path.Axis != SyntaxPathAxis.Default)
                throw new ArgumentException($"Matches() supports Default axis only (got {path.Axis}).", nameof(path));

            return MatchesIgnoringAxis(current, path);
        }

        public IEnumerable<TNodeOrToken> QueryAll(TNodeOrToken current, SyntaxPath path) {
            var last = Enumerable.Repeat(current, 1);
            foreach (var segment in path.Segments) {
                last = QueryAllBySegment(last, segment);
            }
            return last;
        }

        private IEnumerable<TNodeOrToken> QueryAllBySegment(IEnumerable<TNodeOrToken> current, SyntaxPathSegment segment) {
            foreach (var currentItem in current) {
                foreach (var result in QueryAllBySegment(currentItem, segment)) {
                    yield return result;
                }
            }
        }

        private IEnumerable<TNodeOrToken> QueryAllBySegment(TNodeOrToken current, SyntaxPathSegment segment) {
            switch (segment.Axis) {
                case SyntaxPathAxis.Self:
                    return QuerySelf(current, segment);
                case SyntaxPathAxis.Child:
                case SyntaxPathAxis.Default:
                    return QueryAllChildrenOrDescendants(current, segment, descendants: false);
                case SyntaxPathAxis.Descendant:
                    return QueryAllChildrenOrDescendants(current, segment, descendants: true);
                case SyntaxPathAxis.DescendantOrSelf:
                    // TODO: Improve 
                    return QuerySelf(current, segment)
                        .Concat(QueryAllChildrenOrDescendants(current, segment, descendants: true));
                case SyntaxPathAxis.Parent:
                    var parent = ToNodeOrToken(GetParent(current));
                    if (MatchesIgnoringAxis(parent, segment))
                        return Enumerable.Repeat(parent, 1);
                    return Enumerable.Empty<TNodeOrToken>();
                case SyntaxPathAxis.Ancestor:
                    return QueryAllAncestors(current, segment);
                default:
                    throw new ArgumentException($"Unsupported axis: {segment.Axis}.", nameof(segment));
            }
        }

        private IEnumerable<TNodeOrToken> QuerySelf(TNodeOrToken nodeOrToken, SyntaxPathSegment query) {
            var node = AsNode(nodeOrToken);
            if (node != null && ShouldJumpOver(node)) {
                foreach (var child in GetChildren(nodeOrToken, noFirstJump: true)) {
                    if (MatchesIgnoringAxis(child, query))
                        yield return child;
                }
                yield break;
            }

            if (MatchesIgnoringAxis(nodeOrToken, query))
                yield return nodeOrToken;
        }

        private IEnumerable<TNodeOrToken> QueryAllChildrenOrDescendants(TNodeOrToken node, SyntaxPathSegment query, bool descendants) {
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

        private IEnumerable<TNodeOrToken> QueryAllAncestors(TNodeOrToken current, SyntaxPathSegment segment) {
            var parent = ToNodeOrToken(GetParent(current));
            while (Exists(parent)) {
                if (MatchesIgnoringAxis(parent, segment))
                    yield return parent;
                parent = ToNodeOrToken(GetParent(parent));
            }
        }

        private bool MatchesIgnoringAxis(TNodeOrToken node, SyntaxPathSegment segment) {
            if (IsExpressionStatement(node, out var expression))
                return MatchesIgnoringAxis(ToNodeOrToken(expression), segment);

            if (IsSwitchSection(node, out var labels)) {
                foreach (var label in labels) {
                    if (MatchesNodeTypeAndFilter(node, ToNodeOrToken(label), segment))
                        return true;
                }
            }

            if (IsPredefinedType(node, out var keyword))
                return MatchesNodeTypeAndFilter(node, keyword, segment);

            if (HasOtherNodeTypeDefiningChildNode(node, out var nodeTypeChild))
                return MatchesNodeTypeAndFilter(node, nodeTypeChild, segment);

            return MatchesNodeTypeAndFilter(node, node, segment);
        }

        private bool MatchesNodeTypeAndFilter(TNodeOrToken node, TNodeOrToken typeFromNode, SyntaxPathSegment segment) {
            return MatchesNodeType(typeFromNode, segment)
                && MatchesFilter(node, segment.Filter);
        }

        private bool MatchesFilter(TNodeOrToken node, ISyntaxFilterExpression filter) {
            if (filter == null)
                return true;
            switch (filter) {
                case SyntaxPath path:
                    return QueryAll(node, path).Any();

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

                case SyntaxFilterBinaryOperator.Or:
                    return MatchesFilter(node, binary.Left)
                        || MatchesFilter(node, binary.Right);

                case SyntaxFilterBinaryOperator.Equals:
                    return EvaluateToString(node, binary.Left)
                        == EvaluateToString(node, binary.Right);

                default:
                    throw new NotSupportedException($"Unknown binary operator: {binary.Operator}.");
            }
        }

        private string EvaluateToString(TNodeOrToken node, ISyntaxFilterExpression expression) {
            switch (expression) {
                case SyntaxPath path: {
                    var first = QueryAll(node, path).FirstOrDefault();
                    return EvaluateToString(first);
                }

                case SyntaxFilterFunctionExpression function:
                    return EvaluateFunction(function, node);

                case SyntaxFilterLiteralExpression literal:
                    return literal.Value;

                default:
                    throw new NotSupportedException($"Expression cannot be evaluated to a single value: {expression}.");
            }
        }

        private string EvaluateToString(TNodeOrToken node) {
            return AsIdentifierToString(node);
        }

        private string EvaluateFunction(SyntaxFilterFunctionExpression function, TNodeOrToken current) {
            switch (function.Function) {
                case SyntaxPathFunction.Kind:
                    return EvaluateKindFunction(current);

                default:
                    throw new NotSupportedException($"Unknown function: {function.Function}.");
            }
        }

        private TNode GetParent(TNodeOrToken node) {
            var parent = GetDirectParent(node);
            while (parent != null && ShouldJumpOver(parent)) {
                parent = GetDirectParent(ToNodeOrToken(parent));
            }
            return parent;
        }

        private IEnumerable<TNodeOrToken> GetChildren(TNodeOrToken nodeOrToken, bool noFirstJump = false) {
            var node = AsNode(nodeOrToken);
            if (!noFirstJump && node != null && ShouldJumpOver(node)) {
                foreach (var jump in GetDirectChildren(nodeOrToken)) {
                    foreach (var child in GetChildren(jump)) {
                        yield return child;
                    }
                }
                yield break;
            }

            foreach (var child in GetDirectChildren(nodeOrToken)) {
                var childNode = AsNode(child);
                if (childNode != null && ShouldJumpOver(childNode)) {
                    foreach (var jumpChild in GetChildren(child, noFirstJump: true)) {
                        yield return jumpChild;
                    }
                    continue;
                }
                yield return child;
            }
        }

        protected abstract bool ShouldJumpOver(TNode node);
        protected abstract TNode GetDirectParent(TNodeOrToken node);
        protected abstract TNodeOrTokenEnumerable GetDirectChildren(TNodeOrToken node);
        protected abstract bool Exists(TNodeOrToken node);
        protected abstract TNode AsNode(TNodeOrToken node);
        protected abstract TNodeOrToken ToNodeOrToken(TNode node);
        protected abstract bool IsExpressionStatement(TNodeOrToken node, out TNode expression);
        protected abstract bool IsSwitchSection(TNodeOrToken node, out IEnumerable<TNode> labels);
        protected abstract bool IsPredefinedType(TNodeOrToken node, out TNodeOrToken keyword);
        protected virtual bool HasOtherNodeTypeDefiningChildNode(TNodeOrToken node, out TNodeOrToken child) {
            child = default;
            return false;
        }
        protected abstract bool MatchesNodeType(TNodeOrToken node, SyntaxPathSegment query);
        protected abstract string EvaluateKindFunction(TNodeOrToken current);
        protected abstract string AsIdentifierToString(TNodeOrToken node);
    }
}
