using System;
using System.Collections.Generic;
using System.Linq;
using SourcePath.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourcePath.Roslyn {
    using static SyntaxQueryKeyword;
    using static SyntaxKind;

    public class RoslynCSharpSyntaxQueryExecutor {
        private static readonly IReadOnlyDictionary<SyntaxQueryKeyword, HashSet<SyntaxKind>> SyntaxKindsByTarget = new Dictionary<SyntaxQueryKeyword, HashSet<SyntaxKind>> {
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

        public IEnumerable<SyntaxNodeOrToken> QueryAll(CSharpSyntaxNode current, SyntaxQuery query) {
            if (current is CompilationUnitSyntax)
                return current.ChildNodes().SelectMany(c => QueryAll((CSharpSyntaxNode)c, query));

            switch (query.Axis) {
                case SyntaxQueryAxis.Self:
                    if (MatchesIgnoringAxis(current, query))
                        return Enumerable.Repeat((SyntaxNodeOrToken)current, 1);
                    return Enumerable.Empty<SyntaxNodeOrToken>();
                case SyntaxQueryAxis.Child:
                    return QueryAllChildrenOrDescendants(current, query, descendants: false);
                case SyntaxQueryAxis.Descendant:
                    return QueryAllChildrenOrDescendants(current, query, descendants: true);
                case SyntaxQueryAxis.Parent:
                    if (MatchesIgnoringAxis(current.Parent, query))
                        return Enumerable.Repeat((SyntaxNodeOrToken)current.Parent, 1);
                    return Enumerable.Empty<SyntaxNodeOrToken>();
                default:
                    throw new ArgumentException($"Unsupported query axis: {query.Axis}.", nameof(query));
            }
        }

        private IEnumerable<SyntaxNodeOrToken> QueryAllChildrenOrDescendants(SyntaxNode node, SyntaxQuery query, bool descendants) {
            foreach (var child in node.ChildNodesAndTokens()) {
                if (MatchesIgnoringAxis(child, query)) {
                    yield return child;
                    continue;
                }

                if (descendants && child.IsNode) {
                    foreach (var descendant in QueryAllChildrenOrDescendants(child.AsNode(), query, descendants: true)) {
                        yield return descendant;
                    }
                }
            }
        }

        private bool MatchesIgnoringAxis(SyntaxNodeOrToken nodeOrToken, SyntaxQuery query) {
            var node = nodeOrToken.AsNode();
            if (node is ExpressionStatementSyntax statement && node.Kind() == ExpressionStatement)
                return MatchesIgnoringAxis(statement.Expression, query);

            if (node is SwitchSectionSyntax switchSection) {
                foreach (var label in switchSection.Labels) {
                    if (MatchesSyntaxKindAndFilter(switchSection, label.Kind(), query))
                        return true;
                }
            }

            if (node is PredefinedTypeSyntax predefinedType)
                return MatchesSyntaxKindAndFilter(predefinedType, predefinedType.Keyword.Kind(), query);

            return MatchesSyntaxKindAndFilter(nodeOrToken, nodeOrToken.Kind(), query);
        }

        private bool MatchesSyntaxKindAndFilter(SyntaxNodeOrToken nodeOrToken, SyntaxKind syntaxKind, SyntaxQuery query) {
            return GetSyntaxKinds(query).Contains(syntaxKind)
                && MatchesFilter(nodeOrToken, query.Filter);
        }

        private bool MatchesFilter(SyntaxNodeOrToken nodeOrToken, ISyntaxFilterExpression filter) {
            if (filter == null)
                return true;
            switch (filter) {
                case SyntaxQuery query: {
                    var node = nodeOrToken.AsNode();
                    if (node == null)
                        return false; // TODO?

                    return QueryAll((CSharpSyntaxNode)node, query).Any();
                }

                case SyntaxFilterBinaryExpression binary:
                    return MatchesFilterBinary(nodeOrToken, binary);

                default:
                    throw new NotSupportedException($"Unknown filter type: {filter.GetType()}.");
            }
        }

        private bool MatchesFilterBinary(SyntaxNodeOrToken nodeOrToken, SyntaxFilterBinaryExpression binary) {
            switch (binary.Operator) {
                case SyntaxFilterBinaryOperator.And:
                    return MatchesFilter(nodeOrToken, binary.Left)
                        && MatchesFilter(nodeOrToken, binary.Right);

                case SyntaxFilterBinaryOperator.Equals:
                    return EvaluateToString(nodeOrToken, binary.Left)
                        == EvaluateToString(nodeOrToken, binary.Right);

                default:
                    throw new NotSupportedException($"Unknown binary operator: {binary.Operator}.");
            }
        }

        private string EvaluateToString(SyntaxNodeOrToken nodeOrToken, ISyntaxFilterExpression expression) {
            switch (expression) {
                case SyntaxQuery query: {
                    var node = nodeOrToken.AsNode();
                    if (node == null)
                        return null; // TODO?

                    var first = QueryAll((CSharpSyntaxNode)node, query).FirstOrDefault();
                    return EvaluateToString(first);
                }

                case SyntaxFilterLiteralExpression literal:
                    return literal.Value;

                default:
                    throw new NotSupportedException($"Expression canno be evaluated to a single value: {expression}.");
            }
        }

        private string EvaluateToString(SyntaxNodeOrToken nodeOrToken) {
            var node = nodeOrToken.AsNode();
            if (node != null) {
                if (node is IdentifierNameSyntax name)
                    return EvaluateToString(name.Identifier);

                return null;
            }

            return (string)nodeOrToken.AsToken().Value;
        }

        private static HashSet<SyntaxKind> GetSyntaxKinds(SyntaxQuery query) {
            if (!SyntaxKindsByTarget.TryGetValue(query.Keyword, out var kinds))
                throw new NotSupportedException($"Unsupported query keyword: {query.Keyword}.");
            return kinds;
        }
    }
}
