using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;

namespace Lastql.CSharp {
    using static Parser;
    using static Parser<char>;

    public class CSharpSyntaxQueryParser {
        private static readonly IReadOnlyDictionary<string, CSharpSyntaxQueryTarget> TargetsByKeywords =
            ((CSharpSyntaxQueryTarget[])Enum.GetValues(typeof(CSharpSyntaxQueryTarget))).ToDictionary(
                v => v.ToString("G").ToLowerInvariant(),
                v => v
            );

        private static readonly Parser<char, SyntaxQueryAxis> Axis = OneOf(
            Try(String("self::").Select(s => SyntaxQueryAxis.Self)),
            Try(String("//").Select(s => SyntaxQueryAxis.Descendant)),
            String("/").Select(s => SyntaxQueryAxis.Child)
        );

        private static readonly Parser<char, CSharpSyntaxQueryTarget> Target =
            Letter.Labelled("keyword").AtLeastOnceString().Select(ParseKeyword);

        private static readonly Parser<char, CSharpSyntaxQuery> Root = Map(
            (axis, target) => new CSharpSyntaxQuery(axis.GetValueOrDefault(SyntaxQueryAxis.Child), target),
            Axis.Optional(),
            Target
        ).Before(End());

        public CSharpSyntaxQuery Parse(string query) {
            return Root.ParseOrThrow(query);
        }

        private static CSharpSyntaxQueryTarget ParseKeyword(string keyword) {
            if (!TargetsByKeywords.TryGetValue(keyword, out var target))
                throw new FormatException($"Unknown C# keyword: {keyword}.");
            return target;
        }
    }
}
