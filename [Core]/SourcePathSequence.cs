using System;
using System.Collections.Generic;
using System.Text;

namespace SourcePath {
    public class SourcePathSequence<TNode> : ISourcePath<TNode> {
        public SourcePathSequence(IReadOnlyList<SourcePathSegment<TNode>> segments) {
            Argument.NotNullOrEmpty(nameof(segments), segments);
            Segments = segments;
        }

        public IReadOnlyList<SourcePathSegment<TNode>> Segments { get; }

        public bool Matches(TNode node, SourcePathAxis defaultAxis) {
            if (Segments.Count > 1)
                throw new NotSupportedException("Multi-segmented paths are not supported in this version.");

            return Segments[0].Matches(node, defaultAxis);
        }

        public IEnumerable<ISourceNodeKind<TNode>> GetRootNodeKinds() {
            yield return Segments[0].Kind;
        }

        public void AppendToPathString(StringBuilder builder) {
            Argument.NotNull(nameof(builder), builder);
            var first = true;
            foreach (var segment in Segments) {
                if (!first)
                    builder.Append("/");
                segment.AppendToString(builder);
                first = false;
            }
        }

        public override string ToString() => this.ToPathString();
    }
}