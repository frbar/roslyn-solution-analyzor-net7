using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

using static System.Console;

namespace LoadSolutionForAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            VisualStudioInstance[] visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            VisualStudioInstance instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            // Print message for WorkspaceFailed event to help diagnosing project load failures.
            workspace.WorkspaceFailed += (o, e) => WriteLine(e.Diagnostic.Message);

            string solutionPath = args[0];
            WriteLine($"Loading solution '{solutionPath}'");

            // Attach progress reporter so we print projects as they are loaded.
            Solution solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            WriteLine($"Finished loading solution '{solutionPath}'");

            // TODO: Do analysis on the projects in the loaded solution

            var projectName = args[1];
            var className = args[2];
            var propertyName = args[3];

            var project = solution.Projects.SingleOrDefault(p => p.Name == projectName);

            if (project == null)
            {
                WriteLine($"Project {projectName} not found");
                return;
            }

            var compilation = await project.GetCompilationAsync();
            var propertiesForName = compilation.GetSymbolsWithName(s => s.StartsWith(propertyName), SymbolFilter.All);
            var myProperty = propertiesForName.FirstOrDefault(x => x.Kind == SymbolKind.Property && x.ContainingType.Name == className);
            if (myProperty == null)
            {
                WriteLine("Property not found");
                return;
            }

            var callers = await SymbolFinder.FindReferencesAsync(myProperty, solution);

            foreach(var caller in callers)
            {
                if (caller.Locations.Any())
                {
                    foreach(var refLocation in caller.Locations)
                    {
                        WriteLine(myProperty.Name + " is referenced at line " + refLocation.Location.GetLineSpan().StartLinePosition.Line);
                        WriteLine("  in file " + refLocation.Document.FilePath);

                        // get AdditionalProperties by reflection on refLocation
                        var additionalProperties = refLocation.GetType()
                                                              .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                                                              .Where(p => p.Name == "AdditionalProperties")
                                                              .FirstOrDefault();
                        if (additionalProperties != null)
                        {
                            var value = additionalProperties.GetValue(refLocation) as ImmutableDictionary<string, string>;
                            var callerType = value["ContainingTypeInfo"];

                            WriteLine("  from type " + callerType);
                        }
                    }
                }
            }
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            WriteLine("Multiple installs of MSBuild detected:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                WriteLine($"Instance {i + 1}");
                WriteLine($"    Name: {visualStudioInstances[i].Name}");
                WriteLine($"    Version: {visualStudioInstances[i].Version}");
                WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }
            WriteLine("Please select one: ");

            while (true)
            {
                var userResponse = ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}