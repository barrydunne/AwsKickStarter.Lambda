$queues = @(
    @{ Name = "mylambda-sqs"; Url = "http://sqs.eu-west-1.localhost.localstack.cloud:4566/000000000000/mylambda-sqs" },
    @{ Name = "mylambda-sqs-dlq"; Url = "http://sqs.eu-west-1.localhost.localstack.cloud:4566/000000000000/mylambda-sqs-dlq" }
)

$results = foreach ($queue in $queues) {
    $attributes = aws sqs get-queue-attributes --profile localstack --queue-url $queue.Url --attribute-names ApproximateNumberOfMessages ApproximateNumberOfMessagesNotVisible --no-cli-pager | ConvertFrom-Json | Select-Object -ExpandProperty Attributes
    
    [PSCustomObject]@{
        QueueName = $queue.Name
        Visible = $attributes.ApproximateNumberOfMessages
        NotVisible = $attributes.ApproximateNumberOfMessagesNotVisible
    }
}

$results | Format-Table -AutoSize