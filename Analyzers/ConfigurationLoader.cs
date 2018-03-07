using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SourcePath.CSharp;

namespace SourcePath.Analyzers {
    public class ConfigurationLoader {
        private readonly SyntaxQueryParser _parser;

        public ConfigurationLoader(SyntaxQueryParser parser) {
            _parser = parser;
        }

        public Configuration Load(string content) {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var rules = new List<Rule>();

            string ruleId = null;
            SyntaxQuery ruleQuery = null;
            RuleSeverity? ruleSeverity = null;
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
                    ruleSeverity = parts[0] == "error" ? RuleSeverity.Error : RuleSeverity.Warning;
                    ruleMessage = parts[1].Trim(); // TODO: don't allocate (if possible)
                }
                else {
                    if (ruleId != null && ruleQuery != null)
                        rules.Add(new Rule(ruleId, ruleQuery, ruleSeverity ?? RuleSeverity.Error, ruleMessage));
                    ruleId = line;
                    ruleQuery = null;
                    ruleSeverity = null;
                    ruleMessage = null;
                }
            }
            if (ruleId != null && ruleQuery != null)
                rules.Add(new Rule(ruleId, ruleQuery, ruleSeverity ?? RuleSeverity.Error, ruleMessage));
            return new Configuration(rules);
        }
    }
}
