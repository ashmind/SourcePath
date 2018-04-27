using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Impl.Tree;
using SourcePath.Rider.Internal;

namespace SourcePath.Rider {
    using static CSharpTokenType;
    using static ElementType;

    public class RiderCSharpKeywordBasedPathDialect : ISourcePathDialect<ITreeNode> {
        private static IReadOnlyDictionary<string, RiderNodeKind> Keywords { get; } = new[] {
            // Language
            Keyword("abstract", ABSTRACT_KEYWORD),
            //Keyword("add", AddAccessorDeclaration, AddKeyword),
            Keyword("alias", ALIAS_KEYWORD),
            Keyword("as", AS_EXPRESSION),
            Keyword("ascending", ASCENDING_KEYWORD),
            //Keyword("assembly", AssemblyKeyword),
            Keyword("async", ASYNC_KEYWORD),
            Keyword("await", AWAIT_EXPRESSION),
            Keyword("base", BASE_EXPRESSION, BASE_KEYWORD),
            Keyword("if", IF_STATEMENT),
            Keyword("bool", BOOL_KEYWORD),
            Keyword("break", BREAK_STATEMENT, BREAK_KEYWORD),
            Keyword("by", BY_KEYWORD),
            Keyword("byte", BYTE_KEYWORD),
            Keyword("case", CASE_KEYWORD),
            Keyword("catch", GENERAL_CATCH_CLAUSE, SPECIFIC_CATCH_CLAUSE, CATCH_KEYWORD),
            Keyword("char", CHAR_KEYWORD),
            Keyword("checked", CHECKED_EXPRESSION, CHECKED_STATEMENT, CHECKED_KEYWORD),
            Keyword("class", CLASS_DECLARATION, /* CLASS_CONSTRAINT,*/ CLASS_KEYWORD),
            Keyword("const", CONST_KEYWORD),
            Keyword("continue", CONTINUE_STATEMENT, CONTINUE_KEYWORD),
            Keyword("decimal", DECIMAL_KEYWORD),
            Keyword("default", DEFAULT_EXPRESSION, DEFAULT_KEYWORD),
            Keyword("delegate", DELEGATE_DECLARATION, DELEGATE_KEYWORD, ANONYMOUS_METHOD_EXPRESSION),
            Keyword("descending", DESCENDING_KEYWORD),
            Keyword("do", DO_STATEMENT, DO_KEYWORD),
            Keyword("double", DOUBLE_KEYWORD),
            Keyword("else", ELSE_KEYWORD),
            Keyword("enum", ENUM_DECLARATION, ENUM_KEYWORD),
            Keyword("syntaxPathKeyword.Equals", EQUALS_KEYWORD),
            Keyword("event", EVENT_DECLARATION, EVENT_KEYWORD),
            Keyword("explicit", EXPLICIT_KEYWORD),
            Keyword("extern", EXTERN_ALIAS_DIRECTIVE, EXTERN_KEYWORD),
            Keyword("false", FALSE_KEYWORD),
            //Keyword("field", FIELD_KEYWORD),
            Keyword("finally", /*FINALLY_CLAUSE,*/ FINALLY_KEYWORD),
            Keyword("fixed", UNSAFE_CODE_FIXED_STATEMENT, FIXED_KEYWORD),
            Keyword("float", FLOAT_KEYWORD),
            Keyword("for", FOR_STATEMENT, FOR_KEYWORD),
            Keyword("foreach", FOREACH_STATEMENT, FOREACH_KEYWORD),
            Keyword("from", QUERY_FIRST_FROM, QUERY_FROM_CLAUSE, FROM_KEYWORD),
            //Keyword("get", GetAccessorDeclaration, GetKeyword),
            //Keyword("global", GlobalKeyword),
            Keyword("goto", GOTO_CASE_STATEMENT, GOTO_STATEMENT, GOTO_KEYWORD),
            Keyword("group", QUERY_GROUP_CLAUSE, GROUP_KEYWORD),
            Keyword("implicit", IMPLICIT_KEYWORD),
            Keyword("in", IN_KEYWORD),
            Keyword("int", INT_KEYWORD),
            Keyword("interface", INTERFACE_DECLARATION, INTERFACE_KEYWORD),
            Keyword("internal", INTERNAL_KEYWORD),
            Keyword("into", INTO_KEYWORD),
            Keyword("is", IS_EXPRESSION, IS_KEYWORD),
            Keyword("join", QUERY_JOIN_CLAUSE, JOIN_KEYWORD),
            Keyword("let", QUERY_LET_CLAUSE, LET_KEYWORD),
            Keyword("lock", LOCK_STATEMENT, LOCK_KEYWORD),
            Keyword("long", LONG_KEYWORD),
            Keyword("method", METHOD_DECLARATION/*, METHOD_KEYWORD*/),
            //Keyword("module", ModuleKeyword),
            //Keyword("nameOf", NAMEOF_KEYWORD),
            Keyword("namespace", C_SHARP_NAMESPACE_DECLARATION, NAMESPACE_KEYWORD),
            Keyword("new", ANONYMOUS_OBJECT_CREATION_EXPRESSION, ARRAY_CREATION_EXPRESSION, OBJECT_CREATION_EXPRESSION),
            Keyword("null", NULL_KEYWORD),
            Keyword("object", OBJECT_KEYWORD),
            Keyword("on", ON_KEYWORD),
            Keyword("operator", CONVERSION_OPERATOR_DECLARATION, SIGN_OPERATOR_DECLARATION, OPERATOR_KEYWORD),
            Keyword("orderby", QUERY_ORDER_BY_CLAUSE, ORDERBY_KEYWORD),
            Keyword("out", OUT_KEYWORD),
            Keyword("override", OVERRIDE_KEYWORD),
            //Keyword("param", ParamKeyword, Parameter),
            Keyword("params", PARAMS_KEYWORD),
            Keyword("partial", PARTIAL_KEYWORD),
            Keyword("private", PRIVATE_KEYWORD),
            //Keyword("property", PropertyKeyword),
            Keyword("protected", PROTECTED_KEYWORD),
            Keyword("public", PUBLIC_KEYWORD),
            Keyword("readonly", READONLY_KEYWORD),
            Keyword("ref", REF_KEYWORD),
            //Keyword("remove", RemoveAccessorDeclaration, RemoveKeyword),
            Keyword("return", RETURN_STATEMENT, RETURN_KEYWORD),
            Keyword("sbyte", SBYTE_KEYWORD),
            Keyword("sealed", SEALED_KEYWORD),
            Keyword("select", QUERY_SELECT_CLAUSE, SELECT_KEYWORD),
            //Keyword("set", SetAccessorDeclaration, SetKeyword),
            Keyword("short", SHORT_KEYWORD),
            Keyword("sizeof", UNSAFE_CODE_SIZE_OF_EXPRESSION, SIZEOF_KEYWORD),
            Keyword("stackalloc", UNSAFE_CODE_STACK_ALLOC_INITIALIZER, STACKALLOC_KEYWORD),
            Keyword("static", STATIC_KEYWORD),
            Keyword("string", STRING_KEYWORD),
            Keyword("struct", STRUCT_DECLARATION, STRUCT_KEYWORD),
            Keyword("switch", SWITCH_STATEMENT, SWITCH_KEYWORD),
            Keyword("this", THIS_EXPRESSION, THIS_KEYWORD),
            Keyword("throw", THROW_STATEMENT, THROW_EXPRESSION, THROW_KEYWORD),
            Keyword("true", TRUE_KEYWORD),
            Keyword("try", TRY_STATEMENT, TRY_KEYWORD),
            //Keyword("type", TypeKeyword),
            Keyword("typeof", TYPEOF_EXPRESSION, TYPEOF_KEYWORD),
            //Keyword("typevar", TypeVarKeyword),
            Keyword("uint", UINT_KEYWORD),
            Keyword("ulong", ULONG_KEYWORD),
            Keyword("unchecked", UNCHECKED_STATEMENT, UNCHECKED_EXPRESSION, UNCHECKED_KEYWORD),
            Keyword("unsafe", UNSAFE_CODE_UNSAFE_STATEMENT, UNSAFE_KEYWORD),
            Keyword("ushort", USHORT_KEYWORD),
            Keyword("using", USING_ALIAS_DIRECTIVE, USING_SYMBOL_DIRECTIVE, USING_STATEMENT, USING_KEYWORD),
            Keyword("virtual", VIRTUAL_KEYWORD),
            Keyword("void", VOID_KEYWORD),
            Keyword("volatile", VOLATILE_KEYWORD),
            Keyword("when", EXCEPTION_FILTER_CLAUSE, WHEN_KEYWORD),
            Keyword("where", QUERY_WHERE_CLAUSE, WHERE_KEYWORD),
            Keyword("while", WHILE_STATEMENT, WHILE_KEYWORD),
            Keyword("yield", YIELD_STATEMENT, YIELD_KEYWORD),

            // Extras
            Keyword("name", IDENTIFIER),
            Keyword("lambda", LAMBDA_EXPRESSION),
            Keyword("tuple", TUPLE_DECLARED_TYPE_USAGE, TUPLE_TYPE_USAGE)
        }.ToDictionary(
            k => k.Name,
            k => k
        );

        public RiderCSharpKeywordBasedPathDialect() {
        }

        public RiderCSharpKeywordBasedPathDialect(SourcePathDialectSupports supports) {
            Supports = supports;
        }

        public SourcePathDialectSupports Supports { get; private set; }

        public ISourceNodeKind<ITreeNode> ResolveNodeKind(string nodeKindString) {
            Argument.NotNullOrEmpty(nameof(nodeKindString), nodeKindString);
            if (!Keywords.TryGetValue(nodeKindString, out var nodeKind))
                return null;
            return nodeKind;
        }

        private static RiderNodeKind Keyword(string name, params NodeType[] nodeTypes) {
            return new RiderNodeKind(name, new HashSet<NodeType>(nodeTypes));
        }
    }
}
