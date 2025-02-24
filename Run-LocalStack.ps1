$localstackContainerId = docker ps -q -f name=localstack
if (-not $localstackContainerId) {
    Write-Host 'Start LocalStack'
    if ($env:LOCALSTACK_AUTH_TOKEN) {
        docker run -d --name localstack -p 4566:4566 -e PERSISTENCE=1 -e LAMBDA_KEEPALIVE_MS=120000 -e LOCALSTACK_AUTH_TOKEN=$env:LOCALSTACK_AUTH_TOKEN -v //var/run/docker.sock:/var/run/docker.sock localstack/localstack-pro:4.1
    }
    else {
        docker run -d --name localstack -p 4566:4566 -e PERSISTENCE=1 -e LAMBDA_KEEPALIVE_MS=120000 -v //var/run/docker.sock:/var/run/docker.sock localstack/localstack:4.1
    }    

    Write-Host 'Waiting for LocalStack services to be ready...'
    $services = @("lambda", "s3", "sns", "sqs")
    $ready = $false
    $timeout = 300
    $startTime = Get-Date

    while (-not $ready -and ((Get-Date) - $startTime).TotalSeconds -lt $timeout) {
        try {
            $healthCheck = Invoke-WebRequest -Uri "http://localhost:4566/_localstack/health" -UseBasicParsing
            if ($healthCheck.StatusCode -eq 200) {
                $health = ConvertFrom-Json $healthCheck.Content
                $ready = $true
                foreach ($service in $services) {
                    if ($health.services.$service -ne "available" -and $health.services.$service -ne "running") {
                        Write-Host "Service '$service' not yet available. Status: $($health.services.$service)"
                        $ready = $false
                        break
                    }
                }
                if ($ready) {
                    Write-Host 'All required services are available.'
                } else {
                    Write-Host 'Not all required services are available yet. Waiting...'
                    Start-Sleep -s 1
                }
            } else {
                Write-Host "Health check failed with status code: $($healthCheck.StatusCode). Waiting..."
                Start-Sleep -s 5
            }
        } catch {
            Write-Host "Error during health check: $($_.Exception.Message). Waiting..."
            Start-Sleep -s 5
        }
    }

    if (-not $ready) {
        Write-Error "Timeout waiting for LocalStack services to become available."
        exit 1
    }

    Write-Host 'LocalStack services are ready.'
}
