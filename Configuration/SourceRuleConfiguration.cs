using System.Collections.Generic;

namespace SourcePath.Configuration {
    public class SourceRuleConfiguration<TNode> {
        public SourceRuleConfiguration(IReadOnlyCollection<SourceRule<TNode>> rules) {
            Rules = rules;
        }

        public IReadOnlyCollection<SourceRule<TNode>> Rules { get; }
    }
}
