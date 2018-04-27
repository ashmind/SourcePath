using System.Linq;
using SourcePath.Roslyn;
using Xunit;

namespace SourcePath.Tests {
    //public class UseCaseTests {
    //    [Theory]
    //    [InlineData("class C { async void M() {} }", "async void M() {}")]
    //    [InlineData("class C { void M() {} }", null)]
    //    [InlineData("class C { async Task M() {} }", null)]
    //    public void AsyncVoidMethods(string code, string expected) {
    //        var query = "//method[async && void]";
    //        AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
    //    }

    //    [Theory]
    //    [InlineData("class C { void M() { try {} catch (Exception ex) { throw ex; } } }", "throw ex;")]
    //    [InlineData("class C { void M() { try {} catch { throw; } } }", null)]
    //    [InlineData("class C { void M() { throw new Exception(); } }", null)]
    //    public void ThrowExInsteadOfThrow(string code, string expected) {
    //        var query = "//throw[name]";
    //        AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
    //    }

    //    [Theory]
    //    [InlineData("namespace Internal { public class C {} }", "public class C {}")]
    //    public void PublicClassInInternalNamespace(string code, string expected) {
    //        var query = "//class[public && parent::namespace[name == 'Internal']]";
    //        AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
    //    }

    //    [Theory]
    //    [InlineData("class C { void M() { switch(1) { case int i: break; } } }", "case int i")]
    //    [InlineData("class C { void M() { switch(1) { case 2: break; } } }", null)]
    //    [InlineData("class C { void M(object o) { if (o is int i) {} } }", "o is int i")]
    //    [InlineData("class C { void M(object o) { if (o is int) {} } }", null)]
    //    public void SharpLabExplainModePatternMatching(string code, string expected) {
    //        var query = "//*[kind() == 'IsPatternExpression' || self::case[is]]";
    //        AssertQueryAll(expected != null ? new[] { expected } : new string[0], code, query);
    //    }

    //    private static void AssertQueryAll(string[] expected, string code, string queryAsString) {
    //        var unit = TestSyntaxFactory.Parse(code, TestSourceKind.CompilationUnit);
    //        var query = new SyntaxPathParser().Parse(queryAsString);
    //        var results = new RoslynCSharpSyntaxQueryExecutor().QueryAll(unit, query);
    //        Assert.Equal(expected, results.Select(r => r.ToString()).ToArray());
    //    }
    //}
}
