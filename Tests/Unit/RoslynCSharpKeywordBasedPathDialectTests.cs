using Microsoft.CodeAnalysis;
using SourcePath.Roslyn;

namespace SourcePath.Tests.Unit {
    public class RoslynCSharpKeywordBasedPathDialectTests : CSharpKeywordBasedPathDialectTestsBase<RoslynNodeContext> {
        protected override ISourcePathDialect<RoslynNodeContext> NewDialect() {
            return new RoslynCSharpKeywordBasedPathDialect();
        }

        protected override RoslynNodeContext Parse(string code) {
            var parsed = TestSyntaxFactory.Parse(code, TestSourceKind.Statement);
            return new RoslynNodeContext(parsed, null);
        }
    }
}
