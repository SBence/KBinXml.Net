Push-Location ./ReadBenchmark
try {
    dotnet run --configuration Release --framework "net8.0" --filter "*SingleThreadComparison*"
}
finally {
    Pop-Location
}