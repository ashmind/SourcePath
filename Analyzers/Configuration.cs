using System.Collections.Generic;

namespace SourcePath.Analyzers {
    public class Configuration {
        public Configuration(IReadOnlyCollection<Rule> rules) {
            Rules = rules;
        }

        public IReadOnlyCollection<Rule> Rules { get; }
    }
}
