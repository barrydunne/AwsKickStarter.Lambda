# dotnet tool install --global dotnet-reportgenerator-globaltool

Set-Location $PSScriptRoot
dotnet test .\AwsKickStarter.Lambda.Tests\AwsKickStarter.Lambda.Tests.csproj -c Release --framework net8.0 -l "console;verbosity=normal" --results-directory:"$PSScriptRoot/TestResults" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$PSScriptRoot/TestResults/coverage" /p:Include=[AwsKickStarter.Lambda]* /p:Threshold=100
reportgenerator -reports:TestResults/coverage.opencover.xml -targetdir:TestResults/CoverageReport -reporttypes:Html_Dark
./TestResults/CoverageReport/index.html