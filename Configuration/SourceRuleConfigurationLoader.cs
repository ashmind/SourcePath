using System;
using System.Collections.Generic;

namespace SourcePath.Configuration {
    public class SourceRuleConfigurationLoader<TNode> {
        private readonly ISourcePathParser<TNode> _parser;

        public SourceRuleConfigurationLoader(ISourcePathParser<TNode> parser) {
            _parser = parser;
        }

        public string DefaultFileName => ".sourcepathrc";

        public SourceRuleConfiguration<TNode> Load(string content) {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var rules = new List<SourceRule<TNode>>();

            string ruleId = null;
            ISourcePath<TNode> rulePath = null;
            SourceRuleSeverity? ruleSeverity = null;
            string ruleMessage = null;
            foreach (var line in lines) {
                if (Char.IsWhiteSpace(line[0])) {
                    // TODO: don't allocate
                    var trimmed = line.TrimStart();
                    if (ruleId == null)
                        continue; // TODO: error
                    if (rulePath == null) {
                        rulePath = _parser.Parse(trimmed);
                        continue;
                    }
                    // TODO: don't allocate
                    var parts = trimmed.Split(new[] { ':' }, 2);
                    ruleSeverity = parts[0] == "error" ? SourceRuleSeverity.Error : SourceRuleSeverity.Warning;
                    ruleMessage = parts[1].Trim(); // TODO: don't allocate (if possible)
                }
                else {
                    if (ruleId != null && rulePath != null)
                        rules.Add(new SourceRule<TNode>(ruleId, rulePath, ruleSeverity ?? SourceRuleSeverity.Error, ruleMessage));
                    ruleId = line;
                    rulePath = null;
                    ruleSeverity = null;
                    ruleMessage = null;
                }
            }
            if (ruleId != null && rulePath != null)
                rules.Add(new SourceRule<TNode>(ruleId, rulePath, ruleSeverity ?? SourceRuleSeverity.Error, ruleMessage));
            return new SourceRuleConfiguration<TNode>(rules);
        }
    }
}
