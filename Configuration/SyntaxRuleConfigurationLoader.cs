using System;
using System.Collections.Generic;
using SourcePath.CSharp;

namespace SourcePath.Configuration {
    public class SyntaxRuleConfigurationLoader {
        private readonly SyntaxQueryParser _parser;

        public SyntaxRuleConfigurationLoader(SyntaxQueryParser parser) {
            _parser = parser;
        }

        public string DefaultFileName => ".sourcepathrc";

        public SyntaxRuleConfiguration Load(string content) {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var rules = new List<SyntaxRule>();

            string ruleId = null;
            SyntaxQuery ruleQuery = null;
            SyntaxRuleSeverity? ruleSeverity = null;
            string ruleMessage = null;
            foreach (var line in lines) {
                if (Char.IsWhiteSpace(line[0])) {
                    // TODO: don't allocate
                    var trimmed = line.TrimStart();
                    if (ruleId == null)
                        continue; // TODO: error
                    if (ruleQuery == null) {
                        ruleQuery = _parser.Parse(trimmed);
                        continue;
                    }
                    // TODO: don't allocate
                    var parts = trimmed.Split(new[] { ':' }, 2);
                    ruleSeverity = parts[0] == "error" ? SyntaxRuleSeverity.Error : SyntaxRuleSeverity.Warning;
                    ruleMessage = parts[1].Trim(); // TODO: don't allocate (if possible)
                }
                else {
                    if (ruleId != null && ruleQuery != null)
                        rules.Add(new SyntaxRule(ruleId, ruleQuery, ruleSeverity ?? SyntaxRuleSeverity.Error, ruleMessage));
                    ruleId = line;
                    ruleQuery = null;
                    ruleSeverity = null;
                    ruleMessage = null;
                }
            }
            if (ruleId != null && ruleQuery != null)
                rules.Add(new SyntaxRule(ruleId, ruleQuery, ruleSeverity ?? SyntaxRuleSeverity.Error, ruleMessage));
            return new SyntaxRuleConfiguration(rules);
        }
    }
}
