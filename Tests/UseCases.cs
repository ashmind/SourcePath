using System.Linq;
using Lastql.CSharp;
using Lastql.Roslyn;
using Xunit;

namespace Lastql.Tests {
    public class UseCases {
        [Theory]
        [InlineData("class C { async void M() {} }", "async void M() {}")]
        public void AsyncVoidMethods(string code, string expected) {
            var query = "//method[async && void]";
            AssertQueryAll(new[] { expected }, code, query);
        }

        private static void AssertQueryAll(string[] expected, string code, string queryAsString) {
            var unit = TestSyntaxFactory.ParseCompilationUnit(code);
            var query = new SyntaxQueryParser().Parse(queryAsString);
            var results = new CSharpSyntaxQueryRoslynExecutor().QueryAll(unit, query);
            Assert.Equal(expected, results.Select(r => r.ToString()).ToArray());
        }
    }
}
