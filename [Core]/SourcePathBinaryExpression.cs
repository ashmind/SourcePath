using System;
using System.Text;

namespace SourcePath {
    using static SourcePathBinaryOperator;

    public class SourcePathBinaryExpression<TNode> : ISourcePath<TNode> {
        public SourcePathBinaryExpression(ISourcePath<TNode> left, SourcePathBinaryOperator @operator, ISourcePath<TNode> right) {
            Left = Argument.NotNull(nameof(left), left);
            Operator = @operator;
            Right = Argument.NotNull(nameof(right), right);
        }

        public ISourcePath<TNode> Left { get; set; }
        public SourcePathBinaryOperator Operator { get; set; }
        public ISourcePath<TNode> Right { get; set; }

        public bool Matches(TNode node, SourcePathAxis defaultAxis) {
            switch (Operator) {
                case And: return Left.Matches(node, defaultAxis) && Right.Matches(node, defaultAxis);
                case Or: return Left.Matches(node, defaultAxis) || Right.Matches(node, defaultAxis);
                default: throw new NotSupportedException($"Unknown binary operator: {Operator}.");
            }
        }

        public void AppendToPathString(StringBuilder builder) {
            Left.AppendToPathString(builder);
            switch (Operator) {
                case And: builder.Append(" && "); break;
                case Or: builder.Append(" || "); break;
                default: throw new NotSupportedException($"Unknown binary operator: {Operator}.");
            }
            Right.AppendToPathString(builder);
        }
    }
}