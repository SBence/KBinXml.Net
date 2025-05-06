Push-Location ./ReadBenchmark
dotnet run --configuration Release --framework "net8.0" --filter "*MultiThreadComparison1*"
Pop-Location