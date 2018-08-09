using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourcePath.Roslyn {
    using static SyntaxKind;
    using Argument = global::Argument;

    public class RoslynCSharpKeywordBasedPathDialect : ISourcePathDialect<RoslynNodeContext> {
        private static IReadOnlyDictionary<string, RoslynCSharpNodeKind> Keywords { get; } = new[] {
            // Language
            Keyword("abstract", AbstractKeyword),
            Keyword("add", AddAccessorDeclaration, AddKeyword),
            Keyword("alias", AliasKeyword),
            Keyword("as", AsExpression),
            Keyword("ascending", AscendingKeyword),
            Keyword("assembly", AssemblyKeyword),
            Keyword("async", AsyncKeyword),
            Keyword("await", AwaitExpression),
            Keyword("base", BaseExpression, BaseConstructorInitializer),
            Keyword("if", IfStatement),
            Keyword("bool", BoolKeyword),
            Keyword("break", BreakStatement, BreakKeyword),
            Keyword("by", ByKeyword),
            Keyword("byte", ByteKeyword),
            Keyword("case",
                CaseSwitchLabel,
                #if !VSIX
                CasePatternSwitchLabel,
                #endif
                CaseKeyword
            ),
            Keyword("catch", CatchClause, CatchDeclaration),
            Keyword("char", CharKeyword),
            Keyword("checked", CheckedExpression, CheckedStatement, CheckedKeyword),
            Keyword("class", ClassDeclaration, ClassConstraint, ClassKeyword),
            Keyword("const", ConstKeyword),
            Keyword("continue", ContinueStatement, ContinueKeyword),
            Keyword("decimal", DecimalKeyword),
            Keyword("default",
                DefaultExpression,
                #if !VSIX
                DefaultLiteralExpression,
                #endif
                DefaultKeyword,
                DefaultSwitchLabel
            ),
            Keyword("delegate", DelegateDeclaration, DelegateKeyword, AnonymousMethodExpression),
            Keyword("descending", DescendingKeyword),
            Keyword("do", DoStatement, DoKeyword),
            Keyword("double", DoubleKeyword),
            Keyword("else", ElseClause, ElseKeyword),
            Keyword("enum", EnumDeclaration, EnumKeyword),
            Keyword("syntaxPathKeyword.Equals", EqualsKeyword),
            Keyword("event", EventDeclaration, EventFieldDeclaration, EventKeyword),
            Keyword("explicit", ExplicitKeyword),
            Keyword("extern", ExternAliasDirective, ExternKeyword),
            Keyword("false", FalseLiteralExpression, FalseKeyword),
            Keyword("field", FieldKeyword),
            Keyword("finally", FinallyClause, FinallyKeyword),
            Keyword("fixed", FixedStatement, FixedKeyword),
            Keyword("float", FloatKeyword),
            Keyword("for", ForStatement, ForKeyword),
            Keyword("foreach", ForEachStatement, ForEachKeyword),
            Keyword("from", FromClause, FromKeyword),
            Keyword("get", GetAccessorDeclaration, GetKeyword),
            Keyword("global", GlobalKeyword),
            Keyword("goto", GotoCaseStatement, GotoDefaultStatement, GotoStatement, GotoKeyword),
            Keyword("group", GroupClause, GroupKeyword),
            Keyword("implicit", ImplicitKeyword),
            Keyword("in", InKeyword),
            Keyword("int", IntKeyword),
            Keyword("interface", InterfaceDeclaration, InterfaceKeyword),
            Keyword("internal", InternalKeyword),
            Keyword("into", IntoKeyword),
            Keyword("is",
                IsExpression,
                #if !VSIX
                IsPatternExpression,
                #endif
                IsKeyword
            ),
            Keyword("join", JoinClause, JoinIntoClause, JoinKeyword),
            Keyword("let", LetClause, LetKeyword),
            Keyword("lock", LockStatement, LockKeyword),
            Keyword("long", LongKeyword),
            Keyword("method", MethodDeclaration, MethodKeyword),
            Keyword("module", ModuleKeyword),
            Keyword("nameOf", NameOfKeyword),
            Keyword("namespace", NamespaceDeclaration, NamespaceKeyword),
            Keyword("new",
               AnonymousObjectCreationExpression,
               ArrayCreationExpression,
               ImplicitArrayCreationExpression,
               ObjectCreationExpression
            ),
            Keyword("null", NullLiteralExpression, NullKeyword),
            Keyword("object", ObjectKeyword),
            Keyword("on", OnKeyword),
            Keyword("operator", OperatorDeclaration, OperatorKeyword),
            Keyword("orderby", OrderByClause, OrderByKeyword),
            Keyword("out", OutKeyword),
            Keyword("override", OverrideKeyword),
            Keyword("param", ParamKeyword, Parameter),
            Keyword("params", ParamsKeyword),
            Keyword("partial", PartialKeyword),
            Keyword("private", PrivateKeyword),
            Keyword("property", PropertyKeyword),
            Keyword("protected", ProtectedKeyword),
            Keyword("public", PublicKeyword),
            Keyword("readonly", ReadOnlyKeyword),
            Keyword("ref", RefKeyword),
            Keyword("remove", RemoveAccessorDeclaration, RemoveKeyword),
            Keyword("return", ReturnStatement, ReturnKeyword),
            Keyword("sbyte", SByteKeyword),
            Keyword("sealed", SealedKeyword),
            Keyword("select", SelectClause, SelectKeyword),
            Keyword("set", SetAccessorDeclaration, SetKeyword),
            Keyword("short", ShortKeyword),
            Keyword("sizeof", SizeOfExpression, SizeOfKeyword),
            Keyword("stackalloc", StackAllocArrayCreationExpression, StackAllocKeyword),
            Keyword("static", StaticKeyword),
            Keyword("string", StringKeyword),
            Keyword("struct", StructDeclaration, StructKeyword),
            Keyword("switch", SwitchStatement, SwitchKeyword),
            Keyword("this", ThisConstructorInitializer, ThisExpression, ThisKeyword),
            Keyword("throw",
                ThrowStatement,
                #if !VSIX
                ThrowExpression,
                #endif
                ThrowKeyword
            ),
            Keyword("true", TrueLiteralExpression, TrueKeyword),
            Keyword("try", TryStatement, TryKeyword),
            Keyword("type", TypeKeyword),
            Keyword("typeof", TypeOfExpression, TypeOfKeyword),
            Keyword("typevar", TypeVarKeyword),
            Keyword("uint", UIntKeyword),
            Keyword("ulong", ULongKeyword),
            Keyword("unchecked", UncheckedStatement, UncheckedExpression, UncheckedKeyword),
            Keyword("unsafe", UnsafeStatement, UnsafeKeyword),
            Keyword("uShort", UShortKeyword),
            Keyword("using", UsingDirective, UsingStatement, UsingKeyword),
            Keyword("virtual", VirtualKeyword),
            Keyword("void", VoidKeyword),
            Keyword("volatile", VolatileKeyword),
            Keyword("when", 
                #if !VSIX
                WhenClause,
                #endif
                CatchFilterClause,
                WhenKeyword
            ),
            Keyword("where", WhereClause, WhereKeyword),
            Keyword("while", WhileStatement, WhileKeyword),
            Keyword("yield", YieldReturnStatement, YieldBreakStatement, YieldKeyword),

            // Extras
            Keyword("name", IdentifierName, IdentifierToken),
            Keyword("lambda", ParenthesizedLambdaExpression, SimpleLambdaExpression),
            Keyword("tuple", TupleType, TupleExpression)
        }.ToDictionary(
            k => k.Name,
            k => k
        );

        public RoslynCSharpKeywordBasedPathDialect() {
        }

        public RoslynCSharpKeywordBasedPathDialect(SourcePathDialectSupports supports) {
            Supports = supports;
        }

        public SourcePathDialectSupports Supports { get; private set; }

        public ISourceNodeKind<RoslynNodeContext> ResolveNodeKind(string nodeKindString) {
            Argument.NotNullOrEmpty(nameof(nodeKindString), nodeKindString);
            if (!Keywords.TryGetValue(nodeKindString, out var nodeKind))
                return null;
            return nodeKind;
        }

        private static RoslynCSharpNodeKind Keyword(string name, params SyntaxKind[] kinds) {
            return new RoslynCSharpNodeKind(name, new HashSet<SyntaxKind>(kinds));
        }
    }
}
