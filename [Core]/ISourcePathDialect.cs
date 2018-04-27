namespace SourcePath {
    public interface ISourcePathDialect<TNode> {
        ISourceNodeKind<TNode> ResolveNodeKind(string nodeKindString);
        SourcePathDialectSupports Supports { get; }
    }
}
