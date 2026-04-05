param(
    [ValidateSet('arm64', 'x86_64')]
    [string]
    $Architecture = 'x86_64',
    [switch]
    $Rebuild
)

$demoCommands = @(
    @{
        LambdaNamespace = 'Simple.Lambda'
        Description = 'Invoked the lambda without waiting for completion.'
    },
    @{
        LambdaNamespace = 'Simple.LambdaIn'
        Description = 'Invoked the lambda passing a payload without waiting for completion.'
    },
    @{
        LambdaNamespace = 'Simple.LambdaOut'
        Description = 'Invoked the lambda waiting for completion and displayed the output.'
    },
    @{
        LambdaNamespace = 'Simple.LambdaInOut'
        Description = 'Invoked the lambda passing a payload, waited for completion, and displayed the output.'
    },
    @{
        LambdaNamespace = 'S3'
        Description = 'Received an event that an S3 file was created.'
    },
    @{
        LambdaNamespace = 'Sns.Lambda'
        Description = 'Received five notifications from the topic, retried only failed messages, and discarded a single failed message after three retries.'
    },
    @{
        LambdaNamespace = 'Sns.LambdaT'
        Description = 'Showed the same behavior as Sns.Lambda using the typed message variant.'
    },
    @{
        LambdaNamespace = 'Sqs.Basic'
        Description = 'Received a batch of five messages and retried the full batch on failure; after three retries all five messages moved to the dead letter queue.'
    },
    @{
        LambdaNamespace = 'Sqs.BasicT'
        Description = 'Showed the same behavior as Sqs.Basic using the typed message variant.'
    },
    @{
        LambdaNamespace = 'Sqs.BatchResponse'
        Description = 'Received a batch of five messages and retried only failed messages; after three retries the single failed message moved to the dead letter queue.'
    },
    @{
        LambdaNamespace = 'Sqs.BatchResponseT'
        Description = 'Showed the same behavior as Sqs.BatchResponse using the typed message variant.'
    }
)

function Show-DemoBanner {
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $Description
    )

    $line = '*' * 54
    Write-Host $line
    Write-Host 'Demonstration:'
    Write-Host $Description
    Write-Host $line
}

function Read-ContinueChoice {
    while ($true) {
        Write-Host "Press 'y' to continue to the next demo or 'n' to quit." -NoNewline
        $key = [Console]::ReadKey($true)
        Write-Host " $($key.KeyChar)"

        switch ($key.KeyChar.ToString().ToLowerInvariant()) {
            'y' { return $true }
            'n' { return $false }
            default { Write-Host "Please press 'y' or 'n'." }
        }
    }
}

$scriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'Run-LambdaDemo.ps1'

for ($i = 0; $i -lt $demoCommands.Count; $i++) {
    $demo = $demoCommands[$i]
    $index = $i + 1
    $namespace = $demo.LambdaNamespace
    $description = $demo.Description

    Write-Host ""
    Write-Host "[$index/$($demoCommands.Count)] Running demo '$namespace' with architecture '$Architecture'..."

    if ($Rebuild) {
        & $scriptPath -LambdaNamespace $namespace -Architecture $Architecture -Rebuild
    }
    else {
        & $scriptPath -LambdaNamespace $namespace -Architecture $Architecture
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Demo '$namespace' exited with code $LASTEXITCODE."
    }

    if ($index -lt $demoCommands.Count) {
        Show-DemoBanner -Description $description
        if (-not (Read-ContinueChoice)) {
            Write-Host 'Stopping demo run by user choice.'
            break
        }
    }
}

Write-Host 'Run-AllLambdaDemos completed.'