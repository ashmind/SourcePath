using Moq;
using Xunit;

namespace SourcePath.Tests.Unit {
    public class SourcePathParserTests {
        [Fact]
        public void Parse_Kind() {
            var path = NewParser().Parse("if");

            var segment = AssertSingleSegmentSequence(path);
            Assert.Equal("if", segment.Kind.ToPathString());
        }

        [Theory]
        [InlineData("if || if")]
        [InlineData("if && if")]
        public void Parse_TopLevelBinary(string pathAsString) {
            var path = NewParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToPathString());
        }

        [Theory]
        [InlineData("if", null)]
        [InlineData("/if", SourcePathAxis.Child)]
        [InlineData("//if", SourcePathAxis.Descendant)]
        [InlineData("descendant::if", SourcePathAxis.Descendant)]
        [InlineData("self::if", SourcePathAxis.Self)]
        [InlineData("parent::if", SourcePathAxis.Parent)]
        [InlineData("ancestor::if", SourcePathAxis.Ancestor)]
        public void Parse_Axis(string pathAsString, SourcePathAxis? expectedAxis) {
            var path = NewParser().Parse(pathAsString);

            var segment = AssertSingleSegmentSequence(path);
            Assert.Equal(expectedAxis, segment.Axis);
        }

        [Theory]
        [InlineData("if[if]")]
        [InlineData("if[if && if]")]
        [InlineData("if[if && if && if]")]
        [InlineData("if[if && if[if && if]]")]
        [InlineData("if[if || if]")]
        //[InlineData("class[name == 'C']")]
        //[InlineData("class[name == 'C' && method[name == 'M']]")]
        public void Parse_Filter(string pathAsString) {
            var path = NewParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToPathString());
        }

        [Theory]
        [InlineData("class[name[.StartsWith('c')]]")]
        public void Parse_Filter_Call(string pathAsString) {
            var path = NewParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToPathString());
        }

        [Theory]
        [InlineData("if/if")]
        [InlineData("if/if/if")]
        [InlineData("self::if/parent::if/self::if")]
        public void Parse_Path(string pathAsString) {
            var path = NewParser().Parse(pathAsString);
            Assert.Equal(pathAsString, path.ToPathString());
        }

        private SourcePathParser<_> NewParser() {
            return new SourcePathParser<_>(
                MockDialect(),
                Mock.Of<ISourceNodeHandler<_>>()
            );
        }

        private ISourcePathDialect<_> MockDialect() {
            var mock = new Mock<ISourcePathDialect<_>>();
            mock.Setup(m => m.ResolveNodeKind(It.IsAny<string>()))
                .Returns((string kindString) => MockNodeKind(kindString));
            return mock.Object;
        }

        private ISourceNodeKind<_> MockNodeKind(string kindString) {
            var mock = new Mock<ISourceNodeKind<_>>();
            mock.Setup(m => m.ToPathString())
                .Returns(kindString);
            return mock.Object;
        }

        private SourcePathSegment<_> AssertSingleSegmentSequence(ISourcePath<_> path) {
            var sequence = Assert.IsType<SourcePathSequence<_>>(path);
            return Assert.Single(sequence.Segments);
        }

        public class _ {}
    }
}
