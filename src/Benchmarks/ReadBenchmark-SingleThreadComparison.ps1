dotnet build ReadBenchmark --configuration Release --framework net8.0
if ($LASTEXITCODE -eq 0) {
  Start-Process dotnet -WorkingDirectory ./ReadBenchmark/bin/Release/net8.0 -NoNewWindow -Wait -ArgumentList "ReadBenchmark.dll --filter *SingleThreadComparison1*"
}