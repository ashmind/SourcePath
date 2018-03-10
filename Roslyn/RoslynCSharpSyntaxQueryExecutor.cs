using System;
using System.Collections.Generic;
using SourcePath.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourcePath.Roslyn {
    using static SyntaxQueryKeyword;
    using static SyntaxKind;

    public class RoslynCSharpSyntaxQueryExecutor : SyntaxQueryExecutorBase<SyntaxNodeOrToken, ChildSyntaxList, SyntaxNode> {
        private static readonly IReadOnlyDictionary<SyntaxQueryKeyword, HashSet<SyntaxKind>> SyntaxKindsByKeyword = new Dictionary<SyntaxQueryKeyword, HashSet<SyntaxKind>> {
            // Language
            { Abstract, HashSet(AbstractKeyword) },
            { Add, HashSet(AddAccessorDeclaration, AddKeyword) },
            { Alias, HashSet(AliasKeyword) },
            { As, HashSet(AsExpression) },
            { Ascending, HashSet(AscendingKeyword) },
            { Assembly, HashSet(AssemblyKeyword) },
            { Async, HashSet(AsyncKeyword) },
            { Await, HashSet(AwaitExpression) },
            { Base, HashSet(BaseExpression, BaseConstructorInitializer) },
            { If, HashSet(IfStatement) },
            { Bool, HashSet(BoolKeyword) },
            { Break, HashSet(BreakStatement, BreakKeyword) },
            { By, HashSet(ByKeyword) },
            { Byte, HashSet(ByteKeyword) },
            { Case, HashSet(CaseSwitchLabel, CaseKeyword) },
            { Catch, HashSet(CatchClause, CatchDeclaration) },
            { Char, HashSet(CharKeyword) },
            { Checked, HashSet(CheckedExpression, CheckedStatement, CheckedKeyword) },
            { Class, HashSet(ClassDeclaration, ClassConstraint, ClassKeyword) },
            { Const, HashSet(ConstKeyword) },
            { Continue, HashSet(ContinueStatement, ContinueKeyword) },
            { Decimal, HashSet(DecimalKeyword) },
            { Default, HashSet(
                DefaultExpression,
                #if !VSIX
                DefaultLiteralExpression,
                #endif
                DefaultKeyword,
                DefaultSwitchLabel
            ) },
            { Delegate, HashSet(DelegateDeclaration, DelegateKeyword, AnonymousMethodExpression) },
            { Descending, HashSet(DescendingKeyword) },
            { Do, HashSet(DoStatement, DoKeyword) },
            { Double, HashSet(DoubleKeyword) },
            { Else, HashSet(ElseClause, ElseKeyword) },
            { Enum, HashSet(EnumDeclaration, EnumKeyword) },
            { SyntaxQueryKeyword.Equals, HashSet(EqualsKeyword) },
            { Event, HashSet(EventDeclaration, EventFieldDeclaration, EventKeyword) },
            { Explicit, HashSet(ExplicitKeyword) },
            { Extern, HashSet(ExternAliasDirective, ExternKeyword) },
            { False, HashSet(FalseLiteralExpression, FalseKeyword) },
            { Field, HashSet(FieldKeyword) },
            { Finally, HashSet(FinallyClause, FinallyKeyword) },
            { Fixed, HashSet(FixedStatement, FixedKeyword) },
            { Float, HashSet(FloatKeyword) },
            { For, HashSet(ForStatement, ForKeyword) },
            { Foreach, HashSet(ForEachStatement, ForEachKeyword) },
            { From, HashSet(FromClause, FromKeyword) },
            { Get, HashSet(GetAccessorDeclaration, GetKeyword) },
            { Global, HashSet(GlobalKeyword) },
            { Goto, HashSet(GotoCaseStatement, GotoDefaultStatement, GotoStatement, GotoKeyword) },
            { Group, HashSet(GroupClause, GroupKeyword) },
            { Implicit, HashSet(ImplicitKeyword) },
            { In, HashSet(InKeyword) },
            { Int, HashSet(IntKeyword) },
            { Interface, HashSet(InterfaceDeclaration, InterfaceKeyword) },
            { Internal, HashSet(InternalKeyword) },
            { Into, HashSet(IntoKeyword) },
            { Is, HashSet(
                IsExpression,
                #if !VSIX
                IsPatternExpression,
                #endif
                IsKeyword
            ) },
            { Join, HashSet(JoinClause, JoinIntoClause, JoinKeyword) },
            { Let, HashSet(LetClause, LetKeyword) },
            { Lock, HashSet(LockStatement, LockKeyword) },
            { Long, HashSet(LongKeyword) },
            { Method, HashSet(MethodDeclaration, MethodKeyword) },
            { Module, HashSet(ModuleKeyword) },
            { NameOf, HashSet(NameOfKeyword) },
            { Namespace, HashSet(NamespaceDeclaration, NamespaceKeyword) },
            { New, HashSet(
               AnonymousObjectCreationExpression,
               ArrayCreationExpression,
               ImplicitArrayCreationExpression,
               ObjectCreationExpression
            ) },
            { Null, HashSet(NullLiteralExpression, NullKeyword) },
            { Object, HashSet(ObjectKeyword) },
            { On, HashSet(OnKeyword) },
            { Operator, HashSet(OperatorDeclaration, OperatorKeyword) },
            { OrderBy, HashSet(OrderByClause, OrderByKeyword) },
            { Out, HashSet(OutKeyword) },
            { Override, HashSet(OverrideKeyword) },
            { Param, HashSet(ParamKeyword) },
            { Params, HashSet(ParamsKeyword) },
            { Partial, HashSet(PartialKeyword) },
            { Private, HashSet(PrivateKeyword) },
            { Property, HashSet(PropertyKeyword) },
            { Protected, HashSet(ProtectedKeyword) },
            { Public, HashSet(PublicKeyword) },
            { Readonly, HashSet(ReadOnlyKeyword) },
            { Ref, HashSet(RefKeyword) },
            { Remove, HashSet(RemoveAccessorDeclaration, RemoveKeyword) },
            { Return, HashSet(ReturnStatement, ReturnKeyword) },
            { SByte, HashSet(SByteKeyword) },
            { Sealed, HashSet(SealedKeyword) },
            { Select, HashSet(SelectClause, SelectKeyword) },
            { Set, HashSet(SetAccessorDeclaration, SetKeyword) },
            { Short, HashSet(ShortKeyword) },
            { Sizeof, HashSet(SizeOfExpression, SizeOfKeyword) },
            { Stackalloc, HashSet(StackAllocArrayCreationExpression, StackAllocKeyword) },
            { Static, HashSet(StaticKeyword) },
            { String, HashSet(StringKeyword) },
            { Struct, HashSet(StructDeclaration, StructKeyword) },
            { Switch, HashSet(SwitchStatement, SwitchKeyword) },
            { This, HashSet(ThisConstructorInitializer, ThisExpression, ThisKeyword) },
            { Throw, HashSet(
                ThrowStatement,
                #if !VSIX
                ThrowExpression,
                #endif
                ThrowKeyword
            ) },
            { True, HashSet(TrueLiteralExpression, TrueKeyword) },
            { Try, HashSet(TryStatement, TryKeyword) },
            { Type, HashSet(TypeKeyword) },
            { TypeOf, HashSet(TypeOfExpression, TypeOfKeyword) },
            { Typevar, HashSet(TypeVarKeyword) },
            { UInt, HashSet(UIntKeyword) },
            { ULong, HashSet(ULongKeyword) },
            { Unchecked, HashSet(UncheckedStatement, UncheckedExpression, UncheckedKeyword) },
            { Unsafe, HashSet(UnsafeStatement, UnsafeKeyword) },
            { UShort, HashSet(UShortKeyword) },
            { Using, HashSet(UsingDirective, UsingStatement, UsingKeyword) },
            { Virtual, HashSet(VirtualKeyword) },
            { Void, HashSet(VoidKeyword) },
            { Volatile, HashSet(VolatileKeyword) },
            { When, HashSet(
                #if !VSIX
                WhenClause,
                #endif
                CatchFilterClause,
                WhenKeyword
            ) },
            { Where, HashSet(WhereClause, WhereKeyword) },
            { While, HashSet(WhileStatement, WhileKeyword) },
            { Yield, HashSet(YieldReturnStatement, YieldBreakStatement, YieldKeyword) },

            // Extras
            { Name, HashSet(IdentifierName, IdentifierToken) }
        };

        public IEnumerable<SyntaxKind> GetRootSyntaxKinds(SyntaxQuery query) {
            return GetSyntaxKinds(query);
        }

        private static HashSet<SyntaxKind> HashSet(params SyntaxKind[] kinds) {
            return new HashSet<SyntaxKind>(kinds);
        }

        protected override bool ShouldJumpOver(SyntaxNode node) {
            return node.Kind() == CompilationUnit;
        }

        protected override SyntaxNode GetDirectParent(SyntaxNodeOrToken nodeOrToken) => nodeOrToken.Parent;
        protected override ChildSyntaxList GetDirectChildren(SyntaxNodeOrToken nodeOrToken) => nodeOrToken.AsNode()?.ChildNodesAndTokens() ?? new ChildSyntaxList();
        protected override SyntaxNodeOrToken ToNodeOrToken(SyntaxNode node) => node;
        protected override SyntaxNode AsNode(SyntaxNodeOrToken nodeOrToken) => nodeOrToken.AsNode();

        protected override bool IsExpressionStatement(SyntaxNodeOrToken nodeOrToken, out SyntaxNode expression) {
            expression = nodeOrToken.AsNode() as ExpressionStatementSyntax;
            return expression != null;
        }

        protected override bool IsSwitchSection(SyntaxNodeOrToken nodeOrToken, out IEnumerable<SyntaxNode> labels) {
            labels = (nodeOrToken.AsNode() as SwitchSectionSyntax)?.Labels;
            return labels != null;
        }

        protected override bool IsPredefinedType(SyntaxNodeOrToken nodeOrToken, out SyntaxNodeOrToken keyword) {
            keyword = (nodeOrToken.AsNode() as PredefinedTypeSyntax)?.Keyword ?? default;
            return keyword != default;
        }

        protected override bool MatchesNodeType(SyntaxNodeOrToken nodeOrToken, SyntaxQuery query) {
            return GetSyntaxKinds(query).Contains(nodeOrToken.Kind());
        }

        protected override string AsIdentifierToString(SyntaxNodeOrToken nodeOrToken) {
            if (nodeOrToken.AsNode() is IdentifierNameSyntax identifierName)
                return identifierName.Identifier.Text;
            if (nodeOrToken.Kind() == IdentifierToken)
                return nodeOrToken.AsToken().Text;
            return null;
        }

        private static HashSet<SyntaxKind> GetSyntaxKinds(SyntaxQuery query) {
            if (!SyntaxKindsByKeyword.TryGetValue(query.Keyword, out var kinds))
                throw new NotSupportedException($"Unsupported query keyword: {query.Keyword}.");
            return kinds;
        }
    }
}
