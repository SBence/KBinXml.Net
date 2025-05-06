Push-Location ./ReadBenchmark
try {
    dotnet run --configuration Release --framework "net48" --filter "*SingleThreadComparisonBetweenLibsTask*"
}
finally {
    Pop-Location
}