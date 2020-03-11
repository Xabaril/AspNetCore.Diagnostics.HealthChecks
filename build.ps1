# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if (Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

exec { & dotnet restore }

$tag = $(git tag -l --points-at HEAD)
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "ci-$revision" }[$tag -ne $NULL -and $revision -ne "local"]
$commitHash = $(git rev-parse --short HEAD)
$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]

echo "build: Tag is $tag"
echo "build: Package version suffix is $suffix"
echo "build: Build version suffix is $buildSuffix"

exec { & dotnet build AspNetCore.Diagnostics.HealthChecks.sln -c Release --version-suffix=$buildSuffix -v q /nologo }

echo "Running unit tests"

try {

    Push-Location -Path .\test\UnitTests
    exec { & dotnet test }
}
finally {
    Pop-Location
}


if (-Not (Test-Path 'env:APPVEYOR')) {
    exec { & docker-compose up -d }
}

echo "compose up done"

echo "Running functional tests"

try {

    Push-Location -Path .\test\FunctionalTests
    exec { & dotnet test }
}
finally {
    Pop-Location
}


if ($suffix -eq "") {
    exec { & dotnet pack .\src\HealthChecks.SqlServer\HealthChecks.SqlServer.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Redis\HealthChecks.Redis.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.MongoDb\HealthChecks.MongoDb.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.NpgSql\HealthChecks.NpgSql.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.AzureStorage\HealthChecks.AzureStorage.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.MySql\HealthChecks.MySql.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.DocumentDb\HealthChecks.DocumentDb.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.CosmosDb\HealthChecks.CosmosDb.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Hangfire\HealthChecks.Hangfire.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Elasticsearch\HealthChecks.Elasticsearch.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Sqlite\HealthChecks.Sqlite.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Solr\HealthChecks.Solr.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Kafka\HealthChecks.Kafka.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.RabbitMQ\HealthChecks.RabbitMQ.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.EventStore\HealthChecks.EventStore.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.IdSvr\HealthChecks.IdSvr.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.AzureServiceBus\HealthChecks.AzureServiceBus.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.DynamoDb\HealthChecks.DynamoDb.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Uris\HealthChecks.Uris.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Oracle\HealthChecks.Oracle.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.System\HealthChecks.System.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Network\HealthChecks.Network.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Aws.S3\HealthChecks.Aws.S3.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Azure.IoTHub\HealthChecks.Azure.IoTHub.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.AzureKeyVault\HealthChecks.AzureKeyVault.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.UI\HealthChecks.UI.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.UI.Client\HealthChecks.UI.Client.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Publisher.ApplicationInsights\HealthChecks.Publisher.ApplicationInsights.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Datadog\HealthChecks.Publisher.Datadog.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Prometheus\HealthChecks.Publisher.Prometheus.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Seq\HealthChecks.Publisher.Seq.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Consul\HealthChecks.Consul.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.RavenDB\HealthChecks.RavenDB.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Kubernetes\HealthChecks.Kubernetes.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.SignalR\HealthChecks.SignalR.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.Gcp.CloudFirestore\HealthChecks.Gcp.CloudFirestore.csproj -c Release -o .\artifacts --include-symbols --no-build }
    exec { & dotnet pack .\src\HealthChecks.IbmMQ\HealthChecks.IbmMQ.csproj -c Release -o .\artifacts --include-symbols --no-build }	
}

else {
    exec { & dotnet pack .\src\HealthChecks.SqlServer\HealthChecks.SqlServer.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Redis\HealthChecks.Redis.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.MongoDb\HealthChecks.MongoDb.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.NpgSql\HealthChecks.NpgSql.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.AzureStorage\HealthChecks.AzureStorage.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.MySql\HealthChecks.MySql.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.DocumentDb\HealthChecks.DocumentDb.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.CosmosDb\HealthChecks.CosmosDb.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Hangfire\HealthChecks.Hangfire.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Elasticsearch\HealthChecks.Elasticsearch.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Solr\HealthChecks.Solr.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }    
    exec { & dotnet pack .\src\HealthChecks.Sqlite\HealthChecks.Sqlite.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Kafka\HealthChecks.Kafka.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.AzureServiceBus\HealthChecks.AzureServiceBus.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.RabbitMQ\HealthChecks.RabbitMQ.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.EventStore\HealthChecks.EventStore.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.IdSvr\HealthChecks.IdSvr.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.DynamoDb\HealthChecks.DynamoDb.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Uris\HealthChecks.Uris.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Oracle\HealthChecks.Oracle.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.System\HealthChecks.System.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Network\HealthChecks.Network.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Aws.S3\HealthChecks.Aws.S3.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Azure.IoTHub\HealthChecks.Azure.IoTHub.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.AzureKeyVault\HealthChecks.AzureKeyVault.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.UI\HealthChecks.UI.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.UI.Client\HealthChecks.UI.Client.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Publisher.ApplicationInsights\HealthChecks.Publisher.ApplicationInsights.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Datadog\HealthChecks.Publisher.Datadog.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Prometheus\HealthChecks.Publisher.Prometheus.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Publisher.Seq\HealthChecks.Publisher.Seq.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Consul\HealthChecks.Consul.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.RavenDB\HealthChecks.RavenDB.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Kubernetes\HealthChecks.Kubernetes.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.SignalR\HealthChecks.SignalR.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.Gcp.CloudFirestore\HealthChecks.Gcp.CloudFirestore.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }
    exec { & dotnet pack .\src\HealthChecks.IbmMQ\HealthChecks.IbmMQ.csproj -c Release -o .\artifacts --include-symbols --no-build --version-suffix=$suffix }	
}
