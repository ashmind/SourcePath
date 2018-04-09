using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;

namespace SourcePath.CSharp {
    using Pidgin.Expression;
    using static Parser;
    using static Parser<char>;

    public class SyntaxPathParser : ISyntaxPathParser {
        private static readonly IReadOnlyDictionary<string, SyntaxPathKeyword> KeywordsByName =
            ((SyntaxPathKeyword[])Enum.GetValues(typeof(SyntaxPathKeyword))).Except(new[] { SyntaxPathKeyword.Star }).ToDictionary(
                v => v.ToString("G").ToLowerInvariant(),
                v => v
            );

        private static readonly IReadOnlyDictionary<string, SyntaxPathFunction> FunctionsByName = new Dictionary<string, SyntaxPathFunction> {
            { "kind", SyntaxPathFunction.Kind }
        };

        private static readonly Parser<char, SyntaxPath> Root;

        static SyntaxPathParser() {
            var axis = OneOf(
                Try(String("self::").Select(s => SyntaxPathAxis.Self)),
                Try(OneOf(String("//"), String("descendant::")).Select(s => SyntaxPathAxis.Descendant)),
                Char('/').Select(s => SyntaxPathAxis.Child),
                Try(String("ancestor::").Select(s => SyntaxPathAxis.Ancestor)),
                Try(String("parent::").Select(s => SyntaxPathAxis.Parent))
            );
            var keyword = OneOf(
                Char('*').Select(s => SyntaxPathKeyword.Star),
                Letter.Labelled("keyword").AtLeastOnceString().Select(ParseKeyword)
            );

            Parser<char, SyntaxPath> path = null;
            var literal = AnyCharExcept('\'').ManyString().Between(Char('\''))
                .Select(value => new SyntaxFilterLiteralExpression(value));

            Parser<char, ISyntaxFilterExpression> compareBinaryExpression = null;
            var booleanExpressionLeaf = OneOf(
                Try(Rec(() => compareBinaryExpression)),
                Rec(() => path).Cast<ISyntaxFilterExpression>()
            );
            var booleanExpression = ExpressionParser.Build(
                booleanExpressionLeaf,
                new[] {
                    new[] {
                        BinaryOperator("&&", SyntaxFilterBinaryOperator.And),
                        BinaryOperator("||", SyntaxFilterBinaryOperator.Or)
                    }
                }
            );

            var function = Token(c => char.IsLetter(c) || c == '-').Labelled("function").AtLeastOnceString().Before(String("()"))
                .Select(s => new SyntaxFilterFunctionExpression(ParseFunction(s)));

            var compareExpressionLeaf = OneOf(
                Try(function.Cast<ISyntaxFilterExpression>()),
                Rec(() => path).Cast<ISyntaxFilterExpression>(),
                literal.Cast<ISyntaxFilterExpression>()
            );
            compareBinaryExpression = Map(
                (l, o, r) => (ISyntaxFilterExpression)new SyntaxFilterBinaryExpression(l, o, r),
                compareExpressionLeaf,
                String("==").Between(SkipWhitespaces).Select(_ => SyntaxFilterBinaryOperator.Equals),
                compareExpressionLeaf
            );

            var expression = booleanExpression;
            var filter = Char('[').Then(expression).Before(Char(']'));

            var segment = Map(
                (a, k, f) => new SyntaxPathSegment(a.GetValueOrDefault(SyntaxPathAxis.Default), k, f.GetValueOrDefault()),
                axis.Optional(),
                keyword,
                filter.Optional()
            );

            path = segment.SeparatedAtLeastOnce(Char('/'))
                .Select(s => new SyntaxPath(s.ToList()));

            Root = path.Before(End());
        }

        private static OperatorTableRow<char, ISyntaxFilterExpression> BinaryOperator(string token, SyntaxFilterBinaryOperator @operator) {
            return Operator.Binary(
                BinaryOperatorType.LeftAssociative,
                Try(String(token).Between(SkipWhitespaces)).Then(Return<Func<ISyntaxFilterExpression, ISyntaxFilterExpression, ISyntaxFilterExpression>>(
                    (left, right) => new SyntaxFilterBinaryExpression(left, @operator, right)
                ))
            );
        }

        public SyntaxPath Parse(string path, SyntaxPathAxis? axis = null) {
            var parsed = Root.ParseOrThrow(path);
            if (axis != null) {
                parsed = ForceAxis(parsed, axis.Value);
            }

            return parsed;
        }

        private static SyntaxPath ForceAxis(SyntaxPath path, SyntaxPathAxis axis) {
            var first = path.Segments[0];
            if (first.Axis != SyntaxPathAxis.Default)
                throw new FormatException($"First path axis must not be set (it will be automatically set to {axis}).");

            var newSegments = new List<SyntaxPathSegment> {
                new SyntaxPathSegment(axis, first.Keyword, first.Filter)
            };
            newSegments.AddRange(path.Segments.Skip(1));
            return new SyntaxPath(newSegments);
        }

        private static SyntaxPathKeyword ParseKeyword(string name) {
            if (!KeywordsByName.TryGetValue(name, out var keyword))
                throw new FormatException($"Unknown C# keyword: {name}.");
            return keyword;
        }

        private static SyntaxPathFunction ParseFunction(string name) {
            if (!FunctionsByName.TryGetValue(name, out var function))
                throw new FormatException($"Unknown function: {name}.");
            return function;
        }
    }
}
