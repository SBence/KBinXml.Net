Push-Location ./WriteBenchmark
dotnet run --configuration Release --framework "net48" --filter "*SingleThreadComparisonBetweenLibsTask*"
Pop-Location