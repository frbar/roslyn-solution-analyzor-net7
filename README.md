>
> Forked from https://github.com/HamedFathi/RoslynSolutionAnalyzor 
>
> Adding .NET 7 support
>


![roslyn](https://user-images.githubusercontent.com/8418700/141319953-df7377d9-bc72-409e-8fd1-5e7000339d47.png)

Based on this sample you can read & analyze other solutions by **Roslyn**.

# Get Started

```
dotnet build src
dotnet run --project .\src\LoadSolutionForAnalysis\ "path\to\your\solution.sln"
```

# Docker support

```
docker build -t load-solution-for-analysis .
docker run load-solution-for-analysis
```