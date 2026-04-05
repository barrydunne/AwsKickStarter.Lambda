# dotnet tool install --global dotnet-reportgenerator-globaltool

Set-Location $PSScriptRoot
if (Test-Path ./TestResults) {
	Remove-Item ./TestResults -Recurse -Force
}

dotnet test .\AwsKickStarter.Lambda.Tests\AwsKickStarter.Lambda.Tests.csproj -c Release --framework net10.0 -l "console;verbosity=normal" --results-directory:"$PSScriptRoot/TestResults" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$PSScriptRoot/TestResults/coverage.xml" /p:Include=[AwsKickStarter.Lambda]* /p:Threshold=100
reportgenerator -reports:TestResults/coverage.net10.0.xml -targetdir:TestResults/CoverageReport -reporttypes:Html_Dark
./TestResults/CoverageReport/index.html