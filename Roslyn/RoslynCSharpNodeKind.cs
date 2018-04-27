using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourcePath.Roslyn {
    public class RoslynCSharpNodeKind : ISourceNodeKind<SyntaxNodeOrToken> {
        public RoslynCSharpNodeKind(string name, ISet<SyntaxKind> syntaxKinds) {
            Name = Argument.NotNullOrEmpty(nameof(name), name);
            SyntaxKinds = Argument.NotNullOrEmpty(nameof(syntaxKinds), syntaxKinds);
        }

        public string Name { get; }
        public ISet<SyntaxKind> SyntaxKinds { get; }

        public bool Matches(SyntaxNodeOrToken node) => SyntaxKinds.Contains(node.Kind());

        public string ToPathString() => Name;
        public override string ToString() => Name;
    }
}
