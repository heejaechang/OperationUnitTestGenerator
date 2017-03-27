using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SplitOperation
{
    static class Extensions
    {
        public static void PrintTypeInfo(this HashSet<ITypeSymbol> types)
        {
            var locationset = new HashSet<string>();

            var count = 0;
            foreach (var type in types)
            {
                var locations = type.Locations.Select(l => l.SourceTree?.FilePath?.ToLower()).Where(f => f != null);
                locationset.UnionWith(locations);

                Console.WriteLine($"[{count++}] {type.ToDisplayString()} - {string.Join("|", locations)}");
            }

            count = 0;
            foreach (var location in locationset)
            {
                Console.WriteLine("file locations");
                Console.WriteLine($"[{count++}] {location}");
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetAllTypes(this IEnumerable<ISymbol> members)
        {
            foreach (var member in members)
            {
                if (member is INamedTypeSymbol typeMember)
                {
                    yield return typeMember;

                    foreach (var type in GetAllTypes(typeMember.GetTypeMembers()))
                    {
                        yield return type;
                    }

                    continue;
                }

                if (member is INamespaceSymbol namespaceMember)
                {
                    foreach (var type in GetAllTypes(namespaceMember.GetMembers()))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}