using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.LanguageServices;
using SourcePath.CSharp;
using SourcePath.Roslyn;

namespace SourcePath.Vsix {
    public class SearchEngine {
        private readonly SyntaxQueryParser _parser;
        private readonly RoslynCSharpSyntaxQueryExecutor _executor;
        private readonly Func<VisualStudioWorkspace> _getWorkspace;

        public SearchEngine(
            SyntaxQueryParser parser,
            RoslynCSharpSyntaxQueryExecutor executor,
            Func<VisualStudioWorkspace> getWorkspace
        ) {
            _parser = parser;
            _executor = executor;
            _getWorkspace = getWorkspace;
        }

        public async Task<IEnumerable<SyntaxNodeOrToken>> SearchAsync(string query) {
            var parsedQuery = _parser.Parse(query);
            var projectTasks = _getWorkspace()
                .CurrentSolution.Projects
                .Select(async p => Search(parsedQuery, await p.GetCompilationAsync().ConfigureAwait(false)));

            return (await Task.WhenAll(projectTasks).ConfigureAwait(false)).SelectMany(results => results);
        }

        private IEnumerable<SyntaxNodeOrToken> Search(SyntaxQuery query, Compilation compilation) {
            foreach (var tree in compilation.SyntaxTrees) {
                foreach (var match in _executor.QueryAll((CSharpSyntaxNode)tree.GetRoot(), query)) {
                    yield return match;
                }
            }
        }
    }
}
