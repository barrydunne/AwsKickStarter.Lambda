param (
    [Parameter(Mandatory=$true)]
    [string]
    $LambdaNamespace, # Eg 'Simple.Lambda'
    [switch]
    $Rebuild # Rebuild the lambda demo zip file
)

function New-Lambda {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [Parameter(Mandatory=$true)]
        [string]
        $Namespace,
        [bool]
        $Rebuild
    )    
    $root = Join-Path -Path $PSScriptRoot -ChildPath AwsKickStarter.Lambda.Demo
    $zipPath = Join-Path -Path $root -ChildPath "bin/Release/net8.0/AwsKickStarter.Lambda.Demo.zip"
    if (-not (Test-Path $zipPath)) {
        $Rebuild = $true
    }
    if ($Rebuild) {
        Set-Location $root
        dotnet lambda package $zipPath --function-architecture arm64
    }

    Write-Host 'Remove existing lambda function'
    aws lambda delete-function --profile localstack --function-name $functionName --no-cli-pager 2> $null

    $handler = "AwsKickStarter.Lambda.Demo::$Namespace.MyLambda::Handler"
    Write-Host 'Create lambda function'
    Write-Host "Handler: $handler"
    Write-Host "ZipPath: $zipPath"
    aws lambda create-function --profile localstack --function-name $FunctionName --environment '{"Variables":{"ASPNETCORE_ENVIRONMENT":"Development","LAMBDA_NET_SERIALIZER_DEBUG":"true"}}' --runtime dotnet8 --architectures arm64 --zip-file fileb://$zipPath --handler $handler --role 'arn:aws:iam::000000000000:role/lambda-role' --memory-size 128 --timeout 30 --tracing-config Mode=PassThrough --no-cli-pager

    Write-Host 'Wait for the function to become active'
    do {
        $functionState = $(aws lambda get-function --profile localstack --function-name $functionName --no-cli-pager | ConvertFrom-Json).Configuration.State
        Write-Host "Function state: $functionState"
        if ($functionState -eq 'Active') {
            break
        }
        Start-Sleep -Seconds 2
    } while ($functionState -eq 'Pending')
}

function  Invoke-Lambda {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [string]
        $Payload,
        [bool]
        $WithOutput
    )
    $invocationType = if ($WithOutput) { 'RequestResponse' } else { 'Event' }
    if ($Payload) {
        Write-Host "Invoking lambda function $FunctionName as $invocationType with input: $Payload"
        $encodedPayload = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($Payload))
        $result = $(aws lambda invoke --profile localstack --function-name $FunctionName --invocation-type $invocationType --payload $encodedPayload lambda-out.txt --no-cli-pager) | ConvertFrom-Json
    }
    else {
        Write-Host "Invoking lambda function $FunctionName as $invocationType"
        $result = $(aws lambda invoke --profile localstack --function-name $FunctionName --invocation-type $invocationType lambda-out.txt --no-cli-pager) | ConvertFrom-Json
    }
    if (($result.StatusCode -eq 200) -or ($result.StatusCode -eq 202)) {
        Write-Host "Lambda invocation successful"
    } else {
        Write-Error "Lambda invocation failed with status code: $($result.StatusCode)"
    }
    if ($WithOutput) {
        $response = Get-Content lambda-out.txt
        Write-Host "Response: $response"
    }
}

function Get-CompressedString {
    param (
        [string]$UncompressedString
    )

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($UncompressedString)
    $outputStream = New-Object System.IO.MemoryStream
    $gzipStream = New-Object System.IO.Compression.GZipStream($outputStream, [System.IO.Compression.CompressionMode]::Compress)

    try {
        $gzipStream.Write($bytes, 0, $bytes.Length)
    } finally {
        $gzipStream.Close()
    }

    return [Convert]::ToBase64String($outputStream.ToArray())
}

function Publish-Message {
    param (
        [string]$TopicArn,
        [string]$Message,
        [switch]$Compress
    )

    if ($Compress) {
        $compressAttributes = '{"Content-Encoding":{"DataType":"String","StringValue":"gzip"}}'
        $compressed = Get-CompressedString $Message
        Write-Host "Publishing $Message as compressed message: $compressed"
        aws sns publish --profile localstack --topic-arn $TopicArn --message $compressed --message-attributes $compressAttributes --no-cli-pager
    }
    else {
        Write-Host "Publishing message: $Message"
        aws sns publish --profile localstack --topic-arn $TopicArn --message $Message --no-cli-pager
    }
}

function Send-Sqs {
    param (
        [string]$QueueUrl,
        [string]$Message,
        [switch]$Compress
    )

    if ($Compress) {
        $compressAttributes = '{"Content-Encoding":{"DataType":"String","StringValue":"gzip"}}'
        $compressed = Get-CompressedString $Message
        Write-Host "Sending $Message as compressed message: $compressed"
        aws sqs send-message --profile localstack --queue-url $QueueUrl --message-body $compressed --message-attributes $compressAttributes --no-cli-pager
    }
    else {
        Write-Host "Sending message: $Message"
        aws sqs send-message --profile localstack --queue-url $QueueUrl --message-body $Message --no-cli-pager
    }
}

function Start-LambdaSimpleDemo {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [Parameter(Mandatory=$true)]
        [string]
        $Namespace,
        [bool]
        $Rebuild
    )
    New-Lambda -FunctionName $FunctionName -Namespace $Namespace -Rebuild $Rebuild

    $withInput = $Namespace.Contains('LambdaIn')
    $withOutput = $Namespace.EndsWith('Out')

    if ($withInput) {
        Invoke-Lambda -FunctionName $FunctionName -Payload '{"Id":1,"Name":"Adam"}' -WithOutput $withOutput
        Invoke-Lambda -FunctionName $FunctionName -Payload '{"Id":2,"Name":"Barry"}' -WithOutput $withOutput
        Invoke-Lambda -FunctionName $FunctionName -Payload '{"Id":3,"Name":"Conor"}' -WithOutput $withOutput
        Invoke-Lambda -FunctionName $FunctionName -Payload '{"Id":4,"Name":"David"}' -WithOutput $withOutput
        Invoke-Lambda -FunctionName $FunctionName -Payload '{"Id":5,"Name":"Ethan"}' -WithOutput $withOutput
    }
    else {
        Invoke-Lambda -FunctionName $FunctionName -WithOutput $withOutput
    }
}

function Start-LambdaS3Demo {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [Parameter(Mandatory=$true)]
        [string]
        $Namespace,
        [bool]
        $Rebuild
    )

    $bucket = "$FunctionName-bucket"
    $region = $(aws configure get region --profile localstack)

    Write-Host 'Remove existing resources'
    $eventSources = $($(aws lambda list-event-source-mappings --profile localstack --function-name $FunctionName --no-cli-pager) | ConvertFrom-Json).EventSourceMappings
    foreach ($eventSource in $eventSources | Where-Object { $_.FunctionArn -match "function:$functionName$" }) {
        aws lambda delete-event-source-mapping --profile localstack --uuid $eventSource.UUID --no-cli-pager 2> $null
    }
    aws s3 rb "s3://$bucket" --profile localstack --no-cli-pager --force 2> $null

    New-Lambda -FunctionName $FunctionName -Namespace $Namespace -Rebuild $Rebuild

    Write-Host 'Create S3 bucket and upload file'
    aws s3 mb "s3://$bucket" --profile localstack --no-cli-pager

    Write-Host 'Create S3 event source for the lambda function'
    $configuration = '{"LambdaFunctionConfigurations": [{"LambdaFunctionArn": "arn:aws:lambda:' + $region + ':000000000000:function:' + $FunctionName + '", "Events": ["s3:ObjectCreated:*"], "Filter": {"Key": {"FilterRules": [{"Name": "prefix", "Value": "Uploads"}]}}}]}'
    aws s3api put-bucket-notification-configuration --profile localstack --bucket $bucket --region $region --notification-configuration $configuration

    Write-Host 'Upload file'
    $path = Join-Path $PSScriptRoot 'Readme.md'
    aws s3api put-object --profile localstack --bucket $bucket --key 'Uploads/Readme.md' --body $path --no-cli-pager
}

function Start-LambdaSnsDemo {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [Parameter(Mandatory=$true)]
        [string]
        $Namespace,
        [bool]
        $Rebuild
    )
    $topicName = "$FunctionName-sns"
    $region = $(aws configure get region --profile localstack)
    $topicArn = "arn:aws:sns:$($region):000000000000:$topicName"

    Write-Host 'Remove existing resources'
    $eventSources = $($(aws lambda list-event-source-mappings --profile localstack --function-name $FunctionName --no-cli-pager) | ConvertFrom-Json).EventSourceMappings
    foreach ($eventSource in $eventSources | Where-Object { $_.FunctionArn -match "function:$functionName$" }) {
        aws lambda delete-event-source-mapping --profile localstack --uuid $eventSource.UUID --no-cli-pager 2> $null
    }
    aws sns delete-topic --profile localstack --topic-arn $topicArn --no-cli-pager 2> $null

    New-Lambda -FunctionName $FunctionName -Namespace $Namespace -Rebuild $Rebuild
    $functionArn = $(aws lambda get-function --profile localstack --function-name $functionName --no-cli-pager | ConvertFrom-Json).Configuration.FunctionArn

    Write-Host 'Create SNS'
    aws sns create-topic --profile localstack --name $topicName --region $region --no-cli-pager

    Write-Host 'Create SNS event source for the lambda function'
    aws sns subscribe --profile localstack --topic-arn $topicArn --protocol lambda --region $region --notification-endpoint $functionArn --no-cli-pager

    Write-Host 'Publish messages'
    Publish-Message -TopicArn $topicArn -Message '{"Id":1,"Name":"Adam"}'
    Publish-Message -TopicArn $topicArn -Message '{"Id":2,"Name":"Barry"}' -Compress
    Publish-Message -TopicArn $topicArn -Message '{"Id":3,"Name":"Conor"}'
    Publish-Message -TopicArn $topicArn -Message '{"Id":4,"Name":"David"}' -Compress
    Publish-Message -TopicArn $topicArn -Message '{"Id":5,"Name":"Ethan"}'
}

function Start-LambdaSqsDemo {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $FunctionName,
        [Parameter(Mandatory=$true)]
        [string]
        $Namespace,
        [bool]
        $Rebuild
    )
    $topicName = "$FunctionName-sns"
    $queueName = "$FunctionName-sqs"
    $dlqueueName = "$queueName-dlq"
    $region = $(aws configure get region --profile localstack)
    $topicArn = "arn:aws:sns:$($region):000000000000:$topicName"
    $queueArn = "arn:aws:sqs:$($region):000000000000:$queueName"
    $dlqueueArn = "arn:aws:sqs:$($region):000000000000:$dlqueueName"
    $queueUrl = "http://sqs.$($region).localhost.localstack.cloud:4566/000000000000/$queueName"
    $dlqueueUrl = "http://sqs.$($region).localhost.localstack.cloud:4566/000000000000/$dlqueueName"

    Write-Host 'Remove existing resources'
    $eventSources = $($(aws lambda list-event-source-mappings --profile localstack --function-name $FunctionName --no-cli-pager) | ConvertFrom-Json).EventSourceMappings
    foreach ($eventSource in $eventSources | Where-Object { $_.FunctionArn -match "function:$functionName$" }) {
        aws lambda delete-event-source-mapping --profile localstack --uuid $eventSource.UUID --no-cli-pager 2> $null
    }
    aws sqs delete-queue --profile localstack --queue-url $queueUrl --no-cli-pager 2> $null
    aws sqs delete-queue --profile localstack --queue-url $dlqueueUrl --no-cli-pager 2> $null
    aws sns delete-topic --profile localstack --topic-arn $topicArn --no-cli-pager 2> $null

    New-Lambda -FunctionName $FunctionName -Namespace $Namespace -Rebuild $Rebuild

    Write-Host 'Create SQS/SNS'
    aws sns create-topic --profile localstack --name $topicName --region $region --no-cli-pager
    aws sqs create-queue --profile localstack --queue-name $dlqueueName --attributes VisibilityTimeout=60 --no-cli-pager
    $queueAttributes = '{"VisibilityTimeout":"60","RedrivePolicy":"{\"deadLetterTargetArn\":\"' + $dlqueueArn + '\",\"maxReceiveCount\":\"3\"}"}'
    aws sqs create-queue --profile localstack --queue-name $queueName --attributes $queueAttributes --no-cli-pager
    aws sqs get-queue-attributes --profile localstack --queue-url $queueUrl --attribute-names RedrivePolicy --no-cli-pager
    aws sns subscribe --profile localstack --topic-arn $topicArn --protocol sqs --notification-endpoint $queueArn --attributes RawMessageDelivery=true --no-cli-pager

    Write-Host 'Publish messages'
    Send-Sqs -QueueUrl $queueUrl -Message '{"Id":1,"Name":"Adam"}' -Compress
    Send-Sqs -QueueUrl $queueUrl -Message '{"Id":2,"Name":"Barry"}'
    Publish-Message -TopicArn $topicArn -Message '{"Id":3,"Name":"Conor"}' -Compress
    Publish-Message -TopicArn $topicArn -Message '{"Id":4,"Name":"David"}'
    Publish-Message -TopicArn $topicArn -Message '{"Id":5,"Name":"Ethan"}' -Compress

    Write-Host 'Create SQS event source for the lambda function'
    if ($LambdaNamespace.Contains('BatchResponse')) {
        aws lambda create-event-source-mapping --profile localstack --function-name $functionName --function-response-types ReportBatchItemFailures --batch-size 5 --maximum-batching-window-in-seconds 5 --event-source-arn $queueArn --no-cli-pager
    }
    else {
        aws lambda create-event-source-mapping --profile localstack --function-name $functionName --batch-size 5 --maximum-batching-window-in-seconds 5 --event-source-arn $queueArn --no-cli-pager
    }
}

$cwd = Get-Location
try {
    . $PSScriptRoot/Run-LocalStack.ps1

    $functionName = 'mylambda'
    if (!$LambdaNamespace.StartsWith('AwsKickStarter.Lambda.Demo.$LambdaNamespace')) {
        $LambdaNamespace = "AwsKickStarter.Lambda.Demo.$LambdaNamespace"
    }

    if ($LambdaNamespace.Contains('Simple')) {
        Start-LambdaSimpleDemo -FunctionName $functionName -Namespace $LambdaNamespace -Rebuild $Rebuild
    }
    elseif ($LambdaNamespace.Contains('S3')) {
        Start-LambdaS3Demo -FunctionName $functionName -Namespace $LambdaNamespace -Rebuild $Rebuild
    }
    elseif ($LambdaNamespace.Contains('Sns')) {
        Start-LambdaSnsDemo -FunctionName $functionName -Namespace $LambdaNamespace -Rebuild $Rebuild
    }
    elseif ($LambdaNamespace.Contains('Sqs')) {
        Start-LambdaSqsDemo -FunctionName $functionName -Namespace $LambdaNamespace -Rebuild $Rebuild
    }
}
finally {
    Set-Location $cwd
}
