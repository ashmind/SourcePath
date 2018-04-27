namespace SourcePath {
    public interface ISourcePathParser<TNode> {
        ISourcePath<TNode> Parse(string path);
    }
}