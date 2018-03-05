using System;
using System.Text;

namespace SourcePath.CSharp {
    public class SyntaxFilterBinaryExpression : ISyntaxFilterExpression {
        public SyntaxFilterBinaryExpression(ISyntaxFilterExpression left, SyntaxFilterBinaryOperator @operator, ISyntaxFilterExpression right) {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public ISyntaxFilterExpression Left { get; set; }
        public SyntaxFilterBinaryOperator Operator { get; set; }
        public ISyntaxFilterExpression Right { get; set; }

        public void AppendToString(StringBuilder builder) {
            Left.AppendToString(builder);
            switch (Operator) {
                case SyntaxFilterBinaryOperator.And: builder.Append(" && "); break;
                case SyntaxFilterBinaryOperator.Equals: builder.Append(" == "); break;
                default: throw new NotSupportedException($"Unknown binary operator: {Operator}.");
            }
            Right.AppendToString(builder);
        }
    }
}