using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Lastql.CSharp {
    public class CSharpSyntaxQueryParser {
        private static readonly IReadOnlyDictionary<string, CSharpSyntaxQueryTarget> TargetsByKeywords =
            ((CSharpSyntaxQueryTarget[])Enum.GetValues(typeof(CSharpSyntaxQueryTarget))).ToDictionary(
                v => v.ToString("G").ToLowerInvariant(),
                v => v
            );

        private static readonly Parser<SyntaxQueryAxis> Axis = Sprache.Parse
            .String("self::").Select(s => SyntaxQueryAxis.Self)
            .Or(Sprache.Parse.String("//").Select(s => SyntaxQueryAxis.Descendant))
            .Or(Sprache.Parse.String("/").Select(s => SyntaxQueryAxis.Child));
        private static readonly Parser<string> Keyword = Sprache.Parse
            .Identifier(Sprache.Parse.Letter, Sprache.Parse.Letter);

        private static readonly Parser<CSharpSyntaxQuery> Root = Sprache.Parse
            .Optional(Axis)
            .Then(a => Keyword.Select(s => new CSharpSyntaxQuery(a.GetOrElse(SyntaxQueryAxis.Child), ParseKeyword(s))));

        public CSharpSyntaxQuery Parse(string query) {
            return Root.Parse(query);
        }

        private static CSharpSyntaxQueryTarget ParseKeyword(string keyword) {
            if (!TargetsByKeywords.TryGetValue(keyword, out var target))
                throw new FormatException($"Unknown C# keyword: {keyword}.");
            return target;
        }
    }
}
