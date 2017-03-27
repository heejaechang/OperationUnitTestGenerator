using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace SplitOperation
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var project = await GetProjects();

            await SplitFilesAsync(project);
        }

        private static async Task<Project> GetProjects()
        {
            var workspace = MSBuildWorkspace.Create();

#if true
            var project = await workspace.OpenProjectAsync(@"C:\dd\roslyn2\src\Compilers\Core\Portable\CodeAnalysis.csproj");
#else
            var solution = await workspace.OpenSolutionAsync(@"C:\dd\roslyn2\Compilers.sln");
            var project = solution.Projects.Where(p => p.Name == "CodeAnalysis").FirstOrDefault();
#endif

            return project;
        }

        private static Task SplitFilesAsync(Project project)
        {
            var spliter = new FileSpliter();
            return spliter.SplitAsync(project);
        }

        private static Task PrintAnalysis(Project project)
        {
            var analyzer = new FileStructureAnalyzer();
            return analyzer.PrintAnalysisAsync(project);
        }
    }
}
