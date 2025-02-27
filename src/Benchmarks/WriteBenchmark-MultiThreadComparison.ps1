dotnet build WriteBenchmark --configuration Release --framework net8.0
if ($LASTEXITCODE -eq 0) {
  Start-Process dotnet -WorkingDirectory ./WriteBenchmark/bin/Release/net8.0 -NoNewWindow -Wait -ArgumentList "WriteBenchmark.dll --filter *MultiThreadComparison1*"
}