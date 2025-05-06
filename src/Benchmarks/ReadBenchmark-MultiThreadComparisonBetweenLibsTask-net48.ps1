Push-Location ./ReadBenchmark
try {
    dotnet run --configuration Release --framework "net48" --filter "*MultiThreadComparisonBetweenLibsTask*"
}
finally {
    Pop-Location
}