using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Extensions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace WindowsFormsApp2
{
    internal class UnitTestBuilder
    {
        internal string TryCreate(string name, string languageName, string source, Func<SemanticModel, SyntaxNode> nodeGetter)
        {
            var tree = CreateTree(languageName, source);
            var compilation = CreateCompilation(languageName, tree);
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            var node = nodeGetter(model);
            if (node == null)
            {
                // no node we are looking for
                return null;
            }

            var operation = model.GetOperationInternal(node);
            var expectedOperationTree = operation != null ? OperationTreeVerifier.GetOperationTree(operation) : null;

            return languageName == LanguageNames.CSharp ?
                CreateCSharpUnitTest(name, model, node, expectedOperationTree) :
                CreateVBUnitTest(name, model, node, expectedOperationTree);
        }

        private static SyntaxTree CreateTree(string languageName, string source)
        {
            return languageName == LanguageNames.CSharp ?
                Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source) :
                Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source);
        }

        private static Compilation CreateCompilation(string languageName, SyntaxTree tree)
        {
            var syntaxTrees = new SyntaxTree[] { tree };
            var metadataReferences = new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };

            return languageName == LanguageNames.CSharp ?
                (Compilation)Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("test.dll", syntaxTrees, metadataReferences) :
                Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.Create("test.dll", syntaxTrees, metadataReferences);
        }

        private string CreateCSharpUnitTest(
            string name,
            SemanticModel model,
            SyntaxNode syntaxNode,
            string expectedOperationTree)
        {
            string sourceCode = model.SyntaxTree.GetText().ToString();
            TextSpan nodeSpan = syntaxNode.Span;
            sourceCode = sourceCode.Insert(nodeSpan.End, "/*</bind>*/");
            sourceCode = sourceCode.Insert(nodeSpan.Start, "/*<bind>*/");
            sourceCode = sourceCode.Replace("\"", "\"\"");  // double all quote marks.

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            string outerIndent = "        ";
            string indent = outerIndent + "    ";

            writer.Write(outerIndent);
            writer.WriteLine("[Fact]");
            writer.Write(outerIndent);
            writer.WriteLine("public void Test_" + name + "()");
            writer.Write(outerIndent);
            writer.WriteLine("{");

            writer.Write(indent);
            writer.WriteLine("string source = @\"");
            writer.WriteLine(sourceCode);
            writer.WriteLine("\";");

            writer.Write(indent);
            writer.WriteLine("string expectedOperationTree = @\"");
            writer.Write(expectedOperationTree);
            writer.WriteLine("\";");

            writer.Write(indent);
            writer.WriteLine("VerifyOperationTreeForTest<{0}>(source, expectedOperationTree);", syntaxNode.GetType().Name);

            writer.Write(outerIndent);
            writer.WriteLine("}");
            writer.WriteLine();

            return writer.ToString();
        }

        private string CreateVBUnitTest(
            string name,
            SemanticModel model,
            SyntaxNode syntaxNode,
            string expectedOperationTree)
        {
            string sourceCode = model.SyntaxTree.GetText().ToString();
            TextSpan nodeSpan = syntaxNode.Span;

            string bindText = syntaxNode.ToString();
            int indexOfEolInBindText = bindText.IndexOf("\r");
            if (indexOfEolInBindText > 0)
            {
                bindText = bindText.Substring(0, indexOfEolInBindText);
            }
            int indexOfEoln = sourceCode.IndexOf("\r", nodeSpan.Start);
            sourceCode = sourceCode.Insert(indexOfEoln, "'BIND:\"" + bindText + "\"");

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            string outerIndent = "        ";
            string indent = outerIndent + "    ";

            writer.Write(outerIndent);
            writer.WriteLine("<Fact()>");
            writer.Write(outerIndent);
            writer.WriteLine("Public Sub Test_" + name + "()");

            writer.Write(indent);
            writer.WriteLine("Dim source = <![CDATA[");
            writer.WriteLine(sourceCode);
            writer.WriteLine("    ]]>.Value");
            writer.WriteLine();

            writer.Write(indent);
            writer.WriteLine("Dim expectedOperationTree = <![CDATA[");
            writer.Write(expectedOperationTree);
            writer.WriteLine("]]>.Value");
            writer.WriteLine();

            writer.Write(indent);
            writer.WriteLine("VerifyOperationTreeForTest(Of {0})(source, expectedOperationTree)", syntaxNode.GetType().Name);

            writer.Write(outerIndent);
            writer.WriteLine("End Sub");
            writer.WriteLine();

            return writer.ToString();
        }
    }
}