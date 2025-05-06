Push-Location ./ReadBenchmark
dotnet run --configuration Release --framework "net48" --filter "*MultiThreadComparisonBetweenLibsTask*"
Pop-Location