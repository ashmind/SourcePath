using JetBrains.ReSharper.Psi.Tree;
using SourcePath.Tests;
using SourcePath.Tests.Unit;

namespace SourcePath.Rider.Tests.Unit {
    public class RiderCSharpKeywordBasedPathDialectTests : CSharpKeywordBasedPathDialectTestsBase<ITreeNode> {
        protected override ISourcePathDialect<ITreeNode> NewDialect() {
            return new RiderCSharpKeywordBasedPathDialect();
        }

        protected override ITreeNode Parse(string code) {
            return TestParser.ParseCSharp(code, TestSourceKind.Statement);
        }
    }
}
