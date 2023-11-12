>
> Forked from https://github.com/HamedFathi/RoslynSolutionAnalyzor 
> 
> Adding:
> - .NET 7 support
> - Find reference code sample
> - Docker support
>


![roslyn](https://user-images.githubusercontent.com/8418700/141319953-df7377d9-bc72-409e-8fd1-5e7000339d47.png)

Based on this sample you can read & analyze other solutions by **Roslyn**.

# Getting Started

Find references to a property of a class of a project.

```powershell
dotnet build src
dotnet run --project .\src "path\to\your\solution.sln" <project name> <class name> <property name>
```

# Docker support

It will use the `test` project, looking for references to `A.PropertyA`.

```powershell
docker build -t load-solution-for-analysis .
docker run load-solution-for-analysis
```

Expected result:

```powershell
Using MSBuild at '/usr/share/dotnet/sdk/7.0.403' to load projects.
Loading solution 'test/test.sln'
Evaluate        0:01.4922128    test.csproj
Build           0:00.3177451    test.csproj
Resolve         0:00.0671563    test.csproj (net7.0)
Finished loading solution 'test/test.sln'
PropertyA is referenced at line 5
  in file /app/test/OtherCaller.cs
  from type CallerAB
PropertyA is referenced at line 4
  in file /app/test/CallerA.cs
  from type CallerA
```