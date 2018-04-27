namespace SourcePath.Configuration {
    public class SourceRule<TNode> {
        public SourceRule(string id, ISourcePath<TNode> path, SourceRuleSeverity severity, string message) {
            Id = id;
            Path = path;
            Severity = severity;
            Message = message;
        }

        public string Id { get; }
        public ISourcePath<TNode> Path { get; }
        public SourceRuleSeverity Severity { get; }
        public string Message { get; }
    }
}
