using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using AshMind.Extensions;

namespace SourcePath.Tests.Unit {
    public abstract class CSharpKeywordBasedPathDialectTestsBase<TNode> {
        //[Theory]
        //[MemberData(nameof(GetAllCSharpKeywords))]
        //public void ResolveNodeKind_ResolvesAllCSharpKeywords(string keyword) {
        //    var dialect = NewDialect();
        //    Assert.NotNull(dialect.ResolveNodeKind(keyword));
        //}

        [Theory]
        [InlineData("if", "if (true) {}")]
        public void CSharpKeywords_MatchExpectedNodes(string keyword, string code) {
            var parsed = Parse(code);
            var nodeKind = NewDialect().ResolveNodeKind(keyword);
            Assert.True(nodeKind.Matches(parsed));
        }

        [Theory]
        [InlineData("name")]
        [InlineData("lambda")]
        [InlineData("tuple")]
        public void CSharpKeywords_HaveSpecial(string name) {
            var dialect = NewDialect();
            Assert.NotNull(dialect.ResolveNodeKind(name));
        }

        protected abstract TNode Parse(string code);
        protected abstract ISourcePathDialect<TNode> NewDialect();

        public static IEnumerable<object[]> GetAllCSharpKeywords() {
            var source = GetRoslynKeywordsSourceAsync().Result;
            var keywords = SyntaxFactory.ParseCompilationUnit(source)
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Text != "GetPreprocessorKeywordKind" && m.Identifier.Text != "GetText")
                .SelectMany(m => m.Body.DescendantNodes())
                .OfType<LiteralExpressionSyntax>()
                .Select(s => s.Token.Value)
                .OfType<string>()
                .Where(k => Regex.IsMatch(k, "^[a-z]+$"));
            return keywords.Select(k => new[] { k });
        }

        private static async Task<string> GetRoslynKeywordsSourceAsync() {
            var factsPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetAssemblyFileFromCodeBase().DirectoryName,
                "roslyn_SyntaxKindFacts.cached.cs"
            );
            if (File.Exists(factsPath))
                return File.ReadAllText(factsPath);

            using (var client = new HttpClient()) {
                var facts = await client.GetStringAsync(
                    "https://raw.githubusercontent.com/dotnet/roslyn/1b0cf5c732062f66b71a3d62a165d6eb5f8b3022/src/Compilers/CSharp/Portable/Syntax/SyntaxKindFacts.cs"
                ).ConfigureAwait(false);
                File.WriteAllText(factsPath, facts);
                return facts;
            }
        }
    }
}
