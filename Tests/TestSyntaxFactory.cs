using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourcePath.Tests {
    public static class TestSyntaxFactory {
        private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(
            #if VSIX
            LanguageVersion.CSharp6
            #else
            LanguageVersion.Latest
            #endif
        );

        public static StatementSyntax ParseStatement(string code) {
            return EnsureNoErrors(SyntaxFactory.ParseStatement(code, options: ParseOptions));
        }

        public static CompilationUnitSyntax ParseCompilationUnit(string code) {
            return EnsureNoErrors(SyntaxFactory.ParseCompilationUnit(code, options: ParseOptions));
        }

        private static TSyntax EnsureNoErrors<TSyntax>(TSyntax syntax)
            where TSyntax : CSharpSyntaxNode
        {
            var errors = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
            if (errors.Any())
                throw new Exception($"Failed to compile code:\r\n{string.Join("\r\n", errors)}.");
            return syntax;
        }
    }
}
