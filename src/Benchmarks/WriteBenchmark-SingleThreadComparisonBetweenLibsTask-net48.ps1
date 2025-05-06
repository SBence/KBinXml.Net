Push-Location ./WriteBenchmark
try {
    dotnet build --configuration Release --framework "net48" /p:DefineConstants=kbin1_1
    dotnet run --configuration Release --framework "net48" --filter "*SingleThreadComparisonBetweenLibsTask*" --no-build
}
finally {
    Pop-Location
}