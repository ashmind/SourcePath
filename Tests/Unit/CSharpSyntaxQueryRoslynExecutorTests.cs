using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Lastql.CSharp;
using Lastql.Roslyn;

namespace Lastql.Tests.Unit {
    public class CSharpSyntaxQueryRoslynExecutorTests {
        [Theory]
        [InlineData("//as", "var x = \"x\" as object;", "\"x\" as object")]
        [InlineData("//ascending", "var y = from x in xs orderby x.X ascending select x;", "ascending")]
        [InlineData("self::await", "await M();", "await M();")]
        [InlineData("//base", "base.M();", "base")]
        [InlineData("self::break", "break;", "break;")]
        [InlineData("break", "yield break;", "break")]
        [InlineData("//by", "var y = from x in xs group x by x.X;", "by")]
        [InlineData("catch", "try { M(); } catch (E e) {}", "catch (E e) {}")]
        [InlineData("//checked", "var x = checked(y + 1);", "checked(y + 1)")]
        [InlineData("self::checked", "checked { var x = y + 1; }", "checked { var x = y + 1; }")]
        [InlineData("const", "const int x = 0;", "const")]
        [InlineData("self::continue", "continue;", "continue;")]
        [InlineData("//default", "var x = default(int);", "default(int)")]
        [InlineData("//default", "int x = default;", "default")]
        [InlineData("//delegate", "A x = delegate { M(); };", "delegate { M(); }")]
        [InlineData("//descending", "var y = from x in xs orderby x.X descending select x;", "descending")]
        [InlineData("self::do", "do {} while (true);", "do {} while (true);")]
        [InlineData("//else", "if (true) {} else {}", "else {}")]
        [InlineData("//equals", "var y = from x1 in xs join x2 in xs on x1 equals x2 select x1;", "equals")]
        [InlineData("//false", "var x = false;", "false")]
        [InlineData("//finally", "try { M(); } finally {}", "finally {}")]
        [InlineData("self::fixed", "fixed (int* y = &x) {}", "fixed (int* y = &x) {}")]
        [InlineData("self::for", "for (var i = 0; i < 5; i++) {}", "for (var i = 0; i < 5; i++) {}")]
        [InlineData("self::foreach", "foreach (var x in xs) {}", "foreach (var x in xs) {}")]
        [InlineData("//from", "var y = from x in xs select x;", "from x in xs")]
        [InlineData("//global", "global::X.M();", "global")]
        [InlineData("//goto", "L: goto L;", "goto L;")]
        [InlineData("//goto", "switch(x) { case 1: goto case 1; }", "goto case 1;")]
        [InlineData("//goto", "switch(x) { default: goto default; }", "goto default;")]
        [InlineData("//group", "var y = from x in xs group x by x.X;", "group x by x.X")]
        [InlineData("self::if", "if (true) {}", "if (true) {}")]
        [InlineData("//in", "foreach (var x in xs) {}", "in")]
        [InlineData("//into", "var y = from x in xs group x by x into g select g;", "into")]
        [InlineData("//is", "var y = x is Y;", "x is Y")]
        [InlineData("//is", "if (x is Y y) {}", "x is Y y")]
        [InlineData("//join", "var y = from x1 in xs join x2 in xs on x1 equals x2 select x1;", "join x2 in xs on x1 equals x2")]
        [InlineData("//let", "var y = from x in xs let y = x select y;", "let y = x")]
        [InlineData("self::lock", "lock(x) {}", "lock(x) {}")]
        //[InlineData("//nameof", "var y = nameof(X);", "nameof(X)")]
        [InlineData("//new", "var x = new X();", "new X()")]
        [InlineData("//new", "var x = new X { P = 0 };", "new X { P = 0 }")]
        [InlineData("//new", "var x = new X[0];", "new X[0]")]
        [InlineData("//new", "var x = new[] { x };", "new[] { x }")]
        [InlineData("//new", "var x = new { X = 0 };", "new { X = 0 }")]
        [InlineData("//null", "object x = null;", "null")]
        [InlineData("//on", "var y = from x1 in xs join x2 in xs on x1 equals x2 select x1;", "on")]
        [InlineData("//orderby", "var y = from x in xs orderby x.X ascending select x;", "orderby x.X ascending")]
        [InlineData("self::return", "return 0;", "return 0;")]
        [InlineData("//return", "yield return 0;", "return")]
        [InlineData("//select", "var y = from x in xs select x;", "select x")]
        [InlineData("//sizeof", "var x = sizeof(int);", "sizeof(int)")]
        [InlineData("//stackalloc", "var x = stackalloc int[5];", "stackalloc int[5]")]
        [InlineData("self::switch", "switch (x) { default: break; }", "switch (x) { default: break; }")]
        [InlineData("//this", "this.M();", "this")]
        [InlineData("self::throw", "throw e;", "throw e;")]
        [InlineData("//throw", "var y = x ?? throw e;", "throw e")]
        [InlineData("//true", "var x = true;", "true")]
        [InlineData("self::try", "try { M(); } catch {}", "try { M(); } catch {}")]
        [InlineData("//unchecked", "var x = unchecked(y + 1);", "unchecked(y + 1)")]
        [InlineData("self::unsafe", "unsafe {}", "unsafe {}")]
        [InlineData("self::using", "using (x) {}", "using (x) {}")]
        [InlineData("//when", "try { M(); } catch (E e) when (e != null) {}", "when (e != null)")]
        [InlineData("//where", "var y = from x in xs where x.X select x;", "where x.X")]
        [InlineData("self::while", "while (x) {}", "while (x) {}")]
        //[InlineData("//while", "do {} while (true);", "while (true)")]
        [InlineData("self::yield", "yield return 0;", "yield return 0;")]
        [InlineData("self::yield", "yield break;", "yield break;")]
        public void QueryAll_Statement(string query, string code, string expected) {
            TestQueryAll(new[] { expected }, TestSyntaxFactory.ParseStatement(code), query);
        }

        [Theory]
        [InlineData("bool")]
        [InlineData("byte")]
        [InlineData("char")]
        [InlineData("decimal")]
        [InlineData("double")]
        [InlineData("float")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("object")]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("string")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("ushort")]
        public void QueryAll_PrimitiveType(string type) {
            TestQueryAll(
                new[] { type },
                TestSyntaxFactory.ParseStatement($"{type} x;"),
                $"//{type}"
            );
        }

        [Theory]
        [InlineData("//identifier", "int x = 5;", "x")]
        public void QueryAll_Expression_SpecialCategory(string query, string code, string expected) {
            TestQueryAll(new[] { expected }, TestSyntaxFactory.ParseCompilationUnit(code), query);
        }

        [Theory]
        [InlineData("//case", "switch (x) { case 'A': break; }", new[] { "case 'A': break;" })]
        [InlineData("//case", "switch (x) { default: break; }", new string[0])]
        [InlineData("//default", "switch (x) { default: break; }", new[] { "default: break;" })]
        [InlineData("//default", "switch (x) { case 'A': break; }", new string[0])]
        public void QueryAll_SwitchSection(string query, string code, string[] expected) {
            TestQueryAll(expected, TestSyntaxFactory.ParseStatement(code), query);
        }

        [Theory]
        [InlineData("abstract", "abstract class X {}", "abstract")]
        [InlineData("//add", "class X { event Action E { add {} remove {} } }", "add {}")]
        [InlineData("alias", "extern alias X;", "alias")]
        [InlineData("//assembly", "[assembly: A]", "assembly")]
        [InlineData("//async", "class X { async void M() {} }", "async")]
        [InlineData("//base", "class Y : X { Y() : base(1) {} }", ": base(1)")]
        [InlineData("self::class", "class X {}", "class X {}")]
        [InlineData("self::delegate", "delegate void A();", "delegate void A();")]
        [InlineData("self::enum", "enum E { A }", "enum E { A }")]
        [InlineData("//event", "class X { event Action e; }", "event Action e;")]
        [InlineData("//event", "class X { event Action E { add {} remove {} } }", "event Action E { add {} remove {} }")]
        [InlineData("//explicit", "class X { public static explicit operator X(string value) { return null; } }", "explicit")]
        [InlineData("self::extern", "extern alias X;", "extern alias X;")]
        [InlineData("//field", "class X { [field: A] int f; }", "field")]
        [InlineData("//get", "class X { int P { get; set; } }", "get;")]
        [InlineData("//get", "class X { int P { get => 0; set {} } }", "get => 0;")]
        [InlineData("//implicit", "class X { public static implicit operator X(string value) { return null; } }", "implicit")]
        [InlineData("self::interface", "interface I {}", "interface I {}")]
        [InlineData("internal", "internal class X {}", "internal")]
        //[InlineData("//method", "class X { [method: A] void M() {} }", "method")]
        [InlineData("//module", "[module: A]", "module")]
        [InlineData("self::namespace", "namespace N {}", "namespace N {}")]
        [InlineData("//operator", "class X { static X operator -(X value) { return null; } }", "static X operator -(X value) { return null; }")]
        [InlineData("//out", "class X { void M(out int x) { x = 0; } }", "out")]
        [InlineData("//override", "class Y : X { protected override void M() {} }", "override")]
        [InlineData("//param", "class X { void M([param: A] int x) {} }", "param")]
        [InlineData("//params", "class X { void M(params int[] xs) {} }", "params")]
        [InlineData("partial", "partial class X {}", "partial")]
        [InlineData("//private", "class X { private void M() {} }", "private")]
        [InlineData("//property", "class X { [property: A] int P { get; set; } }", "property")]
        [InlineData("//protected", "class X { protected void M() {} }", "protected")]
        [InlineData("public", "public class X {}", "public")]
        [InlineData("//readonly", "class X { readonly int f; }", "readonly")]
        [InlineData("//ref", "class X { void M(ref int x) {} }", "ref")]
        [InlineData("//remove", "class X { event Action E { add {} remove {} } }", "remove {}")]
        [InlineData("sealed", "sealed class X {}", "sealed")]
        [InlineData("//set", "class X { int P { get; set; } }", "set;")]
        [InlineData("//set", "class X { int P { get => 0; set {} } }", "set {}")]
        [InlineData("static", "static class X {}", "static")]
        [InlineData("self::struct", "struct S {}", "struct S {}")]
        [InlineData("//this", "class X { X() : this(1) {} }", ": this(1)")]
        [InlineData("//type", "[type: A] class X {}", "type")]
        [InlineData("//typevar", "class X<[typevar: A] T> {}", "typevar")]
        [InlineData("//unsafe", "class X { unsafe void M() {} }", "unsafe")]
        [InlineData("self::using", "using N;", "using N;")]
        [InlineData("self::using", "using A = N;", "using A = N;")]
        [InlineData("//virtual", "class X { protected virtual void M() {} }", "virtual")]
        [InlineData("//void", "class X { void M() {} }", "void")]
        [InlineData("//volatile", "class X { volatile int f; }", "volatile")]
        public void QueryAll_Declaration(string query, string code, string expected) {
            TestQueryAll(new[] { expected }, TestSyntaxFactory.ParseCompilationUnit(code), query);
        }

        [Theory]
        [InlineData("method", "class X { void M() {} }", "void M() {}")]
        public void QueryAll_Declaration_SpecialCategory(string query, string code, string expected) {
            TestQueryAll(new[] { expected }, TestSyntaxFactory.ParseCompilationUnit(code), query);
        }

        private static void TestQueryAll(string[] expected, CSharpSyntaxNode current, string queryAsString) {
            var results = new CSharpSyntaxQueryRoslynExecutor().QueryAll(current, ParseQuery(queryAsString));
            Assert.Equal(expected, results.Select(r => r.ToString()).ToArray());
        }

        private static SyntaxQuery ParseQuery(string queryAsString) {
            return new SyntaxQueryParser().Parse(queryAsString);
        }
    }
}
