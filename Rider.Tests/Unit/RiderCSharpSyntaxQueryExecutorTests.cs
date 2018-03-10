extern alias xunit_assert;

using System;
using System.Linq;
using System.Reflection;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp.ConstantValues;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Text;
using Moq;
using SourcePath.Roslyn;
using SourcePath.Tests;
using SourcePath.Tests.Unit;

using Assert = xunit_assert::Xunit.Assert;

namespace SourcePath.Rider.Tests.Unit {
    public class RiderCSharpSyntaxQueryExecutorTests : RoslynCSharpSyntaxQueryExecutorTests {
        protected override void TestQueryAllReturnsExpected(string code, TestSourceKind codeKind, string query, params string[] expected) {
            var results = new RiderCSharpSyntaxQueryExecutor().QueryAll(
                ParseCSharp(code, codeKind), ParseQuery(query)
            );
            Assert.Equal(expected, results.Select(r => r.GetText()).ToArray());
        }

        private ITreeNode ParseCSharp(string code, TestSourceKind codeKind) {
            if (codeKind == TestSourceKind.Statement)
                return ParseCSharpStatement(code);

            var language = new TestCSharpLanguage();
            var languageService = new CSharpLanguageService(
                language,
                new CSharpConstantValueService(new CSharpConstantValuePresenter(language)),
                Mock.Of<ICSharpCodeFormatter>(),
                new CommonIdentifierIntern()
            );
            language.SetLanguageService(languageService);
            typeof(CSharpLanguage).GetField("Instance", BindingFlags.Public | BindingFlags.Static).SetValue(null, language);
            var file = MockPsiSourceFile();
            return languageService.ParseFile(new CSharpLexer(new StringBuffer(code)), file);
        }

        private ITreeNode ParseCSharpStatement(string code) {
            var fileCode = "class C { async void M() { " + code + " } }";
            var fileNode = ParseCSharp(fileCode, TestSourceKind.CompilationUnit);
            return fileNode.Descendants<IMethodDeclaration>().First().Body.Statements.First();
        }

        private IPsiSourceFile MockPsiSourceFile() {
            var mock = new Mock<IPsiSourceFile>(MockBehavior.Strict);
            mock.Setup(x => x.Name).Returns("_.cs");
            mock.Setup(x => x.IsValid()).Returns(true);
            mock.Setup(x => x.ResolveContext.IsValid()).Returns(true);
            mock.Setup(x => x.PsiModule).Returns(new Mock<IPsiModule>(MockBehavior.Strict).Object);
            mock.Setup(x => x.Properties.GetDefines()).Returns(Array.Empty<PreProcessingDirective>());
            return mock.Object;
        }

        private class TestCSharpLanguage : CSharpLanguage {
            public TestCSharpLanguage() : base(Name) {
            }

            public void SetLanguageService(CSharpLanguageService languageService) {
                typeof(PsiLanguageType).GetField("myService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, languageService);
                typeof(PsiLanguageType).GetField("myServiceAsked", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, true);
            }
        }
    }
}
