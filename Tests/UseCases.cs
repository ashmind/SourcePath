using System.Linq;
using SourcePath.CSharp;
using SourcePath.Roslyn;
using Xunit;

namespace SourcePath.Tests {
    public class UseCases {
        [Theory]
        [InlineData("class C { async void M() {} }", "async void M() {}")]
        [InlineData("class C { void M() {} }", null)]
        [InlineData("class C { async Task M() {} }", null)]
        public void AsyncVoidMethods(string code, string expected) {
            var query = "//method[async && void]";
            AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
        }

        [Theory]
        [InlineData("class C { void M() { try {} catch (Exception ex) { throw ex; } } }", "throw ex;")]
        [InlineData("class C { void M() { try {} catch { throw; } } }", null)]
        [InlineData("class C { void M() { throw new Exception(); } }", null)]
        public void ThrowExInsteadOfThrow(string code, string expected) {
            var query = "//throw[name]";
            AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
        }

        [Theory]
        [InlineData("namespace Internal { public class C {} }", "public class C {}")]
        public void PublicClassInInternalNamespace(string code, string expected) {
            var query = "//class[public && parent::namespace[name == 'Internal']]";
            AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
        }

        private static void AssertQueryAll(string[] expected, string code, string queryAsString) {
            var unit = TestSyntaxFactory.ParseCompilationUnit(code);
            var query = new SyntaxQueryParser().Parse(queryAsString);
            var results = new RoslynCSharpSyntaxQueryExecutor().QueryAll(unit, query);
            Assert.Equal(expected, results.Select(r => r.ToString()).ToArray());
        }
    }
}
