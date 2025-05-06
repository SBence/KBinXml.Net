Push-Location ./ReadBenchmark
try {
    dotnet run --configuration Release --framework "net8.0" --filter "*MultiThreadComparisonBetweenLibsTask*"
}
finally {
    Pop-Location
}
