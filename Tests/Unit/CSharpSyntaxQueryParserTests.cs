using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AshMind.Extensions;
using Xunit;
using Lastql.CSharp;

namespace Lastql.Tests.Unit {
    public class CSharpSyntaxQueryParserTests {
        [Theory]
        [InlineData("if", CSharpSyntaxQueryTarget.If)]
        public void Parse_Basic(string queryAsString, CSharpSyntaxQueryTarget expectedTarget) {
            var query = new CSharpSyntaxQueryParser().Parse(queryAsString);
            Assert.Equal(expectedTarget, query.Target);
        }

        [Theory]
        [InlineData("if", SyntaxQueryAxis.Child)]
        [InlineData("/if", SyntaxQueryAxis.Child)]
        [InlineData("//if", SyntaxQueryAxis.Descendant)]
        [InlineData("self::if", SyntaxQueryAxis.Self)]
        public void Parse_Axis(string queryAsString, SyntaxQueryAxis expectedAxis) {
            var query = new CSharpSyntaxQueryParser().Parse(queryAsString);
            Assert.Equal(expectedAxis, query.Axis);
        }

        [Theory]
        [MemberData(nameof(GetAllKeywords))]
        public void Parse_Keyword(string keyword) {
            // Assert.DoesNotThrow
            new CSharpSyntaxQueryParser().Parse(keyword);
        }

        public static IEnumerable<object[]> GetAllKeywords() {
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
            return keywords.Select(k => new string[] { k });
        }

        private static async Task<string> GetRoslynKeywordsSourceAsync() {
            var factsPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetAssemblyFileFromCodeBase().DirectoryName,
                "roslyn_SyntaxKindFacts.cached.cs"
            );
            if (File.Exists(factsPath))
                return await File.ReadAllTextAsync(factsPath);

            using (var client = new HttpClient()) {
                var facts = await client.GetStringAsync(
                    "https://raw.githubusercontent.com/dotnet/roslyn/1b0cf5c732062f66b71a3d62a165d6eb5f8b3022/src/Compilers/CSharp/Portable/Syntax/SyntaxKindFacts.cs"
                ).ConfigureAwait(false);
                await File.WriteAllTextAsync(factsPath, facts).ConfigureAwait(false);
                return facts;
            }
        }
    }
}
