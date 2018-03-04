using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Lastql.CSharp {
    public class CSharpSyntaxQuery {
        private readonly HashSet<SyntaxKind> _syntaxKinds;

        public CSharpSyntaxQuery(SyntaxQueryAxis axis, HashSet<SyntaxKind> syntaxKinds) {
            Axis = axis;
            _syntaxKinds = syntaxKinds;
        }

        public SyntaxQueryAxis Axis { get; }
        public IReadOnlyCollection<SyntaxKind> SyntaxKinds => _syntaxKinds;
        public bool MatchesSyntaxKind(SyntaxKind kind) {
            return _syntaxKinds.Contains(kind);
        }
    }
}