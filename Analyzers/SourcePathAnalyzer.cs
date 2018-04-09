using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SourcePath.CSharp;
using SourcePath.Configuration;
using SourcePath.Roslyn;

namespace SourcePath.Analyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SourcePathAnalyzer : DiagnosticAnalyzer {
        private static readonly IReadOnlyDictionary<SyntaxRuleSeverity, DiagnosticDescriptor> _descriptors = new Dictionary<SyntaxRuleSeverity, DiagnosticDescriptor> {
            { SyntaxRuleSeverity.Error, new DiagnosticDescriptor("SPE", "_", "{0}", "SourcePath", DiagnosticSeverity.Error, true) },
            { SyntaxRuleSeverity.Warning, new DiagnosticDescriptor("SPW", "_", "{0}", "SourcePath", DiagnosticSeverity.Warning, true) }
        };
        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            _descriptors.Values.ToImmutableArray();

        private readonly RoslynCSharpSyntaxQueryExecutor _executor;
        private readonly SyntaxRuleConfigurationLoader _configurationLoader;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

        public SourcePathAnalyzer() : this(new SyntaxRuleConfigurationLoader(new SyntaxQueryParser()), new RoslynCSharpSyntaxQueryExecutor()) {
        }

        public SourcePathAnalyzer(SyntaxRuleConfigurationLoader configurationLoader, RoslynCSharpSyntaxQueryExecutor executor) {
            _executor = executor;
            _configurationLoader = configurationLoader;
        }

        public override void Initialize(AnalysisContext context) {
            context.RegisterCompilationStartAction(ProcessCompilationStart);
        }

        private void ProcessCompilationStart(CompilationStartAnalysisContext context) {
            var configuration = GetConfiguration(context.Options);
            if (configuration == null)
                return;

            foreach (var rule in configuration.Rules) {
                var syntaxKinds = ImmutableArray.CreateRange(_executor.GetRootSyntaxKinds(rule.Query));
                context.RegisterSyntaxNodeAction(
                    c => ProcessSyntaxNode(c, rule),
                    syntaxKinds
                );
            }
        }

        private SyntaxRuleConfiguration GetConfiguration(AnalyzerOptions options) {
            foreach (var file in options.AdditionalFiles) {
                if (file.Path.EndsWith(_configurationLoader.DefaultFileName))
                    return _configurationLoader.Load(file.GetText().ToString());
            }
            return null;
        }

        private void ProcessSyntaxNode(SyntaxNodeAnalysisContext context, SyntaxRule rule) {
            if (!_executor.QueryAll(context.Node, rule.Query).Any())
                return;

            var diagnostic = Diagnostic.Create(
                _descriptors[rule.Severity], context.Node.GetLocation(), new[] { rule.Message }
            );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
