namespace Lastql.CSharp {
    public class CSharpSyntaxQuery {
        public CSharpSyntaxQuery(SyntaxQueryAxis axis, CSharpSyntaxQueryTarget target) {
            Axis = axis;
            Target = target;
        }

        public SyntaxQueryAxis Axis { get; }
        public CSharpSyntaxQueryTarget Target { get; }
    }
}