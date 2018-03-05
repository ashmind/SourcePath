using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;

namespace SourcePath.CSharp {
    using Pidgin.Expression;
    using static Parser;
    using static Parser<char>;

    public class SyntaxQueryParser {
        private static readonly IReadOnlyDictionary<string, SyntaxQueryKeyword> TargetsByKeywords =
            ((SyntaxQueryKeyword[])Enum.GetValues(typeof(SyntaxQueryKeyword))).ToDictionary(
                v => v.ToString("G").ToLowerInvariant(),
                v => v
            );

        private static readonly Parser<char, SyntaxQuery> Root;

        static SyntaxQueryParser() {
            var axis = OneOf(
                Try(String("self::").Select(s => SyntaxQueryAxis.Self)),
                Try(OneOf(String("//"), String("descendant::")).Select(s => SyntaxQueryAxis.Descendant)),
                String("/").Select(s => SyntaxQueryAxis.Child),
                Try(String("parent::").Select(s => SyntaxQueryAxis.Parent))
            );
            var target = Letter.Labelled("keyword").AtLeastOnceString().Select(ParseKeyword);

            Parser<char, SyntaxQuery> query = null;
            var literal = AnyCharExcept('\'').ManyString().Between(Char('\''))
                .Select(value => new SyntaxFilterLiteralExpression(value));
            var expressionLeaf = OneOf(
                Rec(() => query).Cast<ISyntaxFilterExpression>(),
                literal.Cast<ISyntaxFilterExpression>()
            );
            var expression = ExpressionParser.Build(
                expressionLeaf,
                new[] {
                    new[] {
                        BinaryOperator("==", SyntaxFilterBinaryOperator.Equals)
                    },
                    new[] {
                        BinaryOperator("&&", SyntaxFilterBinaryOperator.And)
                    },
                }
            );
            var filter = Char('[').Then(expression).Before(Char(']'));

            query = Map(
                (a, t, f) => new SyntaxQuery(a.GetValueOrDefault(SyntaxQueryAxis.Child), t, f.GetValueOrDefault()),
                axis.Optional(),
                target,
                filter.Optional()
            );

            Root = query.Before(End());
        }

        private static OperatorTableRow<char, ISyntaxFilterExpression> BinaryOperator(string token, SyntaxFilterBinaryOperator @operator) {
            return Operator.Binary(
                BinaryOperatorType.LeftAssociative,
                Try(String(token).Between(SkipWhitespaces)).Then(Return<Func<ISyntaxFilterExpression, ISyntaxFilterExpression, ISyntaxFilterExpression>>(
                    (left, right) => new SyntaxFilterBinaryExpression(left, @operator, right)
                ))
            );
        }

        public SyntaxQuery Parse(string query) {
            return Root.ParseOrThrow(query);
        }

        private static SyntaxQueryKeyword ParseKeyword(string keyword) {
            if (!TargetsByKeywords.TryGetValue(keyword, out var target))
                throw new FormatException($"Unknown C# keyword: {keyword}.");
            return target;
        }
    }
}
