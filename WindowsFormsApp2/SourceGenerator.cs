using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Test.Extensions;

namespace WindowsFormsApp2
{
    internal class SourceGenerator
    {
        private static readonly string[] dotNetPrimitiveTypes = {
            "System.SByte",
            "System.Byte",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Int64",
            "System.UInt64",
            "System.Char",
            "System.Decimal",
            "System.Single",
            "System.Double",
            "System.Boolean",
            "System.Object",
        };

        private static readonly Func<SemanticModel, SyntaxNode> s_nodeGetter =
            m => m.SyntaxTree.GetRoot().DescendantNodes().Where(n => m.GetOperationInternal(n) is IUnaryOperatorExpression).FirstOrDefault();

        internal IEnumerable<GeneratedSource> Generates(string language)
        {
            if (language == LanguageNames.CSharp)
            {
                return GetCSharpUnaryPlusMinusExpression().Concat(
                    GetCSharpLogicalNotExpression()).Concat(
                    GetCSharpBitwiseNotExpression()).Concat(
                    GetCSharpDynamicExpression()).Concat(
                    GetCSharpEnum()).Concat(
                    GetCSharpCustomOperator()).Concat(
                    GetCSharpTrueFalseOperator());
            }
            else
            {
                return GetVBUnaryPlusMinusNotExpression().Concat(
                    GetVBEnum()).Concat(
                    GetVBCustomOperator()).Concat(
                    GetVBTrueFalseOperator());
            }
        }

        private IEnumerable<GeneratedSource> GetCSharpUnaryPlusMinusExpression()
        {
            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus") },
                dotNetPrimitiveTypes);
        }

        private IEnumerable<GeneratedSource> GetCSharpLogicalNotExpression()
        {
            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("!", "LogicalNot") },
                new[] { "System.Boolean" });
        }

        private IEnumerable<GeneratedSource> GetCSharpBitwiseNotExpression()
        {
            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("~", "BitwiseNot") },
                dotNetPrimitiveTypes);
        }

        private IEnumerable<GeneratedSource> GetCSharpDynamicExpression()
        {
            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("~", "BitwiseNot"), ("!", "LogicalNot") },
                new[] { "dynamic" });
        }

        private IEnumerable<GeneratedSource> GetCSharpEnum()
        {
            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("~", "BitwiseNot") },
                new[] { "Enum" },
                "enum Enum {{ A, B }}");
        }

        private IEnumerable<GeneratedSource> GetCSharpCustomOperator()
        {
            var code = @"public struct CustomType
{{
    public static CustomType operator +(CustomType x)
    {{
        return x;
    }}
    public static CustomType operator -(CustomType x)
    {{
        return x;
    }}
    public static CustomType operator !(CustomType x)
    {{
        return x;
    }}
    public static CustomType operator ~(CustomType x)
    {{
        return x;
    }}
}}";

            return GetCSharpUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("~", "BitwiseNot"), ("!", "LogicalNot") },
                new[] { "CustomType" },
                code);
        }

        private IEnumerable<GeneratedSource> GetCSharpTrueFalseOperator()
        {
            var code = @"
public struct S
{{
    private int value;
    public S(int v)
    {{
        value = v;
    }}
    public static S operator |(S x, S y)
    {{
        return new S(x.value - y.value);
    }}
    public static S operator &(S x, S y)
    {{
        return new S(x.value + y.value);
    }}
    public static bool operator true(S x)
    {{
        return x.value > 0;
    }}
    public static bool operator false(S x)
    {{
        return x.value <= 0;
    }}
}}
 
class C
{{
    public void M()
    {{
        var x = new S(2);
        var y = new S(1);
        if (x {0} y) {{ }}
    }}
}}
";
            foreach ((string op, string name) @operator in new[] { ("&&", "And"), ("||", "Or") })
            {
                yield return new GeneratedSource(
                    GetTestName($"UnaryOperatorExpression_Type_{@operator.name}_TrueFalse"),
                    LanguageNames.CSharp,
                    string.Format(code, @operator.op),
                    m => m.SyntaxTree.GetRoot().DescendantNodes().Where(n => m.GetOperationInternal(n) is IIfStatement).FirstOrDefault());
            }
        }

        private IEnumerable<GeneratedSource> GetCSharpUnaryOperatorExpressionTests(
            (string expr, string name)[] expressions,
            (string op, string name)[] operators,
            string[] types,
            string extra = null,
            Func<SemanticModel, SyntaxNode> nodeGetter = null)
        {
            var code = @"class A
{{
    {0} Method()
    {{
        {0} i = default({0});
        return {1}{2};
    }}
}}
" + extra;

            return GetUnaryOperatorExpressionTests(code, LanguageNames.CSharp, expressions, operators, types, nodeGetter);
        }

        private IEnumerable<GeneratedSource> GetVBUnaryPlusMinusNotExpression()
        {
            return GetVBUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("Not ", "Not") },
                dotNetPrimitiveTypes);
        }

        private IEnumerable<GeneratedSource> GetVBEnum()
        {
            return GetVBUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("Not ", "Not") },
                new[] { "E" },
                @"Enum E
A
B
End Enum",
                m => m.SyntaxTree.GetRoot().DescendantNodes().Where(n =>
                {
                    var operation = m.GetOperationInternal(n);
                    switch (operation)
                    {
                        case IUnaryOperatorExpression _:
                            return true;
                        case IConversionExpression conversion:
                            return conversion.Operand is IUnaryOperatorExpression;
                        default:
                            return false;
                    }
                }).FirstOrDefault());
        }

        private IEnumerable<GeneratedSource> GetVBCustomOperator()
        {
            var code = @"
Public Class CustomType
    Public Shared Operator -(x As CustomType) As CustomType
        Return x
    End Operator

    Public Shared operator +(x As CustomType) As CustomType
        return x
    End Operator

    Public Shared operator Not(x As CustomType) As CustomType
        return x
    End Operator
End CLass
";

            return GetVBUnaryOperatorExpressionTests(
                new[] { ("i", "Type"), ("Method()", "Method") },
                new[] { ("+", "Plus"), ("-", "Minus"), ("Not ", "Not") },
                new[] { "CustomType" },
                code);
        }

        private IEnumerable<GeneratedSource> GetVBTrueFalseOperator()
        {
            var code = @"
Public Class CustomType
    Public Shared Operator IsTrue(x As CustomType) As Boolean
        Return True
    End Operator

    Public Shared Operator IsFalse(x As CustomType) As Boolean
        Return False
    End Operator

    Public Shared Operator And(x As CustomType, y As CustomType) As CustomType
        Return x
    End Operator

    Public Shared Operator Or(x As CustomType, y As CustomType) As CustomType
        Return x
    End Operator
End Class

Class A
    Sub Method()
        Dim x As CustomType = New CustomType()
        Dim y As CustomType = New CustomType()
        If x {0} y Then
        End If
    End Sub
End Class
";
            foreach ((string op, string name) @operator in new[] { ("AndAlso", "And"), ("OrElse", "Or") })
            {
                yield return new GeneratedSource(
                    GetTestName($"UnaryOperatorExpression_Type_{@operator.name}_TrueFalse"),
                    LanguageNames.VisualBasic,
                    string.Format(code, @operator.op),
                    m => m.SyntaxTree.GetRoot().DescendantNodes().Where(n => m.GetOperationInternal(n) is IIfStatement).FirstOrDefault());
            }
        }

        private IEnumerable<GeneratedSource> GetVBUnaryOperatorExpressionTests(
            (string expr, string name)[] expressions,
            (string op, string name)[] operators,
            string[] types,
            string extra = null,
            Func<SemanticModel, SyntaxNode> nodeGetter = null)
        {
            var code = @"Class A
    Function Method() As {0}
        Dim i As {0} = Nothing
        Return {1}{2}
    End Function
End Class
" + extra;

            return GetUnaryOperatorExpressionTests(code, LanguageNames.VisualBasic, expressions, operators, types, nodeGetter);
        }

        private IEnumerable<GeneratedSource> GetUnaryOperatorExpressionTests(
            string code, string language,
            (string expr, string name)[] expressions,
            (string op, string name)[] operators,
            string[] types,
            Func<SemanticModel, SyntaxNode> nodeGetter)
        {
            nodeGetter = nodeGetter ?? s_nodeGetter;

            foreach (var expression in expressions)
            {
                foreach (var @operator in operators)
                {
                    foreach (var type in types)
                    {
                        yield return new GeneratedSource(
                            GetTestName($"UnaryOperatorExpression_{expression.name}_{@operator.name}_{type}"),
                            language, string.Format(code, type, @operator.op, expression.expr), nodeGetter);
                    }
                }
            }
        }

        private string GetTestName(string rawString)
        {
            return rawString.Replace(" ", "_").Replace(".", "_");
        }
    }

    internal class GeneratedSource
    {
        public GeneratedSource(string name, string language, string source, Func<SemanticModel, SyntaxNode> nodeGetter)
        {
            Name = name;
            Language = language;
            Source = source;
            NodeGetter = nodeGetter;
        }

        public string Name { get; }
        public string Language { get; }
        public string Source { get; }
        public Func<SemanticModel, SyntaxNode> NodeGetter { get; }
    }
}