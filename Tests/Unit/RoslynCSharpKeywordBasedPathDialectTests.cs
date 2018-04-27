using SourcePath.Roslyn;
using Microsoft.CodeAnalysis;

namespace SourcePath.Tests.Unit {
    public class RoslynCSharpKeywordBasedPathDialectTests : CSharpKeywordBasedPathDialectTestsBase<SyntaxNodeOrToken> {
        protected override ISourcePathDialect<SyntaxNodeOrToken> NewDialect() {
            return new RoslynCSharpKeywordBasedPathDialect();
        }

        protected override SyntaxNodeOrToken Parse(string code) {
            return TestSyntaxFactory.Parse(code, TestSourceKind.Statement);
        }
    }
}
