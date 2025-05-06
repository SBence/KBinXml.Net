Push-Location ./ReadBenchmark
try {
    dotnet build --configuration Release --framework "net8.0" /p:DefineConstants=kbin1_1
    dotnet run --configuration Release --framework "net8.0" --filter "*MultiThreadComparisonBetweenLibsTask*" --no-build
}
finally {
    Pop-Location
}
