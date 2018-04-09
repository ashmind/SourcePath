using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using SourcePath.Configuration;

namespace SourcePath.Rider {
    public class SourcePathDaemonStage : CSharpDaemonStageBase {
        private readonly RiderCSharpSyntaxQueryExecutor _queryExecutor;

        public SourcePathDaemonStage(RiderCSharpSyntaxQueryExecutor queryExecutor) {
            _queryExecutor = queryExecutor;
        }

        protected override IDaemonStageProcess CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind processKind, ICSharpFile file) {
            // TODO: Load configuration
            var configuration = new SyntaxRuleConfiguration(new SyntaxRule[0]);
            return new SourcePathDaemonProcess(process, file, configuration, _queryExecutor);
        }
    }
}
