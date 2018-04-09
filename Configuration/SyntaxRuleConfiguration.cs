using System.Collections.Generic;

namespace SourcePath.Configuration {
    public class SyntaxRuleConfiguration {
        public SyntaxRuleConfiguration(IReadOnlyCollection<SyntaxRule> rules) {
            Rules = rules;
        }

        public IReadOnlyCollection<SyntaxRule> Rules { get; }
    }
}
