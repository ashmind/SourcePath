using System.Text;

namespace SourcePath {
    public static class SourcePathExtensions {
        public static string ToPathString<TNode>(this ISourcePath<TNode> path) {
            var builder = new StringBuilder();
            path.AppendToPathString(builder);
            return builder.ToString();
        }
    }
}
