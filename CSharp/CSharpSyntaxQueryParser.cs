using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Sprache;

namespace Lastql.CSharp {
    public class CSharpSyntaxQueryParser {
        private static readonly IReadOnlyDictionary<string, HashSet<SyntaxKind>> SyntaxKindsByKeywords = new Dictionary<string, HashSet<SyntaxKind>> {
            { "abstract", new HashSet<SyntaxKind> { SyntaxKind.AbstractKeyword } },
            { "add", new HashSet<SyntaxKind> { SyntaxKind.AddAccessorDeclaration, SyntaxKind.AddKeyword } },
            { "alias", new HashSet<SyntaxKind> { SyntaxKind.AliasKeyword } },
            { "as", new HashSet<SyntaxKind> { SyntaxKind.AsExpression } },
            { "ascending", new HashSet<SyntaxKind> { SyntaxKind.AscendingKeyword } },
            { "assembly", new HashSet<SyntaxKind> { SyntaxKind.AssemblyKeyword } },
            { "async", new HashSet<SyntaxKind> { SyntaxKind.AsyncKeyword } },
            { "await", new HashSet<SyntaxKind> { SyntaxKind.AwaitExpression } },
            { "base", new HashSet<SyntaxKind> { SyntaxKind.BaseExpression, SyntaxKind.BaseConstructorInitializer } },
            { "if", new HashSet<SyntaxKind> { SyntaxKind.IfStatement } },
            { "bool", new HashSet<SyntaxKind> { SyntaxKind.BoolKeyword } },
            { "break", new HashSet<SyntaxKind> { SyntaxKind.BreakStatement, SyntaxKind.BreakKeyword } },
            { "by", new HashSet<SyntaxKind> { SyntaxKind.ByKeyword } },
            { "byte", new HashSet<SyntaxKind> { SyntaxKind.ByteKeyword } },
            { "case", new HashSet<SyntaxKind> { SyntaxKind.CaseSwitchLabel, SyntaxKind.CaseKeyword } },
            { "catch", new HashSet<SyntaxKind> { SyntaxKind.CatchClause, SyntaxKind.CatchDeclaration } },
            { "char", new HashSet<SyntaxKind> { SyntaxKind.CharKeyword } },
            { "checked", new HashSet<SyntaxKind> { SyntaxKind.CheckedExpression, SyntaxKind.CheckedStatement, SyntaxKind.CheckedKeyword } },
            { "class", new HashSet<SyntaxKind> { SyntaxKind.ClassDeclaration, SyntaxKind.ClassConstraint, SyntaxKind.ClassKeyword } },
            { "const", new HashSet<SyntaxKind> { SyntaxKind.ConstKeyword } },
            { "continue", new HashSet<SyntaxKind> { SyntaxKind.ContinueStatement, SyntaxKind.ContinueKeyword } },
            { "decimal", new HashSet<SyntaxKind> { SyntaxKind.DecimalKeyword } },
            { "default", new HashSet<SyntaxKind> { SyntaxKind.DefaultExpression, SyntaxKind.DefaultLiteralExpression, SyntaxKind.DefaultKeyword, SyntaxKind.DefaultSwitchLabel } },
            { "delegate", new HashSet<SyntaxKind> { SyntaxKind.DelegateDeclaration, SyntaxKind.DelegateKeyword, SyntaxKind.AnonymousMethodExpression } },
            { "descending", new HashSet<SyntaxKind> { SyntaxKind.DescendingKeyword } },
            { "do", new HashSet<SyntaxKind> { SyntaxKind.DoStatement, SyntaxKind.DoKeyword } },
            { "double", new HashSet<SyntaxKind> { SyntaxKind.DoubleKeyword } },
            { "else", new HashSet<SyntaxKind> { SyntaxKind.ElseClause, SyntaxKind.ElseKeyword } },
            { "enum", new HashSet<SyntaxKind> { SyntaxKind.EnumDeclaration, SyntaxKind.EnumKeyword } },
            { "equals", new HashSet<SyntaxKind> { SyntaxKind.EqualsKeyword } },
            { "event", new HashSet<SyntaxKind> { SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration, SyntaxKind.EventKeyword } },
            { "explicit", new HashSet<SyntaxKind> { SyntaxKind.ExplicitKeyword } },
            { "extern", new HashSet<SyntaxKind> { SyntaxKind.ExternAliasDirective, SyntaxKind.ExternKeyword } },
            { "false", new HashSet<SyntaxKind> { SyntaxKind.FalseLiteralExpression, SyntaxKind.FalseKeyword } },
            { "field", new HashSet<SyntaxKind> { SyntaxKind.FieldKeyword } },
            { "finally", new HashSet<SyntaxKind> { SyntaxKind.FinallyClause, SyntaxKind.FinallyKeyword } },
            { "fixed", new HashSet<SyntaxKind> { SyntaxKind.FixedStatement, SyntaxKind.FixedKeyword } },
            { "float", new HashSet<SyntaxKind> { SyntaxKind.FloatKeyword } },
            { "for", new HashSet<SyntaxKind> { SyntaxKind.ForStatement, SyntaxKind.ForKeyword } },
            { "foreach", new HashSet<SyntaxKind> { SyntaxKind.ForEachStatement, SyntaxKind.ForEachKeyword } },
            { "from", new HashSet<SyntaxKind> { SyntaxKind.FromClause, SyntaxKind.FromKeyword } },
            { "get", new HashSet<SyntaxKind> { SyntaxKind.GetAccessorDeclaration, SyntaxKind.GetKeyword } },
            { "global", new HashSet<SyntaxKind> { SyntaxKind.GlobalKeyword } },
            { "goto", new HashSet<SyntaxKind> { SyntaxKind.GotoCaseStatement, SyntaxKind.GotoDefaultStatement, SyntaxKind.GotoStatement, SyntaxKind.GotoKeyword } },
            { "group", new HashSet<SyntaxKind> { SyntaxKind.GroupClause, SyntaxKind.GroupKeyword } },
            { "implicit", new HashSet<SyntaxKind> { SyntaxKind.ImplicitKeyword } },
            { "in", new HashSet<SyntaxKind> { SyntaxKind.InKeyword } },
            { "int", new HashSet<SyntaxKind> { SyntaxKind.IntKeyword } },
            { "interface", new HashSet<SyntaxKind> { SyntaxKind.InterfaceDeclaration, SyntaxKind.InterfaceKeyword } },
            { "internal", new HashSet<SyntaxKind> { SyntaxKind.InternalKeyword } },
            { "into", new HashSet<SyntaxKind> { SyntaxKind.IntoKeyword } },
            { "is", new HashSet<SyntaxKind> { SyntaxKind.IsExpression, SyntaxKind.IsPatternExpression, SyntaxKind.IsKeyword } },
            { "join", new HashSet<SyntaxKind> { SyntaxKind.JoinClause, SyntaxKind.JoinIntoClause, SyntaxKind.JoinKeyword } },
            { "let", new HashSet<SyntaxKind> { SyntaxKind.LetClause, SyntaxKind.LetKeyword } },
            { "lock", new HashSet<SyntaxKind> { SyntaxKind.LockStatement, SyntaxKind.LockKeyword } },
            { "long", new HashSet<SyntaxKind> { SyntaxKind.LongKeyword } },
            { "method", new HashSet<SyntaxKind> { SyntaxKind.MethodKeyword } },
            { "module", new HashSet<SyntaxKind> { SyntaxKind.ModuleKeyword } },
            { "nameof", new HashSet<SyntaxKind> { SyntaxKind.NameOfKeyword } },
            { "namespace", new HashSet<SyntaxKind> { SyntaxKind.NamespaceDeclaration, SyntaxKind.NamespaceKeyword } },
            { "new", new HashSet<SyntaxKind> {
                SyntaxKind.AnonymousObjectCreationExpression,
                SyntaxKind.ArrayCreationExpression,
                SyntaxKind.ImplicitArrayCreationExpression,
                SyntaxKind.ObjectCreationExpression
            } },
            { "null", new HashSet<SyntaxKind> { SyntaxKind.NullLiteralExpression, SyntaxKind.NullKeyword } },
            { "object", new HashSet<SyntaxKind> { SyntaxKind.ObjectKeyword } },
            { "on", new HashSet<SyntaxKind> { SyntaxKind.OnKeyword } },
            { "operator", new HashSet<SyntaxKind> { SyntaxKind.OperatorDeclaration, SyntaxKind.OperatorKeyword } },
            { "orderby", new HashSet<SyntaxKind> { SyntaxKind.OrderByClause, SyntaxKind.OrderByKeyword } },
            { "out", new HashSet<SyntaxKind> { SyntaxKind.OutKeyword } },
            { "override", new HashSet<SyntaxKind> { SyntaxKind.OverrideKeyword } },
            { "param", new HashSet<SyntaxKind> { SyntaxKind.ParamKeyword } },
            { "params", new HashSet<SyntaxKind> { SyntaxKind.ParamsKeyword } },
            { "partial", new HashSet<SyntaxKind> { SyntaxKind.PartialKeyword } },
            { "private", new HashSet<SyntaxKind> { SyntaxKind.PrivateKeyword } },
            { "property", new HashSet<SyntaxKind> { SyntaxKind.PropertyKeyword } },
            { "protected", new HashSet<SyntaxKind> { SyntaxKind.ProtectedKeyword } },
            { "public", new HashSet<SyntaxKind> { SyntaxKind.PublicKeyword } },
            { "readonly", new HashSet<SyntaxKind> { SyntaxKind.ReadOnlyKeyword } },
            { "ref", new HashSet<SyntaxKind> { SyntaxKind.RefKeyword } },
            { "remove", new HashSet<SyntaxKind> { SyntaxKind.RemoveAccessorDeclaration, SyntaxKind.RemoveKeyword } },
            { "return", new HashSet<SyntaxKind> { SyntaxKind.ReturnStatement, SyntaxKind.ReturnKeyword } },
            { "sbyte", new HashSet<SyntaxKind> { SyntaxKind.SByteKeyword } },
            { "sealed", new HashSet<SyntaxKind> { SyntaxKind.SealedKeyword } },
            { "select", new HashSet<SyntaxKind> { SyntaxKind.SelectClause, SyntaxKind.SelectKeyword } },
            { "set", new HashSet<SyntaxKind> { SyntaxKind.SetAccessorDeclaration, SyntaxKind.SetKeyword } },
            { "short", new HashSet<SyntaxKind> { SyntaxKind.ShortKeyword } },
            { "sizeof", new HashSet<SyntaxKind> { SyntaxKind.SizeOfExpression, SyntaxKind.SizeOfKeyword } },
            { "stackalloc", new HashSet<SyntaxKind> { SyntaxKind.StackAllocArrayCreationExpression, SyntaxKind.StackAllocKeyword } },
            { "static", new HashSet<SyntaxKind> { SyntaxKind.StaticKeyword } },
            { "string", new HashSet<SyntaxKind> { SyntaxKind.StringKeyword } },
            { "struct", new HashSet<SyntaxKind> { SyntaxKind.StructDeclaration, SyntaxKind.StructKeyword } },
            { "switch", new HashSet<SyntaxKind> { SyntaxKind.SwitchStatement, SyntaxKind.SwitchKeyword } },
            { "this", new HashSet<SyntaxKind> { SyntaxKind.ThisConstructorInitializer, SyntaxKind.ThisExpression, SyntaxKind.ThisKeyword } },
            { "throw", new HashSet<SyntaxKind> { SyntaxKind.ThrowStatement, SyntaxKind.ThrowExpression, SyntaxKind.ThrowKeyword } },
            { "true", new HashSet<SyntaxKind> { SyntaxKind.TrueLiteralExpression, SyntaxKind.TrueKeyword } },
            { "try", new HashSet<SyntaxKind> { SyntaxKind.TryStatement, SyntaxKind.TryKeyword } },
            { "type", new HashSet<SyntaxKind> { SyntaxKind.TypeKeyword } },
            { "typeof", new HashSet<SyntaxKind> { SyntaxKind.TypeOfExpression, SyntaxKind.TypeOfKeyword } },
            { "typevar", new HashSet<SyntaxKind> { SyntaxKind.TypeVarKeyword } },
            { "uint", new HashSet<SyntaxKind> { SyntaxKind.UIntKeyword } },
            { "ulong", new HashSet<SyntaxKind> { SyntaxKind.ULongKeyword } },
            { "unchecked", new HashSet<SyntaxKind> { SyntaxKind.UncheckedStatement, SyntaxKind.UncheckedExpression, SyntaxKind.UncheckedKeyword } },
            { "unsafe", new HashSet<SyntaxKind> { SyntaxKind.UnsafeStatement, SyntaxKind.UnsafeKeyword } },
            { "ushort", new HashSet<SyntaxKind> { SyntaxKind.UShortKeyword } },
            { "using", new HashSet<SyntaxKind> { SyntaxKind.UsingDirective, SyntaxKind.UsingStatement, SyntaxKind.UsingKeyword } },
            { "virtual", new HashSet<SyntaxKind> { SyntaxKind.VirtualKeyword } },
            { "void", new HashSet<SyntaxKind> { SyntaxKind.VoidKeyword } },
            { "volatile", new HashSet<SyntaxKind> { SyntaxKind.VolatileKeyword } },
            { "when", new HashSet<SyntaxKind> { SyntaxKind.WhenClause, SyntaxKind.CatchFilterClause, SyntaxKind.WhenKeyword } },
            { "where", new HashSet<SyntaxKind> { SyntaxKind.WhereClause, SyntaxKind.WhereKeyword } },
            { "while", new HashSet<SyntaxKind> { SyntaxKind.WhileStatement, SyntaxKind.WhileKeyword } },
            { "yield", new HashSet<SyntaxKind> { SyntaxKind.YieldReturnStatement, SyntaxKind.YieldBreakStatement, SyntaxKind.YieldKeyword } }
        };

        private static readonly Parser<SyntaxQueryAxis> Axis = Sprache.Parse
            .String("self::").Select(s => SyntaxQueryAxis.Self)
            .Or(Sprache.Parse.String("//").Select(s => SyntaxQueryAxis.Descendant))
            .Or(Sprache.Parse.String("/").Select(s => SyntaxQueryAxis.Child));
        private static readonly Parser<string> Keyword = Sprache.Parse
            .Identifier(Sprache.Parse.Letter, Sprache.Parse.Letter);

        private static readonly Parser<CSharpSyntaxQuery> Root = Sprache.Parse
            .Optional(Axis)
            .Then(a => Keyword.Select(s => new CSharpSyntaxQuery(a.GetOrElse(SyntaxQueryAxis.Child), ConvertKeywordToKinds(s))));

        public CSharpSyntaxQuery Parse(string query) {
            return Root.Parse(query);
        }

        private static HashSet<SyntaxKind> ConvertKeywordToKinds(string keyword) {
            if (!SyntaxKindsByKeywords.TryGetValue(keyword, out var kind))
                throw new FormatException($"Unknown C# keyword: {keyword}.");
            return kind;
        }
    }
}
