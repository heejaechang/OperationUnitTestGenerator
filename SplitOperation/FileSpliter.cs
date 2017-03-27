using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace SplitOperation
{
    class FileSpliter
    {
        public async Task SplitAsync(Project project)
        {
            var nodes = new HashSet<SyntaxNode>();

            foreach (var document in project.Documents.Where(d => d.FilePath?.IndexOf("IStatement.cs") > 0 || d.FilePath?.IndexOf("IExpression.cs") > 0))
            {
                var root = await document.GetSyntaxRootAsync();

                foreach (var node in root.DescendantNodes().Where(n => GetName(n) != null))
                {
                    nodes.Add(node);
                }
            }

            SaveNodes(project, nodes);
        }

        private void SaveNodes(Project project, HashSet<SyntaxNode> nodes)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var path = @"C:\dd\roslyn2\src\Compilers\Core\Portable\Operations";

            var head = @"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
 
using System.Collections.Immutable;
 
namespace Microsoft.CodeAnalysis.Semantics
{";

            var foot = @"}";

            foreach (var node in nodes)
            {
                var name = GetName(node);
                if (!set.Add(name))
                {
                    throw new Exception("duplicates!!");
                }

                var sb = new StringBuilder();
                sb.AppendLine(head);
                sb.Append(node.ToFullString().Trim());
                sb.AppendLine(foot);

                var tree = CSharpSyntaxTree.ParseText(sb.ToString(), (CSharpParseOptions)project.ParseOptions);
                var root = Formatter.Format(tree.GetRoot(), project.Solution.Workspace);

                using (var writer = File.CreateText(Path.Combine(path, name + ".cs")))
                {
                    var text = root.ToFullString();
                    writer.WriteLine(text);
                }
            }
        }

        private string GetName(SyntaxNode node)
        {
            switch (node)
            {
                case BaseTypeDeclarationSyntax type:
                    return type.Identifier.ToString().Trim();
                case DelegateDeclarationSyntax @delegate:
                    return @delegate.Identifier.ToString().Trim();
                default:
                    return null;
            }
        }
    }
}
