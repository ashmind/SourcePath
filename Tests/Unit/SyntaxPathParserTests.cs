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
using SourcePath.CSharp;

namespace SourcePath.Tests.Unit {
    public class SyntaxPathParserTests {
        [Theory]
        [InlineData("if", SyntaxPathKeyword.If)]
        public void Parse_Basic(string pathAsString, SyntaxPathKeyword expectedTarget) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            var segment = Assert.Single(path.Segments);
            Assert.Equal(expectedTarget, segment.Keyword);
        }

        [Theory]
        [InlineData("*", SyntaxPathKeyword.Star)]
        [InlineData("name", SyntaxPathKeyword.Name)]
        [InlineData("lambda", SyntaxPathKeyword.Lambda)]
        [InlineData("tuple", SyntaxPathKeyword.Tuple)]
        public void Parse_Special(string pathAsString, SyntaxPathKeyword expectedTarget) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            var segment = Assert.Single(path.Segments);
            Assert.Equal(expectedTarget, segment.Keyword);
        }

        [Theory]
        [InlineData("if", SyntaxPathAxis.Default)]
        [InlineData("/if", SyntaxPathAxis.Child)]
        [InlineData("//if", SyntaxPathAxis.Descendant)]
        [InlineData("descendant::if", SyntaxPathAxis.Descendant)]
        [InlineData("self::if", SyntaxPathAxis.Self)]
        [InlineData("parent::if", SyntaxPathAxis.Parent)]
        [InlineData("ancestor::if", SyntaxPathAxis.Ancestor)]
        public void Parse_Axis(string pathAsString, SyntaxPathAxis expectedAxis) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            var segment = Assert.Single(path.Segments);
            Assert.Equal(expectedAxis, segment.Axis);
        }

        [Theory]
        [InlineData("if[if]")]
        [InlineData("if[if && if]")]
        [InlineData("if[if && if && if]")]
        [InlineData("if[if && if[if && if]]")]
        [InlineData("if[if || if]")]
        [InlineData("class[name == 'C']")]
        [InlineData("class[name == 'C' && method[name == 'M']]")]
        public void Parse_Filter(string pathAsString) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToString());
        }

        [Theory]
        [InlineData("if/if")]
        [InlineData("if/if/if")]
        [InlineData("self::if/parent::if/self::if")]
        public void Parse_Path(string pathAsString) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToString());
        }

        [Theory]
        [MemberData(nameof(GetAllKeywords))]
        public void Parse_Keyword(string keyword) {
            // Assert.DoesNotThrow
            new SyntaxPathParser().Parse(keyword);
        }

        [Theory]
        [InlineData("*[kind() == 'MethodDeclaration']")]
        public void Parse_Function(string pathAsString) {
            var path = new SyntaxPathParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToString());
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
