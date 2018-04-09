namespace SourcePath.CSharp {
    public interface ISyntaxPathParser {
        SyntaxPath Parse(string path, SyntaxPathAxis? axis = null);
    }
}