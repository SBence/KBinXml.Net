Push-Location ./ReadBenchmark
dotnet run --configuration Release --framework "net8.0" --filter "*SingleThreadComparisonBetweenLibsTask*"
Pop-Location