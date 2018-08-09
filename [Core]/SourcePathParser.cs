using System;
using System.Linq;
using Pidgin;
using Pidgin.Expression;

namespace SourcePath {
    using static Parser;
    using static Parser<char>;
    using static SourcePathAxis;

    public class SourcePathParser<TNode> : ISourcePathParser<TNode> {
        private readonly Parser<char, ISourcePath<TNode>> _root;
        private readonly ISourcePathDialect<TNode> _dialect;
        private readonly ISourceNodeHandler<TNode> _nodeHandler;

        public SourcePathParser(
            ISourcePathDialect<TNode> dialect,
            ISourceNodeHandler<TNode> nodeHandler
        ) {
            _dialect = dialect;
            _nodeHandler = nodeHandler;

            var axis = OneOf(
                Try(String("self::").Select(_ => AxisIfSupported(Self))),
                Try(OneOf(String("//"), String("descendant::")).Select(_ => AxisIfSupported(Descendant))),
                Char('/').Select(_ => AxisIfSupported(Child)),
                Try(String("ancestor::").Select(_ => AxisIfSupported(Ancestor))),
                Try(String("parent::").Select(_ => AxisIfSupported(Parent)))
            );
            var kind = Token(c => char.IsLetter(c) || c == '*'  || c == '_' || c == ':')
                .Labelled("kind")
                .AtLeastOnceString()
                .Select(ParseNodeKind);

            var quotedString = OneOf(
                Token(c => c != '\'').ManyString().Between(Char('\'')),
                Token(c => c != '"').ManyString().Between(Char('"'))
            ).Labelled("string constant").Select(s => new SourcePathConstant<TNode>(s, _nodeHandler));
            var constant = quotedString;
            var functionName = Token(char.IsLetter)
                .Labelled("function name")
                .AtLeastOnceString();
            var callArguments = constant.Select(l => new[] { l });
            var callSuffix = Char('.').Then(Map(
                (name, arguments) => (name, arguments),
                functionName,
                callArguments.Between(Char('('), Char(')'))
            ));
            Parser<char, SourcePathSequence<TNode>> path = null;
            var callOrPath = OneOf(
                Try(callSuffix.Select(c => new SourcePathFunctionCall<TNode>(c.name, c.arguments, _nodeHandler)))
                    .Cast<ISourcePath<TNode>>(),
                Rec(() => path).Cast<ISourcePath<TNode>>()
            );
            var booleanExpressionLeaf = callOrPath;
            var booleanExpression = ExpressionParser.Build(
                booleanExpressionLeaf,
                new[] {
                    new[] {
                        BinaryOperator("&&", SourcePathBinaryOperator.And),
                        BinaryOperator("||", SourcePathBinaryOperator.Or)
                    }
                }
            );

            var expression = booleanExpression;
            var filter = Char('[').Then(expression).Before(Char(']'));

            var segment = Map(
                (a, k, f) => new SourcePathSegment<TNode>(
                    a.HasValue ? a.Value : (SourcePathAxis?)null,
                    k,
                    f.GetValueOrDefault(),
                    _nodeHandler
                ),
                axis.Optional(),
                kind,
                filter.Optional()
            );

            path = segment.SeparatedAtLeastOnce(Char('/'))
                .Select(s => new SourcePathSequence<TNode>(s.ToList()));

            _root = expression.Before(End());
        }

        private SourcePathAxis AxisIfSupported(SourcePathAxis axis) {
            if (!_dialect.Supports.AxisSelf && (axis == Self || axis == DescendantOrSelf || axis == AncestorOrSelf))
                throw new FormatException($"Dialect {_dialect} does not support Self (found {axis})");

            if (!_dialect.Supports.AxisDescendant && (axis == Descendant || axis == DescendantOrSelf))
                throw new FormatException($"Dialect {_dialect} does not support Descendant (found {axis})");

            if (!_dialect.Supports.AxisParent && axis == Parent)
                throw new FormatException($"Dialect {_dialect} does not support Parent (found {axis})");

            if (!_dialect.Supports.AxisAncestor && (axis == Ancestor || axis == AncestorOrSelf))
                throw new FormatException($"Dialect {_dialect} does not support Ancestor (found {axis})");

            return axis;
        }

        private static OperatorTableRow<char, ISourcePath<TNode>> BinaryOperator(string token, SourcePathBinaryOperator @operator) {
            return Operator.Binary(
                BinaryOperatorType.LeftAssociative,
                Try(String(token).Between(SkipWhitespaces)).Then(Return<Func<ISourcePath<TNode>, ISourcePath<TNode>, ISourcePath<TNode>>>(
                    (left, right) => new SourcePathBinaryExpression<TNode>(left, @operator, right)
                ))
            );
        }

        public ISourcePath<TNode> Parse(string path) {
            var parsed = _root.ParseOrThrow(path);
            ValidateTopLevel(parsed);
            return parsed;
        }

        private void ValidateTopLevel(ISourcePath<TNode> expression) {
            switch (expression) {
                case SourcePathSequence<TNode> path:
                    ValidateTopLevel(path);
                    break;

                case SourcePathBinaryExpression<TNode> binary:
                    if (!_dialect.Supports.TopLevelAnd && binary.Operator == SourcePathBinaryOperator.And)
                        throw new FormatException($"Dialect {_dialect} does not support operator && at top level.");
                    ValidateTopLevel(binary.Left);
                    ValidateTopLevel(binary.Right);
                    break;

                default:
                    throw new NotSupportedException($"Unrecognized top-level expression type: {expression}.");
            }
        }

        private void ValidateTopLevel(SourcePathSequence<TNode> path) {
            if (!_dialect.Supports.TopLevelAxis && path.Segments[0].Axis != null)
                throw new FormatException($"Dialect {_dialect} does not support axis at top level ({path.Segments[0].Axis}).");

            if (!_dialect.Supports.TopLevelSegments && path.Segments.Count > 1)
                throw new FormatException($"Dialect {_dialect} does not support multiple segments at top level (e.g. {path.Segments[1]})");
        }

        private ISourceNodeKind<TNode> ParseNodeKind(string kindString) {
            var kind = _dialect.ResolveNodeKind(kindString);
            if (kind == null)
                throw new FormatException($"Dialect {_dialect} does not support node kind {kindString}.");
            return kind;
        }
    }
}
