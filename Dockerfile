# build and copy dotnet project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY src/LoadSolutionForAnalysis.csproj .
RUN dotnet restore LoadSolutionForAnalysis.csproj
COPY src/ .
#RUN dotnet build LoadSolutionForAnalysis.csproj -c Release -o /app/build

RUN dotnet publish LoadSolutionForAnalysis.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY test/ /app/test/

ENTRYPOINT ["dotnet", "LoadSolutionForAnalysis.dll", "test/test.sln", "test", "A", "PropertyA"]
