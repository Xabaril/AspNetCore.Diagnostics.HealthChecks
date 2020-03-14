Param(
    [parameter(Mandatory=$false)][bool]$PublishToDockerHub=$false
)


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

#Select the UI version from dependencies.props and use it as image version


$version = select-xml -Path ${PSScriptRoot}/build/dependencies.props -XPath "/Project/PropertyGroup[contains(@Label,'Health Checks Package Versions')]/HealthCheckUI"

$tag = $version.node.InnerXML

#Building docker image

echo "Building docker image with tag: $tag"
echo "Publish to Docker Hub : $PublishToDockerHub"

exec { & docker build . -f ${PSScriptRoot}/build/docker-images/HealthChecks.UI.Image/Dockerfile -t xabarilcoding/healthchecksui:$tag }
exec { & docker tag xabarilcoding/healthchecksui:$tag xabarilcoding/healthchecksui:latest }

echo "Created docker image healthchecksui:$tag. You can execute this image using docker run"
echo "Sample: docker run --name ui -p 5000:80 -e 'HealthChecksUI:HealthChecks:0:Name=httpBasic' -e 'HealthChecksUI:HealthChecks:0:Uri=http://www.google.es' -d healthchecksui:dev"

#Publish it

if($PublishToDockerHub){
    docker push xabarilcoding/healthchecksui:$tag 
    docker push xabarilcoding/healthchecksui:latest 
}