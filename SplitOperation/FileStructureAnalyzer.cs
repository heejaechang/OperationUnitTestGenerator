using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SplitOperation
{
    class FileStructureAnalyzer
    {
        public async Task PrintAnalysisAsync(Project project)
        {
            var compilation = await project.GetCompilationAsync();

            var allInterfaces = GetAllRelatedInterfaces(compilation);
            allInterfaces.PrintTypeInfo();

            var allTypes = GetAllRelatedTypes(compilation, allInterfaces);
            allTypes.PrintTypeInfo();
        }

        private HashSet<ITypeSymbol> GetAllRelatedTypes(Compilation compilation, HashSet<ITypeSymbol> allInterfaces)
        {
            var alltypes = new HashSet<ITypeSymbol>();

            GetAllRelatedTypes(compilation, allInterfaces, alltypes);
            return alltypes;
        }

        private void GetAllRelatedTypes(Compilation compilation, HashSet<ITypeSymbol> allInterfaces, HashSet<ITypeSymbol> alltypes)
        {
            foreach (var itface in allInterfaces)
            {
                foreach (var member in itface.GetMembers())
                {
                    switch (member)
                    {
                        case IPropertySymbol property:
                            AddIfNeeded(alltypes, property.Type);
                            break;
                        case IMethodSymbol method:
                            AddIfNeeded(alltypes, method.ReturnType);
                            AddIfNeeded(alltypes, method.Parameters.Select(p => p.Type).ToArray());
                            break;
                        default:
                            throw new Exception("can't happen");
                    }
                }
            }
        }

        private void AddIfNeeded(HashSet<ITypeSymbol> alltypes, params ISymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                alltypes.Add((ITypeSymbol)symbol);
            }
        }

        private HashSet<ITypeSymbol> GetAllRelatedInterfaces(Compilation compilation)
        {
            var allInterfaces = new HashSet<ITypeSymbol>();

            GetAllRelatedInterfaces(compilation, allInterfaces);
            return allInterfaces;
        }

        private void GetAllRelatedInterfaces(Compilation compilation, HashSet<ITypeSymbol> allInterfaces)
        {
            var ioperation = compilation.GetTypeByMetadataName("Microsoft.CodeAnalysis.IOperation");

            foreach (var type in compilation.GlobalNamespace.GetMembers().GetAllTypes())
            {
                if (type.TypeKind == TypeKind.Interface && type.AllInterfaces.Any(i => i.Equals(ioperation)))
                {
                    allInterfaces.Add(type);
                }
            }
        }
    }
}
