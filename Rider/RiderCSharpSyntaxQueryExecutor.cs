using System;
using System.Collections.Generic;
using SourcePath.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Impl.Tree;

namespace SourcePath.Roslyn {
    using static SyntaxQueryKeyword;
    using static CSharpTokenType;
    using static ElementType;
    using System.Linq;

    public class RiderCSharpSyntaxQueryExecutor : SyntaxQueryExecutorBase<ITreeNode, IEnumerable<ITreeNode>, ITreeNode> {
        private static readonly IReadOnlyDictionary<SyntaxQueryKeyword, HashSet<NodeType>> NodeTypesByKeyword = new Dictionary<SyntaxQueryKeyword, HashSet<NodeType>> {
            // Language
            { Abstract, HashSet(ABSTRACT_KEYWORD) },
            { Alias, HashSet(ALIAS_KEYWORD) },
            { As, HashSet(AS_EXPRESSION) },
            { Ascending, HashSet(ASCENDING_KEYWORD) },
            //{ Assembly, HashSet(ASSEMBLY_KEYWORD) },
            { Async, HashSet(ASYNC_KEYWORD) },
            { Await, HashSet(AWAIT_EXPRESSION) },
            { Base, HashSet(BASE_EXPRESSION, BASE_KEYWORD) },
            { If, HashSet(IF_STATEMENT) },
            { Bool, HashSet(BOOL_KEYWORD) },
            { Break, HashSet(BREAK_STATEMENT, BREAK_KEYWORD) },
            { By, HashSet(BY_KEYWORD) },
            { Byte, HashSet(BYTE_KEYWORD) },
            { Case, HashSet(CASE_KEYWORD) },
            { Catch, HashSet(GENERAL_CATCH_CLAUSE, SPECIFIC_CATCH_CLAUSE, CATCH_KEYWORD) },
            { Char, HashSet(CHAR_KEYWORD) },
            { Checked, HashSet(CHECKED_EXPRESSION, CHECKED_STATEMENT, CHECKED_KEYWORD) },
            { Class, HashSet(CLASS_DECLARATION, /* CLASS_CONSTRAINT,*/ CLASS_KEYWORD) },
            { Const, HashSet(CONST_KEYWORD) },
            { Continue, HashSet(CONTINUE_STATEMENT, CONTINUE_KEYWORD) },
            { Decimal, HashSet(DECIMAL_KEYWORD) },
            { Default, HashSet(DEFAULT_EXPRESSION, DEFAULT_KEYWORD) },
            { Delegate, HashSet(DELEGATE_DECLARATION, DELEGATE_KEYWORD, ANONYMOUS_METHOD_EXPRESSION) },
            { Descending, HashSet(DESCENDING_KEYWORD) },
            { Do, HashSet(DO_STATEMENT, DO_KEYWORD) },
            { Double, HashSet(DOUBLE_KEYWORD) },
            { Else, HashSet(PP_ELSE_SECTION, ELSE_KEYWORD) },
            { Enum, HashSet(ENUM_DECLARATION, ENUM_KEYWORD) },
            { SyntaxQueryKeyword.Equals, HashSet(EQUALS_KEYWORD) },
            { Event, HashSet(EVENT_DECLARATION, EVENT_KEYWORD) },
            { Explicit, HashSet(EXPLICIT_KEYWORD) },
            { Extern, HashSet(EXTERN_ALIAS_DIRECTIVE, EXTERN_KEYWORD) },
            { False, HashSet(FALSE_KEYWORD) },
            // { Field, HashSet(FIELD_KEYWORD) },
            { Finally, HashSet(/*FINALLY_CLAUSE,*/ FINALLY_KEYWORD) },
            { Fixed, HashSet(UNSAFE_CODE_FIXED_STATEMENT, FIXED_KEYWORD) },
            { Float, HashSet(FLOAT_KEYWORD) },
            { For, HashSet(FOR_STATEMENT, FOR_KEYWORD) },
            { Foreach, HashSet(FOREACH_STATEMENT, FOREACH_KEYWORD) },
            { From, HashSet(QUERY_FIRST_FROM, QUERY_FROM_CLAUSE, FROM_KEYWORD) },
            { Goto, HashSet(GOTO_CASE_STATEMENT, GOTO_STATEMENT, GOTO_KEYWORD) },
            { Group, HashSet(QUERY_GROUP_CLAUSE, GROUP_KEYWORD) },
            { Implicit, HashSet(IMPLICIT_KEYWORD) },
            { In, HashSet(IN_KEYWORD) },
            { Int, HashSet(INT_KEYWORD) },
            { Interface, HashSet(INTERFACE_DECLARATION, INTERFACE_KEYWORD) },
            { Internal, HashSet(INTERNAL_KEYWORD) },
            { Into, HashSet(INTO_KEYWORD) },
            { Is, HashSet(IS_EXPRESSION, IS_KEYWORD) },
            { Join, HashSet(QUERY_JOIN_CLAUSE, JOIN_KEYWORD) },
            { Let, HashSet(QUERY_LET_CLAUSE, LET_KEYWORD) },
            { Lock, HashSet(LOCK_STATEMENT, LOCK_KEYWORD) },
            { Long, HashSet(LONG_KEYWORD) },
            { Method, HashSet(METHOD_DECLARATION/*, METHOD_KEYWORD*/) },
            //{ Module, HashSet(MODULE_KEYWORD) },
            { NameOf, HashSet(/*NAMEOF_KEYWORD*/) },
            { Namespace, HashSet(C_SHARP_NAMESPACE_DECLARATION, NAMESPACE_KEYWORD) },
            { New, HashSet(ANONYMOUS_OBJECT_CREATION_EXPRESSION, ARRAY_CREATION_EXPRESSION, OBJECT_CREATION_EXPRESSION) },
            { Null, HashSet(NULL_KEYWORD) },
            { Object, HashSet(OBJECT_KEYWORD) },
            { On, HashSet(ON_KEYWORD) },
            { Operator, HashSet(CONVERSION_OPERATOR_DECLARATION, SIGN_OPERATOR_DECLARATION, OPERATOR_KEYWORD) },
            { OrderBy, HashSet(QUERY_ORDER_BY_CLAUSE, ORDERBY_KEYWORD) },
            { Out, HashSet(OUT_KEYWORD) },
            { Override, HashSet(OVERRIDE_KEYWORD) },
            //{ Param, HashSet(PARAM_KEYWORD) },
            { Params, HashSet(PARAMS_KEYWORD) },
            { Partial, HashSet(PARTIAL_KEYWORD) },
            { Private, HashSet(PRIVATE_KEYWORD) },
            //{ Property, HashSet(PROPERTY_KEYWORD) },
            { Protected, HashSet(PROTECTED_KEYWORD) },
            { Public, HashSet(PUBLIC_KEYWORD) },
            { Readonly, HashSet(READONLY_KEYWORD) },
            { Ref, HashSet(REF_KEYWORD) },
            { Return, HashSet(RETURN_STATEMENT, RETURN_KEYWORD) },
            { SByte, HashSet(SBYTE_KEYWORD) },
            { Sealed, HashSet(SEALED_KEYWORD) },
            { Select, HashSet(QUERY_SELECT_CLAUSE, SELECT_KEYWORD) },
            { Short, HashSet(SHORT_KEYWORD) },
            { Sizeof, HashSet(UNSAFE_CODE_SIZE_OF_EXPRESSION, SIZEOF_KEYWORD) },
            { Stackalloc, HashSet(UNSAFE_CODE_STACK_ALLOC_INITIALIZER, STACKALLOC_KEYWORD) },
            { Static, HashSet(STATIC_KEYWORD) },
            { String, HashSet(STRING_KEYWORD) },
            { Struct, HashSet(STRUCT_DECLARATION, STRUCT_KEYWORD) },
            { Switch, HashSet(SWITCH_STATEMENT, SWITCH_KEYWORD) },
            { This, HashSet(THIS_EXPRESSION, THIS_KEYWORD) },
            { Throw, HashSet(THROW_STATEMENT, THROW_EXPRESSION, THROW_KEYWORD) },
            { True, HashSet(TRUE_KEYWORD) },
            { Try, HashSet(TRY_STATEMENT, TRY_KEYWORD) },
            //{ Type, HashSet(TYPE_KEYWORD) },
            { TypeOf, HashSet(TYPEOF_EXPRESSION, TYPEOF_KEYWORD) },
            //{ Typevar, HashSet(TYPEVAR_KEYWORD) },
            { UInt, HashSet(UINT_KEYWORD) },
            { ULong, HashSet(ULONG_KEYWORD) },
            { Unchecked, HashSet(UNCHECKED_STATEMENT, UNCHECKED_EXPRESSION, UNCHECKED_KEYWORD) },
            { Unsafe, HashSet(UNSAFE_CODE_UNSAFE_STATEMENT, UNSAFE_KEYWORD) },
            { UShort, HashSet(USHORT_KEYWORD) },
            { Using, HashSet(USING_ALIAS_DIRECTIVE, USING_SYMBOL_DIRECTIVE, USING_STATEMENT, USING_KEYWORD) },
            { Virtual, HashSet(VIRTUAL_KEYWORD) },
            { Void, HashSet(VOID_KEYWORD) },
            { Volatile, HashSet(VOLATILE_KEYWORD) },
            { When, HashSet(EXCEPTION_FILTER_CLAUSE, WHEN_KEYWORD) },
            { Where, HashSet(QUERY_WHERE_CLAUSE, WHERE_KEYWORD) },
            { While, HashSet(WHILE_STATEMENT, WHILE_KEYWORD) },
            { Yield, HashSet(YIELD_STATEMENT, YIELD_KEYWORD) },

            //// Extras
            { Name, HashSet(IDENTIFIER) }
        };

        private static readonly HashSet<NodeType> ChildrenSkipNodeTypes = new HashSet<NodeType> {
            C_SHARP_FILE, CLASS_BODY, DECLARATION_STATEMENT, MODIFIERS_LIST, MULTIPLE_CONSTANT_DECLARATION, USING_LIST
        };

        private static HashSet<NodeType> HashSet(params NodeType[] types) {
            return new HashSet<NodeType>(types);
        }

        protected override bool ShouldJumpOver(ITreeNode node) {
            return ChildrenSkipNodeTypes.Contains(node.NodeType);
        }

        protected override ITreeNode GetDirectParent(ITreeNode node) => node.Parent;
        protected override IEnumerable<ITreeNode> GetDirectChildren(ITreeNode node) => node.Children();
        protected override ITreeNode AsNode(ITreeNode node) => node;
        protected override ITreeNode ToNodeOrToken(ITreeNode node) => node;

        protected override bool IsExpressionStatement(ITreeNode node, out ITreeNode expression) {
            expression = (node as IExpressionStatement)?.Expression;
            return expression != null;
        }

        protected override bool IsSwitchSection(ITreeNode node, out IEnumerable<ITreeNode> labels) {
            labels = (node as ISwitchSection)?.CaseLabels;
            return labels != null;
        }

        protected override bool IsPredefinedType(ITreeNode node, out ITreeNode keyword) {
            keyword = null;
            return false;
        }

        protected override bool HasOtherNodeTypeDefiningChildNode(ITreeNode node, out ITreeNode child) {
            child = null;
            return false;
        }
        
        protected override bool MatchesNodeType(ITreeNode node, SyntaxQuery query) {
            switch (query.Keyword) {
                case Add: return IsAccessorWithName("add");
                case Get: return IsAccessorWithName("get");
                case Remove: return IsAccessorWithName("remove");
                case Set: return IsAccessorWithName("set");

                case Base: if (IsConstructorInitializer(ConstructorInitializerKind.BASE)) return true; break;
                case This: if (IsConstructorInitializer(ConstructorInitializerKind.THIS)) return true; break;

                case Event: {
                    if (node is IMultipleEventDeclaration events && events.Children().OfType<IEventDeclaration>().Count() == 1)
                        return true;
                    break;
                }

                case Default: if (IsCaseLabel(isDefault: true)) return true; break;
                case Case: if (IsCaseLabel(isDefault: false)) return true; break;

                case Global: return node is IIdentifier identifier && identifier.Name == "global";
            }

            return GetNodeTypes(query).Contains(node.NodeType);

            bool IsAccessorWithName(string name) {
                return node is IAccessorDeclaration accessor
                    && accessor.NameIdentifier.Name == name;
            }

            bool IsConstructorInitializer(ConstructorInitializerKind kind) {
                return node is IConstructorInitializer initializer
                    && initializer.Kind == kind;
            }

            bool IsCaseLabel(bool isDefault) {
                return node is ISwitchCaseLabel caseLabel
                    && caseLabel.IsDefault == isDefault;
            }
        }

        protected override string AsIdentifierToString(ITreeNode node) {
            return null;
        }

        private static HashSet<NodeType> GetNodeTypes(SyntaxQuery query) {
            if (!NodeTypesByKeyword.TryGetValue(query.Keyword, out var types))
                throw new NotSupportedException($"Unsupported query keyword: {query.Keyword}.");
            return types;
        }
    }
}
