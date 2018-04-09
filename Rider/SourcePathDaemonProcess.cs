using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using SourcePath.Configuration;

namespace SourcePath.Rider {
    internal class SourcePathDaemonProcess : IDaemonStageProcess {
        private IDaemonProcess _process;
        private readonly ICSharpFile _file;
        private readonly RiderCSharpSyntaxQueryExecutor _queryExecutor;
        private readonly SyntaxRuleConfiguration _configuration;

        public SourcePathDaemonProcess(
            IDaemonProcess process,
            ICSharpFile file,
            SyntaxRuleConfiguration configuration,
            RiderCSharpSyntaxQueryExecutor queryExecutor
        ) {
            _process = process;
            _file = file;
            _queryExecutor = queryExecutor;
            _configuration = configuration;
        }

        public IDaemonProcess DaemonProcess => _process;
        public void Execute(Action<DaemonStageResult> committer) {
            // NOT IMPLEMENTED YET
            throw new NotImplementedException();
            //var ruleDictionary = _configuration.Rules.ToDic
            //foreach (var rule in _configuration.Rules) {
                
            //}
        }
    }
}