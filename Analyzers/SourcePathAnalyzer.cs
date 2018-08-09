using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SourcePath.Configuration;
using SourcePath.Roslyn;

namespace SourcePath.Analyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SourcePathAnalyzer : DiagnosticAnalyzer {
        private static readonly IReadOnlyDictionary<SourceRuleSeverity, DiagnosticDescriptor> _descriptors = new Dictionary<SourceRuleSeverity, DiagnosticDescriptor> {
            { SourceRuleSeverity.Error, new DiagnosticDescriptor("SPE", "_", "{0}", "SourcePath", DiagnosticSeverity.Error, true) },
            { SourceRuleSeverity.Warning, new DiagnosticDescriptor("SPW", "_", "{0}", "SourcePath", DiagnosticSeverity.Warning, true) }
        };
        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            _descriptors.Values.ToImmutableArray();

        private readonly SourceRuleConfigurationLoader<RoslynNodeContext> _configurationLoader;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

        public SourcePathAnalyzer() : this(
            new SourceRuleConfigurationLoader<RoslynNodeContext>(
                new SourcePathParser<RoslynNodeContext>(
                    new RoslynCSharpKeywordBasedPathDialect(new SourcePathDialectSupports {
                        TopLevelAxis = false,
                        TopLevelSegments = false,
                        TopLevelAnd = false
                    }),
                    new RoslynNodeHandler()
                )
            )
        ) {
        }

        public SourcePathAnalyzer(SourceRuleConfigurationLoader<RoslynNodeContext> configurationLoader) {
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
                var syntaxKinds = ImmutableArray.CreateRange(GetSyntaxKinds(rule.Path));
                context.RegisterSyntaxNodeAction(
                    c => ProcessSyntaxNode(c, rule),
                    syntaxKinds
                );
            }
        }

        private IEnumerable<SyntaxKind> GetSyntaxKinds(ISourcePath<RoslynNodeContext> path) {
            foreach (var nodeKind in GetRootNodeKinds(path)) {
                foreach (var syntaxKind in nodeKind.SyntaxKinds) {
                    yield return syntaxKind;
                }
            }
        }

        private IEnumerable<RoslynCSharpNodeKind> GetRootNodeKinds(ISourcePath<RoslynNodeContext> path) {
            switch (path) {
                case SourcePathSequence<RoslynNodeContext> sequence:
                    yield return (RoslynCSharpNodeKind)sequence.Segments[0].Kind;
                    yield break;

                case SourcePathBinaryExpression<RoslynNodeContext> binary:
                    foreach (var kind in GetRootNodeKinds(binary.Left)) yield return kind;
                    foreach (var kind in GetRootNodeKinds(binary.Right)) yield return kind;
                    yield break;

                default:
                    throw new NotSupportedException($"Unknown top-level path type: {path.GetType()}.");
            }
        }

        private SourceRuleConfiguration<RoslynNodeContext> GetConfiguration(AnalyzerOptions options) {
            foreach (var file in options.AdditionalFiles) {
                if (file.Path.EndsWith(_configurationLoader.DefaultFileName))
                    return _configurationLoader.Load(file.GetText().ToString());
            }
            return null;
        }

        private void ProcessSyntaxNode(SyntaxNodeAnalysisContext context, SourceRule<RoslynNodeContext> rule) {
            var convertedContext = new RoslynNodeContext(context.Node, context.SemanticModel);
            if (!rule.Path.Matches(convertedContext, defaultAxis: SourcePathAxis.Self))
                return;

            var diagnostic = Diagnostic.Create(
                _descriptors[rule.Severity], context.Node.GetLocation(), new[] { rule.Message }
            );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
