using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourcePath.CSharp;

namespace SourcePath.Roslyn {
    public interface IRoslynCSharpSyntaxQueryExecutor {
        IEnumerable<SyntaxKind> GetRootSyntaxKinds(SyntaxPath path);
        bool Matches(SyntaxNodeOrToken current, SyntaxPathSegment path);
        IEnumerable<SyntaxNodeOrToken> QueryAll(SyntaxNodeOrToken current, SyntaxPath path);
    }
}