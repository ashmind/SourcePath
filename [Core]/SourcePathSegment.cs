using System;
using System.Text;

namespace SourcePath {
    public class SourcePathSegment<TNode> {
        private readonly ISourcePathAxisNavigator<TNode> _navigator;

        public SourcePathSegment(
            SourcePathAxis? axis,
            ISourceNodeKind<TNode> kind,
            ISourcePath<TNode> filter,
            ISourcePathAxisNavigator<TNode> navigator
        ) {
            Argument.NotNull(nameof(kind), kind);
            Argument.NotNull(nameof(navigator), navigator);

            Axis = axis;
            Kind = kind;
            Filter = filter;
            _navigator = navigator;
        }

        public SourcePathAxis? Axis { get; }
        public ISourceNodeKind<TNode> Kind { get; }
        public ISourcePath<TNode> Filter { get; }

        public bool Matches(TNode node, SourcePathAxis defaultAxis) {
            var axis = Axis ?? defaultAxis;
            if (axis == SourcePathAxis.Self)
                return MatchesIgnoringAxis(node);

            foreach (var other in _navigator.Navigate(node, axis)) {
                if (MatchesIgnoringAxis(other))
                    return true;
            }

            return false;
        }

        private bool MatchesIgnoringAxis(TNode node) {
            return Kind.Matches(node)
                && (Filter == null || Filter.Matches(node, SourcePathAxis.Child));
        }

        public override string ToString() {
            var builder = new StringBuilder();
            AppendToString(builder);
            return builder.ToString();
        }

        public void AppendToString(StringBuilder builder) {
            Argument.NotNull(nameof(builder), builder);
            switch (Axis) {
                case SourcePathAxis.Self: builder.Append("self::"); break;
                case SourcePathAxis.Descendant: builder.Append("//"); break;
                case SourcePathAxis.Parent: builder.Append("parent::"); break;
            }
            builder.Append(Kind.ToPathString());
            if (Filter != null) {
                builder.Append("[");
                Filter.AppendToPathString(builder);
                builder.Append("]");
            }
        }
    }
}
