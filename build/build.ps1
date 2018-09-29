# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

exec { & dotnet restore }

$tag = $(git tag -l --points-at HEAD)
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "ci-$revision"}[$tag -ne $NULL -and $revision -ne "local"]
$commitHash = $(git rev-parse --short HEAD)
$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]

echo "build: Tag is $tag"
echo "build: Package version suffix is $suffix"
echo "build: Build version suffix is $buildSuffix" 

exec { & dotnet build BeatPulse.sln -c Release --version-suffix=$buildSuffix -v q /nologo }

if (-Not (Test-Path 'env:APPVEYOR')) {
	exec { & docker-compose up -d }
}

echo "compose up done"

echo "running tests"

try {

	Push-Location -Path .\tests\UnitTests
	exec { & dotnet test }
} finally {
	Pop-Location
}

try {
    
        Push-Location -Path .\tests\FunctionalTests
        exec { & dotnet test }
    } finally {
        Pop-Location
    }


if ($suffix -eq "") {
    exec { & dotnet pack .\src\BeatPulse\BeatPulse.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.SqlServer\BeatPulse.SqlServer.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.Redis\BeatPulse.Redis.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.MongoDb\BeatPulse.MongoDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.NpgSql\BeatPulse.NpgSql.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.AzureStorage\BeatPulse.AzureStorage.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.MySql\BeatPulse.MySql.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.DocumentDb\BeatPulse.DocumentDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.Sqlite\BeatPulse.Sqlite.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.UI\BeatPulse.UI.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.Kafka\BeatPulse.Kafka.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.RabbitMQ\BeatPulse.RabbitMQ.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.IdSvr\BeatPulse.IdSvr.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.AzureServiceBus\BeatPulse.AzureServiceBus.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.ApplicationInsightsTracker\BeatPulse.ApplicationInsightsTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.PrometheusTracker\BeatPulse.PrometheusTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.DynamoDb\BeatPulse.DynamoDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
	exec { & dotnet pack .\src\BeatPulse.StatusPageTracker\BeatPulse.StatusPageTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.Uris\BeatPulse.Uris.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.Oracle\BeatPulse.Oracle.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.System\BeatPulse.System.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\BeatPulse.Network\BeatPulse.Network.csproj -c Release -o ..\..\artifacts --include-symbols --no-build }
} else {
    exec { & dotnet pack .\src\BeatPulse\BeatPulse.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.SqlServer\BeatPulse.SqlServer.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.Redis\BeatPulse.Redis.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.MongoDb\BeatPulse.MongoDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.NpgSql\BeatPulse.NpgSql.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.AzureStorage\BeatPulse.AzureStorage.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.MySql\BeatPulse.MySql.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.DocumentDb\BeatPulse.DocumentDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.Sqlite\BeatPulse.Sqlite.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.UI\BeatPulse.UI.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.Kafka\BeatPulse.Kafka.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.AzureServiceBus\BeatPulse.AzureServiceBus.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.RabbitMQ\BeatPulse.RabbitMQ.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.IdSvr\BeatPulse.IdSvr.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.ApplicationInsightsTracker\BeatPulse.ApplicationInsightsTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.PrometheusTracker\BeatPulse.PrometheusTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.DynamoDb\BeatPulse.DynamoDb.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
	exec { & dotnet pack .\src\BeatPulse.StatusPageTracker\BeatPulse.StatusPageTracker.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.Uris\BeatPulse.Uris.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.Oracle\BeatPulse.Oracle.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.System\BeatPulse.System.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\BeatPulse.Network\BeatPulse.Network.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --version-suffix=$suffix }
}

